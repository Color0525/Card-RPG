using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillDatabase", menuName = "CreateSkillDatabase")]
public class NSkillDatabaseScriptable : ScriptableObject
{
    public enum ID
    {
        Attack,
        LastPowerAttack,
        Buff,
    }

    [SerializeField] ID m_id;
    [SerializeField] string m_name;
    [SerializeField] [Multiline(4)] string m_info;
    [SerializeField] int m_costSP;//増加クール
    public string Name { get { return m_name; } private set { m_name = value; } }
    public string Info { get { return m_info; } private set { m_info = value; } }
    public int CostSP { get { return m_costSP; } private set { m_costSP = value; } }

    //↓インスペクターで選択したスキルタイプによって変わるように
    [SerializeField] float m_powerRate;
    [SerializeField] string m_stateName;
    public string StateName { get { return m_stateName; } private set { m_stateName = value; } }

    public Action<BattleStatusControllerBase, BattleStatusControllerBase[]> Effect { get; set; }//m_idを変更した時ここに対応したスキル効果を入れる？

    private void OnValidate()
    {
        switch (m_id)
        {
            case ID.Attack:
                Effect = Attack;
                break;
            case ID.LastPowerAttack:
                Effect = LastPowerAttack;
                break;
            case ID.Buff:
                Effect = Buff;
                break;
            default:
                break;
        }
    }

    public void Attack(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        foreach (var target in targets)
        {
            target.Damage(actor.m_Power * m_powerRate);//使用者がtargetにm_powerRateでダメージを与える関数
            //怯み値の減少(ダメージメソッドにオーバーロードを追加？)
            //あるなら追加効果
        }
    }

    public static void LastPowerAttack(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        //UseSkill
        //m_powerRate＝使用者の体力に応じたパワー
        //使用者がtargetにm_powerRateでダメージを与える関数
    }

    public static void Buff(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数))
    {
        //UseSkill
        //target.全体攻撃UP()
    }

    void UseSkill(BattleStatusControllerBase actor)//これはunit側で？
    {
        //クールを増やす　//user.UseSP();
        //m_battleManager.ActionText(m_name); //スキル名を表示
        //m_anim.Play(skill.m_StateName);//アニメーション起動
    }
}
