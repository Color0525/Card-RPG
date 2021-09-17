//using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 戦闘ユニットのステータス管理（継承用）
/// </summary>
public abstract class BattleStatusControllerBase : MonoBehaviour
{
    //ステータス
    [SerializeField] string m_name = default;
    [SerializeField] int m_maxHP = 100;
    [SerializeField] int m_currentHP = 100;
    [SerializeField] int m_maxGuard = 100;
    [SerializeField] int m_currentGuard = 100;
    //[SerializeField] int m_maxSP = 0;
    //[SerializeField] int m_currentSP = 0;
    [SerializeField] int m_actionPoint = 0;
    [SerializeField] int m_coolTime = 0;
    [SerializeField] int m_attackPower = 20;
    List<float> m_attackPowerRate = new List<float>();
    [SerializeField] int m_defensePower = 20;
    List<float> m_defensePowerRate = new List<float>();
    bool m_alive = true;
    //状態効果
    List<StatusEffectDataBase> m_statesEffects = new List<StatusEffectDataBase>();
    Action m_timeElapsedStatusEffect = null;
    //スキル
    [SerializeField] protected SkillDatabase[] m_havesSkills;
    //Nいらない？//NSkillDatabaseScriptable m_currentSkill;
    Action m_activate = default;
    //アイコン等
    [SerializeField] protected StatusIconController m_statusIcon = default;
    [SerializeField] Sprite m_coolTimeIcon = default;
    //[SerializeField] Transform m_hitParticlePosition = default;
    [SerializeField] protected Transform m_centerPosition = default;
    [SerializeField] GameObject m_damageTextPrefab = default;

    Animator m_anim;

    //get set
    public string Name { get { return m_name; } }
    public int MaxHP { get { return m_maxHP; } }
    public int CurrentHP { get { return m_currentHP; } }
    public int MaxGuard { get { return m_maxGuard; } }
    public int CurrentGuard { get { return m_currentGuard; } }
    //public int MaxSP { get { return m_maxSP; } }
    //public int CurrentSP { get { return m_currentSP; } }
    public int ActionPoint { get { return m_actionPoint; } }
    public int CoolTime { get { return m_coolTime; } }
    public int AttackPower { get { return m_attackPower; } }
    public List<float> AttackPowerRate { get { return m_attackPowerRate; } }
    //public Func<int, float> FuncAttackPowerRate { get; set; } = n => n; 
    public int TotalAttackPower
    {
        get
        {
            float totalPower = m_attackPowerRate.Aggregate((float)m_attackPower, (result, n) => result * n);
            return Mathf.FloorToInt(totalPower);
        }
    }
    //public int DefensePower { get { return m_defensePower; } }
    public List<float> DefensePowerRate { get { return m_defensePowerRate; } }
    public int TotalDefensePower
    {
        get
        {
            float totalPower = m_defensePowerRate.Aggregate((float)m_defensePower, (result, n) => result * n);
            return Mathf.FloorToInt(totalPower);
        }
    }
    public bool Alive { get { return m_alive; } }
    public Action TimeElapsedStatusEffect { get { return m_timeElapsedStatusEffect; } set { m_timeElapsedStatusEffect = value; } }
    public SkillDatabase[] HavesSkills { get { return m_havesSkills; } }
    //N//public NSkillDatabaseScriptable m_CurrentSkill { get { return m_currentSkill; } set { m_currentSkill = value; } }
    public Sprite CoolTimeIcon { get { return m_coolTimeIcon; } }
    public Transform CenterPosition { get { return m_centerPosition; } }

    void Start()
    {
        m_anim = GetComponent<Animator>();
    }

    /// <summary>
    /// ステータスアイコン等をセットアップ
    /// </summary>
    /// <param name="coolTimePanel"></param>
    public virtual void SetupIcon(GameObject coolTimePanel)
    {
        m_statusIcon.SetupStatus(this, coolTimePanel);
    }

    /// <summary>
    /// 行動開始
    /// </summary>
    public virtual void BeginAction()
    {
        //N//m_battleManager.BeginAction();//GM側でいい?

        //ダウン状態なら解除して怯み値を戻す
        StatusEffectDataBase down = m_statesEffects.SingleOrDefault(x => x is Down);
        if (down != null)
        {
            RemoveStatesEffect(down);
            UpdateGuardValue(+m_maxGuard);
        }
        //NStatusEffectDataBase a = null;
        //foreach (var statesEffect in m_statesEffects)
        //{
        //    if (statesEffect is Down)
        //    {
        //        a = statesEffect;
        //        break;
        //    }
        //}
        //if (a != null) RemoveStatesEffect(a);
    }
    /// <summary>
    /// 行動終了
    /// </summary>
    public virtual void EndAction()
    {

    }

    //N
    /// <summary>
    /// スキルを使う
    /// </summary>
    /// <param name="skill"></param>
    protected void UseSkill(SkillDatabase skill, BattleStatusControllerBase[] targets)
    {
        m_activate = () => skill.Effect(this, targets);
        //UpdateSP(-skill.CostSP);
        UpdateCoolTimeValue(skill.CostTime);
        BattleManager.Instance.ShowActionText(skill.Name); //スキル名を表示
        m_anim.Play(skill.StateName);//アニメーション起動
    }

    ///// <summary>
    ///// 指定したスキルのステートをプレイ
    ///// </summary>
    ///// <param name="sutateName"></param>
    //public void PlayStateAnimator(SkillData skill)
    //{
    //    BattleManager.Instance.ShowActionText(skill.m_SkillName);
    //    m_anim.Play(skill.m_StateName);
    //}
    public void Activate()// Attackアニメイベント 
    {
        //N　
        m_activate();
        //if (target)
        //{
        //    Instantiate(m_CurrentSkill.m_HitEffectPrefab, target.m_hitParticlePosition.position, m_CurrentSkill.m_HitEffectPrefab.transform.rotation);
        //    Attack(target, m_CurrentSkill.GetPowerRate(this));
        //}
    }
    public void End()//Attackアニメイベント 
    {
        StartCoroutine(BattleManager.Instance.ReturnWaitTime());
    }

    /// <summary>
    /// CT以上のAPがあるなら、CTを0にしてAPをその分減らし、行動割り込みをする
    /// </summary>
    protected void ActInterrupt()
    {
        if (m_coolTime > 0 && m_coolTime <= m_actionPoint)
        {
            UpdateActionPointValue(-m_coolTime);
            UpdateCoolTimeValue(-m_coolTime);
            BattleManager.Instance.AddPriorityUnit(this);
        }
    }

    //N
    ///// <summary>
    ///// ダメージを与える////このタイミングでなにかするなら必要
    ///// </summary>
    ///// <param name="target"></param>
    //public void Attack(BattleStatusControllerBase target, float powerRate)
    //{
    //    target.Damage(m_power * powerRate);
    //}
    /// <summary>
    /// 最終的に受けるダメージ量を返す
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="enemyTotalattackPower"></param>
    /// <returns></returns>
    public int GetReceiveDamage(float damage, int enemyTotalattackPower)
    {
        //耐性による倍率も追加したい
        float rateByStatus = (float)Mathf.Max(1, enemyTotalattackPower) / Mathf.Max(1, TotalDefensePower);//ステータスによる倍率//値が大きすぎるのを防ぐ＆0除算にならないように最低1
        return Mathf.CeilToInt(damage * rateByStatus);//最小でも1ダメージは与えたいので切り上げ
    }
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="value"></param>
    public void Damage(int value)
    {
        UpdateHP(-value);
        DamageText(m_centerPosition.position, value);

        if (m_currentHP == 0)
        {
            m_anim.SetBool("Dead", true);//  Death()に入れる？
            Death();
        }
        else
        {
            m_anim.SetTrigger("GetHit");
        }
    }
    /// <summary>
    /// ガードの減少
    /// </summary>
    /// <param name="value"></param>
    public void DecreaseGuardValue(int value)
    {
        if (m_currentGuard == 0) return;//すでに0なら減少しない

        UpdateGuardValue(-value);

        if (m_currentGuard == 0)
        {
            AddStatesEffect(new Down(this));//NStatusEffectDataBase.ID.Down);//ダウンする
            //ダウンアニメ//割れるパーティクル演出
        }
    }
    ///// <summary>
    ///// SPが減少
    ///// </summary>
    ///// <param name="cost"></param>
    //public void UseSP(int cost)
    //{
    //    UpdateSP(-cost);
    //}
    /// <summary>
    /// クールタイム増加
    /// </summary>
    /// <param name="value"></param>
    public void IncreaseCoolTime(int value)
    {
        UpdateCoolTimeValue(value);
    }

    /// <summary>
    /// 時間経過
    /// </summary>
    public void TimeElapsed()
    {
        UpdateCoolTimeValue(-1);
        UpdateActionPointValue(1);
        if (m_timeElapsedStatusEffect != null)
        {
            m_timeElapsedStatusEffect();//状態変化listの経過時間も減らす
        }
    }

    /// <summary>
    /// HPを更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateHP(int value = 0)
    {
        int before = m_currentHP;
        m_currentHP = Mathf.Clamp(m_currentHP + value, 0, m_maxHP);
        if (before != m_currentHP)
        {
            m_statusIcon.UpdateHPBar(m_maxHP, m_currentHP);
        }
    }
    /// <summary>
    /// 怯み値を更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateGuardValue(int value = 0)
    {
        int before = m_currentGuard;
        m_currentGuard = Mathf.Clamp(m_currentGuard + value, 0, m_maxGuard);
        if (before != m_currentGuard)
        {
            m_statusIcon.UpdateGuardBar(m_maxGuard, m_currentGuard);//0ならゲージが破壊演出つけたい
        }
    }
    ///// <summary>
    ///// SPを更新
    ///// </summary>
    ///// <param name="value"></param>
    //void UpdateSP(int value = 0)
    //{
    //    int before = m_currentSP;
    //    m_currentSP = Mathf.Clamp(m_currentSP + value, 0, m_maxSP);
    //    if (before != m_currentSP)
    //    {
    //        m_statusIcon.UpdateSPBar(m_maxSP, m_currentSP);
    //    }
    //}
    /// <summary>
    /// アクションポイントを更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateActionPointValue(int value = 0)
    {
        int before = m_actionPoint;
        m_actionPoint += value;
        if (before != m_actionPoint)
        {
            m_statusIcon.UpdateActionPointDisplay(m_actionPoint);
        }
    }
    /// <summary>
    /// クールタイムを更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateCoolTimeValue(int value = 0)
    {
        int before = m_coolTime;
        m_coolTime += value;
        if (before != m_coolTime)
        {
            m_statusIcon.UpdateCoolTimeBar(m_coolTime);
        }
    }

    /// <summary>
    /// 状態効果を新しく持つ
    /// </summary>
    /// <param name="effect"></param>
    public void AddStatesEffect(StatusEffectDataBase effect)
    {
        //NStatusEffectDataBase effect = new NStatusEffectDataBase(this, id, effectTime, effectPower);
        effect.AddEffect();
        m_statesEffects.Add(effect);
        m_statusIcon.AddStatusEffectDisplay(effect);
        //＜状態効果によるアイコンやパーティクル表示、アニメ変更
    }
    /// <summary>
    /// 状態効果を削除する
    /// </summary>
    /// <param name="effect"></param>
    public void RemoveStatesEffect(StatusEffectDataBase effect)
    {
        effect.RemoveEffect();
        m_statesEffects.Remove(effect);
        m_statusIcon.RemoveStatusEffectDisplay(effect);
    }

    /// <summary>
    /// 死亡する
    /// </summary>
    /// <param name="deadUnit"></param>
    public virtual void Death() //Damageメソッド内に入れる？
    {
        m_alive = false;
        m_statusIcon.HideCoolTimeBar();
        BattleManager.Instance.CheckWinOrLose(this);

        //if (this.gameObject.GetComponent<BattleEnemyController>())
        //{
        //    m_battleManager.DeleteEnemyList(this.gameObject.GetComponent<BattleEnemyController>());
        //}
        //else if (this.gameObject.GetComponent<BattlePlayerController>())
        //{
        //    m_battleManager.DeletePlayerList(this.gameObject.GetComponent<BattlePlayerController>());
        //}
    }

    /// <summary>
    /// DamageTextを出す
    /// </summary>
    /// <param name="unitCenterPos"></param>
    /// <param name="damage"></param>
    public void DamageText(Vector3 unitCenterPos, int damage)
    {
        GameObject go = Instantiate(m_damageTextPrefab, GameObject.FindWithTag("MainCanvas").transform);
        //Vector3 nearRamdom = new Vector3(unitCenterPos.x + Random.Range(-0.5f, 0.5f), unitCenterPos.y + Random.Range(-0.5f, 0.5f), unitCenterPos.z + Random.Range(-0.5f, 0.5f));
        go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, unitCenterPos);
        go.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
        Destroy(go, 1f);
    }
}
