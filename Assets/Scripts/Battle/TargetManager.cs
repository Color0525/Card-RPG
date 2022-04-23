using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ターゲット処理を管理///////////＜不要＞
/// </summary>
public class TargetManager : MonoBehaviour
{
    //シングルトン
    static TargetManager m_instance;
    static public TargetManager Instance => m_instance;
    private void Awake() => m_instance = this;


    [SerializeField] GameObject m_targetButtonPrefab = default;

    Action<List<BattleStatusControllerBase>> m_finishCallback = null;
    //List<BattleStatusControllerBase> m_selectedUnit = new List<BattleStatusControllerBase>();
    List<TargetButtonController> m_targetButton = new List<TargetButtonController>();
    public List<TargetButtonController> TargetButton => m_targetButton;


    /// <summary>
    /// ターゲットを始める
    /// </summary>
    public void StartTarget(List<BattleStatusControllerBase> targets, bool changeable, Action<List<BattleStatusControllerBase>> callback)
    {
        //m_cancelTargetingButton.SetActive(true);//キャンセルボタン表示←UIマネで？
        targets.ForEach(x =>
        {
            Debug.Log(x.CenterPosition.position);

            GameObject go = Instantiate(m_targetButtonPrefab, GameObject.FindWithTag("MainCanvas").transform);
            go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, x.CenterPosition.position);
            TargetButtonController tbc = go.GetComponent<TargetButtonController>();
            //tbc.SetupTarget(x, changeable);
            m_targetButton.Add(tbc);
        });
        m_targetButton.First().SelectTarget();
        m_finishCallback = callback; //コールバック関数を受け取って選択終了時に実行する
    }

    /// <summary>
    /// ターゲットを終え、コールバックを呼ぶ
    /// </summary>
    public void FinishTarget()
    {
        m_finishCallback(m_targetButton.Where(x => x.Selected).Select(x => x.ThisUnit).ToList());
        StopTarget();

        //m_currentActor.GetComponent<BattlePlayerController>().PlayerAction(m_currentCommandSkill, selectUnits);
    }

    /// <summary>
    /// ターゲットを終了
    /// </summary>
    public void StopTarget()
    {
        //m_cancelTargetingButton.SetActive(false);//キャンセルボタン非表示←UIマネで？
        //ターゲットボタンの削除
        m_targetButton.ForEach(x => Destroy(x.gameObject));
        m_targetButton.Clear();

        //m_currentCommandTargets.ForEach(x => Destroy(x.gameObject));
        //m_currentCommandTargets.Clear();
    }
    ///// <summary>
    ///// 対象選択をキャンセルする//コマンド選択に戻る
    ///// </summary>
    //public void CancelTarget()
    //{
    //    StopTarget();
    //    //BeginPlayerCommandSelect();
    //}
}
