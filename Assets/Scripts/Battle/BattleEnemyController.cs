using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵戦闘時の行動
/// </summary>
public class BattleEnemyController : BattleStatusControllerBase
{
    [SerializeField] GameObject m_DeadParticlePrefab = default;

    /// <summary>
    /// 行動(enemy)
    /// </summary>
    public override void BeginAction()
    {
        base.BeginAction();
        EnemyAction();
    }
    /// <summary>
    /// 行動終了（enemy）
    /// </summary>
    public override void EndAction()
    {
        base.EndAction();
        //ActInterrupt();//できるなら行動割り込み
    }

    /// <summary>
    /// 行動(Enemy)
    /// </summary>
    void EnemyAction()
    {
        //N
        //持っているスキルからランダムに使用
        SkillDatabase skill = m_havesSkills[Random.Range(0, m_havesSkills.Length)];
        //対象範囲によってtargetを変える
        BattleStatusControllerBase[] targets = default;
        if (skill.Renge != SkillDatabase.TargetRenge.Myself)
        {
            //List<BattlePlayerController> players = BattleManager.Instance.PlayerUnits.Where;
            BattlePlayerController[] players = FindObjectsOfType<BattlePlayerController>().Where(x => x.IsAlive).ToArray();
            if (skill.Renge == SkillDatabase.TargetRenge.Single)
            {
                targets = new BattleStatusControllerBase[] { players[Random.Range(0, players.Length)] };
            }
            else if (skill.Renge == SkillDatabase.TargetRenge.Overall)
            {
                targets = players;
            }
        }
        else 
        {
            targets = new BattleStatusControllerBase[] { this };
        }
        UseSkill(skill, targets);
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
        Instantiate(m_DeadParticlePrefab, this.m_centerPosition.position, Quaternion.identity);
        this.gameObject.SetActive(false);
    }
}
