using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillDatabase", menuName = "CreateSkillDatabase")]
public class SkillDatabase : ScriptableObject
{
    public enum ID
    {
        //攻撃系0～99
        Attack = 10,
        ShockAttack = 20,
        LastPowerAttack = 30,

        //補助系100～199
        AttackUp = 100,
        Poison = 150,

        //常時系200～
    }

    [SerializeField] ID m_id = default;
    [SerializeField] string m_name = "None";
    [SerializeField] [Multiline(4)] string m_info = default;
    [SerializeField] int m_costSP = 0;
    [SerializeField] int m_costTime = 100;

    //↓インスペクターで選択したスキルタイプ(ID)によって変わるように？
    //攻撃系なら有り
    [SerializeField] float m_damageRate = 1;
    [SerializeField] int m_breakPower = 0;
    //状態効果系なら有り
    [SerializeField] int m_effectTime = -1;
    [SerializeField] float m_effectRete = 0;

    [SerializeField] string m_stateName = default;

    Action<BattleStatusControllerBase, BattleStatusControllerBase[]> m_effect = default;

    //get,set
    public string Name { get { return m_name; } }
    public string Info { get { return m_info; } }
    public int CostSP { get { return m_costSP; } }
    public int CostTime { get { return m_costTime; } }
    public string StateName { get { return m_stateName; } }
    public Action<BattleStatusControllerBase, BattleStatusControllerBase[]> Effect { get { return m_effect; } }//m_idを変更した時ここに対応したスキル効果を入れる？

    private void OnValidate()
    {
        switch (m_id)
        {
            case ID.Attack:
                m_effect = Attack;
                break;
            case ID.ShockAttack:
                m_effect = ShockAttack;
                break;
            case ID.LastPowerAttack:
                m_effect = LastPowerAttack;
                break;
            case ID.AttackUp:
                m_effect = AttackUp;
                break;
            case ID.Poison:
                m_effect = Poison;
                break;
            default:
                break;
        }
    }

    public void Attack(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        foreach (var target in targets)
        {
            //ダメージ関数などの計算構成は変えるかも
            //int a = Mathf.FloorToInt(actor.FuncAttackPowerRate(actor.AttackPower));
            target.Damage(target.GetReceiveDamage(actor.AttackPower * m_damageRate, actor.TotalAttackPower));//使用者がtargetにm_powerRateでダメージを与える関数
            //あるなら追加効果
            //UseSkill
        }
    }

    public void ShockAttack(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        foreach (var target in targets)
        {
            target.Damage(target.GetReceiveDamage(actor.AttackPower * m_damageRate, actor.TotalAttackPower));//使用者がtargetにm_powerRateでダメージを与える関数
            target.DecreaseGuardValue(m_breakPower);//怯み削り量はダメージに比例させる？
        }
    }

    public void LastPowerAttack(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数)
    {
        float specialPowerRate = (float)actor.MaxHP / actor.CurrentHP;//使用者の体力割合が少ないほど威力倍率が高くなる
        foreach (var target in targets)
        {
            target.Damage(target.GetReceiveDamage(actor.AttackPower * specialPowerRate, actor.TotalAttackPower));
        }
    }

    public void AttackUp(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数))
    {
        actor.AddStatesEffect(new AttackUp(m_effectRete, actor, m_effectTime));
    }
    public void Poison(BattleStatusControllerBase actor, BattleStatusControllerBase[] targets)//スキル選択時にそのスキルが選択系ならtarget選択関数))
    {
        foreach (var target in targets)
        {
            target.AddStatesEffect(new Poison(target, m_effectTime));
        }
    }

    void UseSkill(BattleStatusControllerBase actor)//これはunit側で
    {
        //クールを増やす　//user.UseSP();
        //m_battleManager.ActionText(m_name); //スキル名を表示
        //m_anim.Play(skill.m_StateName);//アニメーション起動
    }
}
