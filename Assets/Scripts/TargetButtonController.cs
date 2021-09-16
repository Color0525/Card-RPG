using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetButtonController : MonoBehaviour
{
    static List<TargetButtonController> m_othersTargetButton = new List<TargetButtonController>();
    static public List<TargetButtonController> OthersTargetButton => m_othersTargetButton;

    [SerializeField] GameObject m_targetingImage = default;
    bool m_selected = true;
    BattleStatusControllerBase m_thisUnit;
    bool m_canSelect = false;
    //List<TargetButtonController> m_otherTargetButton = null;

    public bool Selected => m_selected;
    public BattleStatusControllerBase ThisUnit => m_thisUnit;

    //[SerializeField] AudioClip m_selectSE = default;
    //AudioSource m_audio;


    private void Start()
    {
        //m_audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// ターゲットボタンをセットアップ
    /// </summary>
    /// <param name="thisUnit"></param>
    public void SetupTarget(BattleStatusControllerBase thisUnit, bool canSelect)// List<TargetButtonController> otherTargetButton = null)
    {
        m_thisUnit = thisUnit;
        m_canSelect = canSelect;
        m_othersTargetButton.Add(this);
        //m_otherTargetButton = otherTargetButton;
    }

    /// <summary>
    /// ポイントされたとき呼ばれ,選択状態にする
    /// </summary>
    public void SelectTarget()
    {
        //if (m_otherTargetButton != null)//他ターゲットボタンのリストを持っているなら
        if (m_canSelect)
        {
            m_othersTargetButton.ForEach(x => x.NotSelectTarget());//全解除

            m_selected = true;//このオブジェクトだけ選択
            m_targetingImage.gameObject.SetActive(true);
            //m_anim.SetTrigger("Highlighted");
            //PlaySE(m_selectSE);
        }
    }
    /// <summary>
    /// ポイントが外れたとき呼ばれ、選択状態を解除する
    /// </summary>
   　public void NotSelectTarget()
    {
        if (m_canSelect)
        {
            m_selected = false;
            m_targetingImage.gameObject.SetActive(false);
            //m_anim.SetTrigger("Normal");
        }
    }
    ///// <summary>
    ///// ポイントが外れたとき呼ばれる
    ///// </summary>
    //public void NotSelectTarget()
    //{
    //    m_selected = false;
    //    m_targetingImage.gameObject.SetActive(false);
    //    //m_anim.SetTrigger("Normal");
    //}

    /// <summary>
    /// 押してターゲット決定
    /// </summary>
    public void DecideTarget()
    {
        BattleManager.Instance.PlayPlayerAction();
    }

    ///// <summary>
    ///// SEを再生
    ///// </summary>
    ///// <param name="se"></param>
    //void PlaySE(AudioClip se)
    //{
    //    m_audio.clip = se;
    //    m_audio.Play();
    //}
}
