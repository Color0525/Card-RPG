using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// コマンドボタンを管理
/// </summary>
public class CommandButtonController : MonoBehaviour
{ 
    [SerializeField] TextMeshProUGUI m_commandName = default;
    [SerializeField] TextMeshProUGUI m_costSP = default;
    [SerializeField] AudioClip m_selectSE = default;
    [SerializeField] AudioClip m_ngSE = default;

    //スキル
    SkillDatabase m_currentSkill;
    //行動者
    BattlePlayerController m_actor;
    //テキスト
    TextMeshProUGUI m_commandInfo;
    //アニメ
    Animator m_anim;
    //SE
    AudioSource m_audio;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// コマンドボタンをセットアップ
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="actor"></param>
    /// <param name="text"></param>
    public void SetupCammand(SkillDatabase skill, BattlePlayerController actor, TextMeshProUGUI text)
    {
        //N
        m_currentSkill = skill;
        m_actor = actor;
        m_commandInfo = text;
        m_commandName.text = skill.Name;
        m_costSP.text = skill.CostSP.ToString() + "SP";
    }

    /// <summary>
    /// ポイントされたとき呼ばれる
    /// </summary>
    public void ShowCommandInfo()
    {
        m_commandInfo.text = m_currentSkill.Info;
        m_anim.SetTrigger("Highlighted");
        PlaySE(m_selectSE);
    }
    
    /// <summary>
    /// ポイントが外れたとき呼ばれる
    /// </summary>
    public void HideCommandInfo()
    {
        m_commandInfo.text = null;
        m_anim.SetTrigger("Normal");
    }

    /// <summary>
    /// 押されたときコマンド使用
    /// </summary>
    public void PlayCommand()
    {
        if (m_currentSkill.CostSP <= m_actor.CurrentSP)
        {
            m_actor.PlayerActionCommand(m_currentSkill);
        }
        else
        {
            FindObjectOfType<BattleManager>().ShowActionText("SPが足りない！");
            PlaySE(m_ngSE);
        }
    }

    /// <summary>
    /// SEを再生
    /// </summary>
    /// <param name="se"></param>
    void PlaySE(AudioClip se)
    {
        m_audio.clip = se;
        m_audio.Play();
    }
}
