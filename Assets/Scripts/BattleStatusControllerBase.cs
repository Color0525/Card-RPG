﻿//using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 戦闘ユニットのステータス管理（継承用）
/// </summary>
public class BattleStatusControllerBase : MonoBehaviour
{
    //ステータス
    [SerializeField] string m_name;
    [SerializeField] int m_maxHP = 10;
    [SerializeField] int m_currentHP = 10;
    [SerializeField] int m_maxSP = 0;
    [SerializeField] int m_currentSP = 0;
    [SerializeField] int m_power = 3;
    //スキル
    [SerializeField] NSkillDatabaseScriptable[] m_havesSkills;
    //Nいらない？//NSkillDatabaseScriptable m_currentSkill;
    //アイコン等
    [SerializeField] StatusIconController m_statusIcon;
    [SerializeField] Transform m_hitParticlePosition;
    [SerializeField] Transform m_damageTextPosition;
    [SerializeField] GameObject m_damageTextPrefab;
    
    Animator m_anim;

    public static BattleManager m_battleManager;

    //プロパティ
    public string m_Name { get { return m_name; } private set { m_name = value; } }
    public int m_MaxHP { get { return m_maxHP; } private set { m_maxHP = value; } }
    public int m_CurrentHP { get { return m_currentHP; } private set { m_currentHP = value; } }
    public int m_MaxSP { get { return m_maxSP; } private set { m_maxSP = value; } }
    public int m_CurrentSP { get { return m_currentSP; } private set { m_currentSP = value; } }
    public int m_Power { get { return m_power; } private set { m_power = value; } }
    public NSkillDatabaseScriptable[] m_HavesSkills { get { return m_havesSkills; } private set { m_havesSkills = value; } }
    //N//public NSkillDatabaseScriptable m_CurrentSkill { get { return m_currentSkill; } set { m_currentSkill = value; } }


    //追加要素
    //public event Func<bool, int> ActionPoint;
    

    void Start()
    {
        m_battleManager = FindObjectOfType<BattleManager>();
        m_anim = GetComponent<Animator>();
        m_statusIcon.SetupStatus(this);
    }

    /// <summary>
    /// StatusIconをセット
    /// </summary>
    /// <param name="statusIconController"></param>
    public void SetStatusIcon(StatusIconController statusIconController)
    {
        m_statusIcon = statusIconController;
    }

    /// <summary>
    /// 行動開始
    /// </summary>
    public virtual void StartAction()
    {
        m_battleManager.StartActingTurn();
    }
    /// <summary>
    /// 行動終了
    /// </summary>
    void EndAction()
    {
        m_battleManager.EndActingTurn();
    }

    //N
    protected void UseSkill(NSkillDatabaseScriptable skill)
    {
        //選択機能ができたらここにm_CurrentSkill.Effect(this, targets);
        UseSP(skill.CostSP);//クールを増やすにする
        m_battleManager.ActionText(skill.Name); //スキル名を表示
        m_anim.Play(skill.StateName);//アニメーション起動
    }

    /// <summary>
    /// 指定したスキルのステートをプレイ
    /// </summary>
    /// <param name="sutateName"></param>
    public void PlayStateAnimator(SkillData skill)
    {
        m_battleManager.ActionText(skill.m_SkillName);
        m_anim.Play(skill.m_StateName);
    }
    public virtual void Hit(BattleStatusControllerBase target = null)// Attackアニメイベント 
    {
        //N
        //if (target)
        //{
        //    Instantiate(m_CurrentSkill.m_HitEffectPrefab, target.m_hitParticlePosition.position, m_CurrentSkill.m_HitEffectPrefab.transform.rotation);
        //    Attack(target, m_CurrentSkill.GetPowerRate(this));
        //}
    }
    public virtual void End()//Attackアニメイベント 
    {
        EndAction();
    }

    /// <summary>
    /// ダメージを与える
    /// </summary>
    /// <param name="target"></param>
    public void Attack(BattleStatusControllerBase target, float powerRate)
    {
        target.Damage(m_power * powerRate);
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="power"></param>
    public void Damage(float power)
    {
        int finalDamage = Mathf.CeilToInt(power);
        UpdateHP(-finalDamage);
        DamageText(m_damageTextPosition.position, finalDamage);

        if (m_currentHP == 0)
        {
            m_anim.SetBool("Dead", true);
            Death();
        }
        else
        {
            m_anim.SetTrigger("GetHit");//UPdateHPへ？
        }
    }

    /// <summary>
    /// 死亡する
    /// </summary>
    /// <param name="deadUnit"></param>
    public virtual void Death()
    {
        if (this.gameObject.GetComponent<BattleEnemyController>())
        {
            m_battleManager.DeleteEnemyList(this.gameObject.GetComponent<BattleEnemyController>());
        }
        else if (this.gameObject.GetComponent<BattlePlayerController>())
        {
            m_battleManager.DeletePlayerList(this.gameObject.GetComponent<BattlePlayerController>());
        }
    }

    /// <summary>
    /// SPを消費する
    /// </summary>
    /// <param name="cost"></param>
    public void UseSP(int cost)
    {
        UpdateSP(-cost);
    }

    /// <summary>
    /// HPを更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateHP(int value = 0)
    {
        int current = m_currentHP;
        m_currentHP = Mathf.Clamp(m_currentHP + value, 0, m_maxHP);
        if (current != m_currentHP)
        {
            m_statusIcon.UpdateHPBar(m_maxHP, m_currentHP);
        }
    }

    /// <summary>
    /// SPを更新
    /// </summary>
    /// <param name="value"></param>
    void UpdateSP(int value = 0)
    {
        int current = m_currentSP;
        m_currentSP = Mathf.Clamp(m_currentSP + value, 0, m_maxSP);
        if (current != m_currentSP)
        {
            m_statusIcon.UpdateSPBar(m_maxSP, m_currentSP);
        }
    }

    /// <summary>
    /// DamageTextを出す
    /// </summary>
    /// <param name="unitCanvas"></param>
    /// <param name="damage"></param>
    void DamageText(Vector3 unitCanvas, int damage)
    {
        GameObject go = Instantiate(m_damageTextPrefab, GameObject.FindWithTag("MainCanvas").transform);
        go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, unitCanvas);
        go.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
        Destroy(go, 1f);
    }
}
