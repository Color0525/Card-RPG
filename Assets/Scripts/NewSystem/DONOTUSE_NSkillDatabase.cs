using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DONOTUSE_NSkillDatabase : MonoBehaviour
{
    public enum ID
    {
        Attack,
        LastPowerAttack,
        Buff,
    }

    public class Skill
    {
        public ID m_id;
        [Multiline(4)] string m_info;
        int m_costSP;
        float m_powerRate;
        public Action<BattleStatusControllerBase, BattleStatusControllerBase[]> m_effect;
        
        public Skill(ID name, string info, int costSP, float powerRate, Action<BattleStatusControllerBase, BattleStatusControllerBase[]> effect)
        {
            //代入
        }
    }

    public Skill[] skills = 
    { 
        new Skill(ID.Attack, "通常攻撃", 0, 1, Attack),
        //LastPowerAttack
        //Buff
    };

    public Action<BattleStatusControllerBase, BattleStatusControllerBase[]> GetSkillEffect(ID id)　　//もしくはskills[id].m_effectで、呼ぶ
    {
        foreach (var skill in skills)
        {
            if (id == skill.m_id)
            {
                return skill.m_effect;
            }
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
