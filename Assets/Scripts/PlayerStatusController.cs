using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのステータスアイコンを操作
/// </summary>
public class PlayerStatusController : StatusIconController
{
    [SerializeField] TextMeshProUGUI m_HPValue = default;
    [SerializeField] TextMeshProUGUI m_GuardValue = default;
    [SerializeField] TextMeshProUGUI m_SPValue = default;

    public override void Awake()
    {
        //m_coolTimeBarPanel = GameObject.FindWithTag("PlayersCoolTimeBar").transform;
    }

    /// <summary>
    /// ステータスアイコンのセットアップ(Player)
    /// </summary>
    /// <param name="status"></param>
    /// <param name="coolTimePanel"></param>
    public override void SetupStatus(BattleStatusControllerBase status, GameObject coolTimePanel)
    {
        UpdateHPBar(status.MaxHP, status.CurrentHP);
        UpdateGuardBar(status.MaxGuard, status.CurrentGuard);
        UpdateSPBar(status.MaxSP, status.CurrentSP);

        //m_coolTimeBarPanel = GameObject.FindWithTag("PlayersCoolTimeBar").transform;
        GameObject go = Instantiate(m_coolTimeBarPrefab, coolTimePanel.transform.Find("Players"));
        m_coolTimeBar = go.GetComponent<Slider>();
        m_coolTimeValue = go.GetComponentInChildren<TextMeshProUGUI>();//valueを参照
        Image[] children = go.GetComponentsInChildren<Image>();
        foreach (var child in children)
        {
            if (child.name == "Frame") child.color = Color.green;//枠の色を変える(playerは緑) 
            else if (child.name == "UnitImage") child.sprite = status.CoolTimeIcon;//そのユニットのイメージに
        }
        UpdateCoolTimeBar(status.CoolTime);
    }

    /// <summary>
    /// HPBarを更新(Player)
    /// </summary>
    /// <param name="maxHP"></param>
    /// <param name="currentHP"></param>
    public override void UpdateHPBar(int maxHP, int currentHP)
    {
        base.UpdateHPBar(maxHP, currentHP);

        //バーが飛び出すアニメ
        m_HPBar.transform.DOScale(new Vector3(1.5f, 0.9f, 1), 0.05f);
        m_HPBar.transform.DOScale(1f, 0.1f).SetDelay(m_effectTime);

        //減るときは赤く、回復するときは緑などの演出追加？

        //値を現在値までなめらかに変化させる
        DOTween.To(() => int.Parse(m_HPValue.text), x => m_HPValue.text = x.ToString(), currentHP, m_effectTime);
    }

    /// <summary>
    /// GuardBarを更新(Player)
    /// </summary>
    /// <param name="maxGuard"></param>
    /// <param name="currentGuard"></param>
    public override void UpdateGuardBar(int maxGuard, int currentGuard)
    {
        //バーが飛び出すアニメ
        m_guardBar.transform.DOScale(new Vector3(1.5f, 0.9f, 1), 0.05f);
        base.UpdateGuardBar(maxGuard, currentGuard);
        m_guardBar.transform.DOScale(1f, 0.1f).SetDelay(m_effectTime);

        //0になった時ガラスが割れるような演出追加？

        //値を現在値までなめらかに変化させる
        DOTween.To(() => int.Parse(m_GuardValue.text), x => m_GuardValue.text = x.ToString(), currentGuard, m_effectTime);
    }

    /// <summary>
    /// SPBarを更新(Player)
    /// </summary>
    /// <param name="maxSP"></param>
    /// <param name="currentSP"></param>
    public override void UpdateSPBar(int maxSP, int currentSP)
    {
        //バーが飛び出すアニメ
        m_SPBar.transform.DOScale(new Vector3(1.5f, 0.9f, 1), 0.05f);
        base.UpdateSPBar(maxSP, currentSP);
        m_SPBar.transform.DOScale(1f, 0.1f).SetDelay(m_effectTime);

        //値を現在値までなめらかに変化させる
        DOTween.To(() => int.Parse(m_SPValue.text), x => m_SPValue.text = x.ToString(), currentSP, m_effectTime);
    }
}
