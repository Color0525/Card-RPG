using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetButtonController : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] GameObject m_targetingImageObj = default;
    //bool m_changeable = false;
    bool m_selected = false;
    BattleStatusControllerBase m_thisUnit;
    Action m_clickEvent = default;
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
    public void Setup(Vector2 position, BattleStatusControllerBase thisUnit, bool selected, Action clickEvent)// List<TargetButtonController> otherTargetButton = null)
    {
        GetComponent<RectTransform>().position = position;
        m_thisUnit = thisUnit;
        m_targetingImageObj.gameObject.SetActive(selected);
        m_selected = selected;
        m_clickEvent = clickEvent;
        //m_changeable = changeable;
        //m_otherTargetButton = otherTargetButton;
    }

    /// <summary>
    /// ポイントされたとき呼ばれ,ターゲット表示する
    /// </summary>
    public void SelectTarget()
    {
        //if (m_otherTargetButton != null)//他ターゲットボタンのリストを持っているなら
        if (!m_selected)
        {
            //TargetManager.Instance.TargetButton.ForEach(x => x.NotSelectTarget());//全解除

            //m_selected = true;//このオブジェクトだけ選択
            m_targetingImageObj.gameObject.SetActive(true);
            //m_anim.SetTrigger("Highlighted");
            //PlaySE(m_selectSE);
        }
    }
    /// <summary>
    /// ポイントが外れたとき呼ばれ、ターゲット表示を解除する
    /// </summary>
    public void NotSelectTarget()
    {
        if (!m_selected)
        {
            //m_selected = false;
            m_targetingImageObj.gameObject.SetActive(false);
            //m_anim.SetTrigger("Normal");
        }
    }
    /// <summary>
    /// 押したとき呼ばれ、選択を確定し、クリックイベントを呼ぶ
    /// </summary>
    public void DecideTarget()
    {
        m_selected = true;
        m_clickEvent();
        //TargetManager.Instance.FinishTarget();
        //BattleManager.Instance.PlayPlayerAction();//targetControllerに返す？
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    SelectTarget();
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    NotSelectTarget();
    //}

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    DecideTarget();
    //}

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
