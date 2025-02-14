﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffectDataBase
{
    protected BattleStatusControllerBase m_target = default;
    protected int m_effectTime = -1;
    protected string m_effectName = null;
    protected Action m_addEffect = default;
    protected Action m_removeEffect = default;
    public string EffectName { get { return m_effectName; } }
    public Action AddEffect { get { return m_addEffect; } }
    public Action RemoveEffect { get { return m_removeEffect; } }

    protected StatusEffectDataBase(BattleStatusControllerBase target) => m_target = target;
    //{
    //    m_target = target;
    //    if (effectTime > 0)
    //    {
    //        m_effectTime = effectTime;
    //        m_addEffect += () => m_target.TimeElapsedStatusEffect += TimeElapsed;
    //        m_removeEffect += () => m_target.TimeElapsedStatusEffect -= TimeElapsed;
    //    }
    //}
    /*
    public enum ID
    {
        Down,
        AttackUp,
        Poison,
    }
    public NStatusEffectDataBase(BattleStatusControllerBase target, ID id, int effectTime = -1, float effectPower = 0)
    {
        switch (id)
        {
            case ID.Down:
                m_target = target;
                //m_effectTime = effectTime;
                //m_effectPower = effectPower;
                AddEffect += () =>
                {
                    m_target.IncreaseCoolTime(30);
                    m_target.DefensePowerRate.Add(0.5f);
                };
                RemoveEffect += () => m_target.DefensePowerRate.Remove(0.5f);
                break;
            case ID.AttackUp:
                m_target = target;
                m_effectTime = effectTime;
                m_effectPower = effectPower;
                AddEffect += AddAttackUp;
                RemoveEffect += RemoveAttackUp;
                break;
            case ID.Poison:
                m_target = target;
                m_effectTime = effectTime;
                //m_effectPower = effectPower;
                AddEffect += () =>
                {
                    m_target.TimeElapsedStatusEffect += PoisonDamage;
                    m_target.TimeElapsedStatusEffect += TimeElapsed;
                };
                RemoveEffect += () =>
                {
                    m_target.TimeElapsedStatusEffect -= PoisonDamage;
                    m_target.TimeElapsedStatusEffect -= TimeElapsed;
                };
                break;
            default:
                break;
        }
    }
    */
    protected void SetEffectTime(int effectTime)
    {
        m_effectTime = effectTime;
        m_addEffect += () => m_target.TimeElapsedStatusEffect += TimeElapsed;
        m_removeEffect += () => m_target.TimeElapsedStatusEffect -= TimeElapsed;
    }
    void TimeElapsed()
    {
        m_effectTime--;
        if (m_effectTime == 0) m_target.RemoveStatesEffect(this);
    }
    //public abstract void AddEffect();
    //public abstract void RemoveEffect();
}

//効果時間無し
public class Down : StatusEffectDataBase
{
    public Down(BattleStatusControllerBase target) : base(target)
    {
        m_effectName = "ダウン";
        m_addEffect += () =>
        {
            m_target.IncreaseCoolTime(30);
            m_target.DefensePowerRate.Add(0.5f);
        };
        m_removeEffect += () => m_target.DefensePowerRate.Remove(0.5f);
    }
    //public override void AddEffect()
    //{
    //    m_target.IncreaseCoolTime(30);
    //    m_target.DefensePowerRate.Add(0.5f);
    //}
    //public override void RemoveEffect() => m_target.DefensePowerRate.Remove(0.5f);
}

//効果時間有り
//public class InTimeStatusEffectDataBase : NStatusEffectDataBase
//{
//    protected int m_effectTime = -1;
//    protected InTimeStatusEffectDataBase(BattleStatusControllerBase target, int effectTime): base(target)
//    {
//        m_effectTime = effectTime;
//        m_addEffect += () => m_target.TimeElapsedStatusEffect += TimeElapsed;
//        m_removeEffect += () => m_target.TimeElapsedStatusEffect -= TimeElapsed;
//    }
//    protected void TimeElapsed()
//    {
//        m_effectTime--;
//        if (m_effectTime == 0) m_target.RemoveStatesEffect(this);
//    }
//}

public class Poison : StatusEffectDataBase
{
    public Poison(BattleStatusControllerBase target, int effectTime = -1) : base(target)
    {
        m_effectName = "毒";
        m_addEffect += () => m_target.TimeElapsedStatusEffect += PoisonDamage;
        m_removeEffect += () => m_target.TimeElapsedStatusEffect -= PoisonDamage;
        SetEffectTime(effectTime);
    }
    void PoisonDamage()//10カウント毎に、最大体力の5％分のダメージ
    {
        if (m_effectTime % 10 == 0) { m_target.Damage(Mathf.CeilToInt(m_target.MaxHP * 0.05f)); }
    }
}

public class AttackUp : StatusEffectDataBase
{
    //float m_effectRate = 1;
    public AttackUp(float effectRate, BattleStatusControllerBase target, int effectTime = -1) : base(target)
    {
        m_effectName = $"攻撃力{effectRate}倍";
        m_addEffect += () => m_target.AttackPowerRate.Add(effectRate);
        m_removeEffect += () => m_target.AttackPowerRate.Remove(effectRate);
        SetEffectTime(effectTime);

        //m_effectRate = effectRate;
        //m_addEffect += () => m_target.FuncAttackPowerRate += GetMultipliedValue;
        //m_removeEffect += () => m_target.FuncAttackPowerRate -= GetMultipliedValue;
    }
    //float GetMultipliedValue(int origin) => origin * m_effectRate;
}