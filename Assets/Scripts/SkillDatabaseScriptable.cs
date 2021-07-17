using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillDatabase", menuName = "CreateSkillDatabase")]
public class SkillDatabaseScriptable : ScriptableObject
{
    public enum ID
    {
        Attack,
        LastPowerAttack,
        Buff,
    }

    [SerializeField] ID m_id;
    [SerializeField] [Multiline(4)] string m_info;
    [SerializeField] int m_costSP;
    [SerializeField] float m_powerRate;

    public Action<BattleStatusControllerBase, BattleStatusControllerBase[]> GetSkillEffect(ID id)//プロパティにする？
    {
        if (id == ID.Attack)
        {
            return Attack;
        }
        if (id == ID.LastPowerAttack)
        {
            return LastPowerAttack;
        }
        if (id == ID.Buff)
        {
            return Buff;
        }
        return null;
    }

    public static void Attack(BattleStatusControllerBase actor, BattleStatusControllerBase[] target)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        //UseSkill
        //使用者がtargetにm_powerRateでダメージを与える関数
    }

    public static void LastPowerAttack(BattleStatusControllerBase actor, BattleStatusControllerBase[] target)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        //UseSkill
        //m_powerRate＝使用者の体力に応じたパワー
        //使用者がtargetにm_powerRateでダメージを与える関数
    }

    public static void Buff(BattleStatusControllerBase actor, BattleStatusControllerBase[] target)//スキル選択時にそのスキルが選択系ならtarget選択関数))
    {
        //UseSkill
        //target.全体攻撃UP()
    }

    void UseSkill(BattleStatusControllerBase user)
    {
        //m_name[]を表示
        //user.UseSP();
    }
}
