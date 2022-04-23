using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// コマンドボタンを管理
/// </summary>
public class CommandButtonController : MonoBehaviour
{
    static List<CommandButtonController> m_othersCommandButton = new List<CommandButtonController>();
    static public List<CommandButtonController> OthersCommandButton => m_othersCommandButton;

    [SerializeField] TextMeshProUGUI m_commandName = default;
    [SerializeField] TextMeshProUGUI m_coolTime = default;
    //[SerializeField] TextMeshProUGUI m_costSP = default;
    [SerializeField] AudioClip m_selectSE = default;
    //[SerializeField] AudioClip m_ngSE = default;

    //スキル
    SkillDatabase m_currentSkill;
    //行動者
    //BattlePlayerController m_actor;
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
    public void SetupCammand(SkillDatabase skill, /*BattlePlayerController actor,*/ TextMeshProUGUI text)
    {
        //N
        m_currentSkill = skill;
        //m_actor = actor;
        m_commandInfo = text;
        m_commandName.text = skill.Name;
        m_coolTime.text = skill.CostTime.ToString();
        //m_costSP.text = skill.CostSP.ToString() + "SP";
        m_othersCommandButton.Add(this);
    }

    /// <summary>
    /// ポイントされたとき呼ばれ、コマンドの説明＆強調を表示
    /// </summary>
    public void ShowCommandInfo()
    {
        m_othersCommandButton.ForEach(x => x.HideCommandInfo());//すべて非選択に

        m_commandInfo.text = m_currentSkill.Info;//これだけ選択
        //m_anim.SetTrigger("Highlighted");
        m_anim.SetBool("Show", true);
        PlaySE(m_selectSE);
    }

    /// <summary>
    /// ポイントが外れたとき呼ばれ、コマンドの説明＆強調を非表示
    /// </summary>
    public void HideCommandInfo()
    {
        m_commandInfo.text = null;
        //m_anim.SetTrigger("Normal");
        m_anim.SetBool("Show", false);
    }

    /// <summary>
    /// 押されたときコマンド使用
    /// </summary>
    public void PlayCommand()
    {
        //BattleManager.Instance.BeginTargetSelect(c);

        Debug.Log("BattleManager.Instance.BeginPlayerTargetSelect(m_currentSkill);");

        //if (m_currentSkill.CostSP <= m_actor.CurrentSP)
        //{
        //    BattleManager.Instance.BeginPlayerTargetSelect(m_currentSkill);

        //    //m_actor.SelectSkillTarget(m_currentSkill);
        //    //m_currentCommandSkill = skill;
        //}
        //else
        //{
        //    BattleManager.Instance.ShowActionText("SPが足りない！");
        //    PlaySE(m_ngSE);
        //}
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
