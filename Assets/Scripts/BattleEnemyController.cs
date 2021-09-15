using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵戦闘時の行動
/// </summary>
public class BattleEnemyController : BattleStatusControllerBase
{
    [SerializeField] GameObject m_DeadParticlePrefab = default;

    /// <summary>
    /// 行動(敵)
    /// </summary>
    public override void BeginAction()
    {
        base.BeginAction();
        EnemyAction();
    }

    /// <summary>
    /// 行動(Enemy)
    /// </summary>
    void EnemyAction()
    {
        //N
        //持っているスキルからランダムに使用
        SkillDatabase skill = m_havesSkills[Random.Range(0, m_havesSkills.Length)];
        ////UseSP(m_CurrentSkill.m_CostSP); // 敵はSP消費なし
        skill.Effect(this, FindObjectsOfType<BattlePlayerController>());
        UseSkill(skill);
        //PlayStateAnimator(m_CurrentSkill);
    }

    //public override void Death(BattleStatusControllerBase deadUnit)
    //{
    //    base.Death(deadUnit);
    //    if (m_questTarget)
    //    {
    //        SceneController.m_Instance.AddQuestCount();
    //    }
    //}

    // アニメイベント
    //N
    //public override void Hit(BattleStatusControllerBase target = null)
    //{
    //    BattlePlayerController thisTarget = FindObjectOfType<BattlePlayerController>();
    //    base.Hit(thisTarget);
    //}

    void Dead()//共通にして、敗カットシーンはカメラ移動だけ？
    {
        Instantiate(m_DeadParticlePrefab, this.gameObject.transform.position, Quaternion.identity);
        this.gameObject.SetActive(false);
    }
}
