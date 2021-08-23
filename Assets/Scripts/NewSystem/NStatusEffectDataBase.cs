using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NStatusEffectDataBase : MonoBehaviour
{
    public enum StatusEffect
    {
        AttackUp,
    }

    string m_name;
    int m_remainingTurn;
    BattleStatusControllerBase m_target;
    Action m_addEffect;
    Action m_removeEffect;

    NStatusEffectDataBase(int remainingTurn, BattleStatusControllerBase target, StatusEffect statusEffect)
    {
        m_remainingTurn = remainingTurn;
        m_target = target;
        if (statusEffect == StatusEffect.AttackUp)
        {
            m_addEffect = AddAttackUp;
            m_removeEffect = RemoveAttackUp;
        }
    }
    void TurnProgress()
    {
        m_remainingTurn--;
        if (m_remainingTurn <= 0)
        {
            Destroy(this);
        }
    }
    
    void AddAttackUp()
    {
        //m_target.m_currentPower += m_target.m_Power * 0.5;
    }
    void RemoveAttackUp()
    {
        //m_target.m_currentPower -= m_target.m_Power * 0.5;
    }

    private void OnEnable()
    {
        m_addEffect();
        //m_target.ターン終了デリゲート += TurnProgress;
    }
    private void OnDisable()
    {
        m_removeEffect();
        //m_target.ターン終了デリゲート -= TurnProgress;
    }
}
