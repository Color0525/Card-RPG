//using System;
using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// BattleSceneを管理
/// </summary>
public class BattleManager : MonoBehaviour
{
    //シングルトン
    static BattleManager m_instance;
    static public BattleManager Instance => m_instance;
    private void Awake() => m_instance = this;

    /// <summary>
    /// 戦闘状態
    /// </summary>
    enum BattleState
    {
        BeginBattle,
        WaitTime,
        InAction,
        EndBattle,
        EndWait,
    }
    /// <summary>
    /// 現在の戦闘状態
    /// </summary>
    [SerializeField] BattleState m_battleState = BattleState.BeginBattle;
    //ユニットの位置
    [SerializeField] Transform m_playerBattlePosition = default;
    [SerializeField] Transform m_enemyBattlePosition = default;
    //開始演出
    [SerializeField] CinemachineVirtualCamera m_beginBattleCamera = default;
    [SerializeField] float m_beginBattleTime = 2f;
    //UI
    [SerializeField] GameObject m_commandWindow = default;
    [SerializeField] Transform m_commandArea = default;
    [SerializeField] GameObject m_commandButtonPrefab = default;
    [SerializeField] TextMeshProUGUI m_commandInfoText = default;
    [SerializeField] GameObject m_ActionTextPrefab = default;
    //カットシーン
    [SerializeField] PlayableDirector m_winCutScene = default;
    [SerializeField] PlayableDirector m_loseCutScene = default;
    //ディレイ
    //N//[SerializeField] float m_delayAtEndTurn = 1f;
    
    //N//タイムラインの経過速度
    [SerializeField] float m_timeUpdateInterval = 0.1f;
    float m_timeCount = 0;

    /// <summary>
    ///戦うプレイヤー
    /// </summary>
    [SerializeField] GameObject[] m_playerPrefabs;
    /// <summary>
    /// 戦うエネミー
    /// </summary>
    [SerializeField] GameObject[] m_enemyPrefabs;
    //プレイヤー、エネミーの戦闘ユニットリスト
    [SerializeField] List<BattlePlayerController> m_playerUnits = new List<BattlePlayerController>();
    [SerializeField] List<BattleEnemyController> m_enemyUnits = new List<BattleEnemyController>();
    /// <summary>
    /// すべての現在戦闘ユニット
    /// </summary>
    [SerializeField] List<BattleStatusControllerBase> m_allUnits = new List<BattleStatusControllerBase>();
    /// <summary>
    /// m_allUnitに対応する現在の行動ユニット番号
    /// </summary>
    //N//int m_currentNum = 0;

    /// <summary>
    /// バトル中か
    /// </summary>
    //N//bool m_inBattle = true;

    /// <summary>
    /// 勝利したか
    /// </summary>
    bool m_won = default;


    void Start()
    {
        Cursor.visible = true; //カーソル表示

        StartCoroutine(SceneController.m_Instance.FadeIn());//フェードイン

        //SceneControllerからユニット情報を取得
        SceneController sc = SceneController.m_Instance;
        if (sc.m_PlayerPrefabs != null)
        {
            m_playerPrefabs = sc.m_PlayerPrefabs;
        }
        if (sc.m_EnemyPrefabs != null)
        {
            m_enemyPrefabs = sc.m_EnemyPrefabs;
        }

        //ユニットをインスタンスしてListにAdd
        foreach (var unit in m_playerPrefabs)
        {
            GameObject player = Instantiate(unit, m_playerBattlePosition);
            m_playerUnits.Add(player.GetComponent<BattlePlayerController>());
            m_allUnits.Add(player.GetComponent<BattleStatusControllerBase>());
        }
        foreach (var unit in m_enemyPrefabs)
        {
            GameObject enemy = Instantiate(unit, m_enemyBattlePosition);
            m_enemyUnits.Add(enemy.GetComponent<BattleEnemyController>());
            m_allUnits.Add(enemy.GetComponent<BattleStatusControllerBase>());
        }

        //戦闘開始演出（最後にWaitTimeにする）
        StartCoroutine(BeginBattle(m_beginBattleTime));
    }

    void Update()
    {
        //State管理
        switch (m_battleState)
        {
            case BattleState.BeginBattle://開始演出待ち
                break;

            case BattleState.WaitTime://一定時間おきにクールを減らし、クールタイムが0になったユニットから現在ユニット行動開始
                m_timeCount += Time.deltaTime;
                if (m_timeCount > m_timeUpdateInterval) 
                {
                    m_timeCount = 0;

                    foreach (var unit in m_allUnits)
                    {
                        if (unit.Alive)
                        {
                            //クールタイムはflootにして、0以下になるようにする(状態異常やスキルなどでクール速度半減、倍増、などあるから)
                            if (unit.CoolTime <= 0)
                            {
                                m_battleState = BattleState.InAction;
                                unit.BeginAction();
                                return;
                            }
                        }
                    }
                    foreach (var unit in m_allUnits)
                    {
                        if (unit.Alive)
                        {
                            unit.TimeElapsed();
                        }
                    }
                }

                //N//m_allUnits[m_currentNum].StartAction();
                break;

            case BattleState.InAction://行動中
                break;

            case BattleState.EndBattle://戦闘終了したときResultTimelineを再生しAfterBattleへ
                if (m_won)
                {
                    //クエストのタスクチェック
                    if (SceneController.m_Instance.m_CurrentQuest && !SceneController.m_Instance.m_CurrentQuest.m_Clear)
                    {
                        foreach (var enemyObj in m_enemyPrefabs)
                        {
                            if (SceneController.m_Instance.m_CurrentQuest.CheckTarget(enemyObj))
                            {
                                SceneController.m_Instance.m_CurrentQuest.AddQuestCount();
                            }
                        }
                    }

                    m_winCutScene.Play();
                }
                else
                {
                    m_loseCutScene.Play();
                }

                m_battleState = BattleState.EndWait;

                //N//次のターンへ
                //m_currentNum++;
                //if (m_currentNum >= m_allUnits.Count)//_actionParty.Length) 
                //{
                //    m_currentNum = 0;
                //    //_isPlayersTurn = _isPlayersTurn ? false : true;
                //}
                //m_battleState = BattleState.WaitTime;
                break;

            case BattleState.EndWait://クリックでMapシーンへ
                if (Input.anyKeyDown)
                {
                    SceneController.m_Instance.CallLoadMapScene(!m_won);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 戦闘開始演出
    /// </summary>
    /// <param name="aimTime"></param>
    /// <returns></returns>
    IEnumerator BeginBattle(float aimTime)
    {
        yield return StartCoroutine(SceneController.m_Instance.SlideEffect());
        yield return new WaitForSeconds(aimTime);
        m_beginBattleCamera.gameObject.SetActive(false);
        yield return new WaitForSeconds(Camera.main.gameObject.GetComponent<CinemachineBrain>().m_DefaultBlend.BlendTime);
        m_battleState = BattleState.WaitTime;
    }

    //N
    ///// <summary>
    ///// 戦闘状態を行動中にする
    ///// </summary>
    //public void BeginAction()
    //{
    //    m_battleState = BattleState.InAction;
    //}

    /// <summary>
    /// 待機時間に戻る
    /// </summary>
    public void ReturnWaitTime()
    {
        m_battleState = BattleState.WaitTime;
        //N//StartCoroutine(DelayAndUpdateState(m_delayAtEndTurn, BattleState.EndBattle));//コルーチンやめてEndTurのカウントでディレイをかける
    }

    //N
    ///// <summary>
    ///// Delayし、戦闘状態を更新する
    ///// </summary>
    ///// <param name="delayTime"></param>
    ///// <param name="state"></param>
    ///// <returns></returns>
    //IEnumerator DelayAndUpdateState(float delayTime, BattleState state)
    //{
    //    yield return new WaitForSeconds(delayTime);
    //    m_battleState = state;
    //}

    /// <summary>
    /// コマンドセレクトを開始する
    /// </summary>
    /// <param name="actor"></param>
    public void StartCommandSelect(SkillDatabase[] skills, BattlePlayerController actor)
    {
        m_commandWindow.SetActive(true);
        foreach (var skill in skills)
        {
            GameObject go = Instantiate(m_commandButtonPrefab, m_commandArea);
            go.GetComponent<CommandButtonController>().SetupCammand(skill, actor, m_commandInfoText);
        }
    }

    /// <summary>
    /// コマンドセレクトを終了する
    /// </summary>
    public void EndCommandSelect()
    {
        foreach (Transform child in m_commandArea)
        {
            Destroy(child.gameObject);
        }
        m_commandWindow.SetActive(false);
    }

    ///// <summary>
    ///// 死亡エネミーを現在戦闘ユニットリストから消し、全滅したなら勝利でバトル終了
    ///// </summary>
    ///// <param name="deadEnemy"></param>
    //public void DeleteEnemyList(BattleEnemyController deadEnemy)
    //{
    //    m_allUnits.Remove(deadEnemy);
    //    m_enemyUnits.Remove(deadEnemy);
    //    if (m_enemyUnits.Count == 0)
    //    {
    //        m_battleState = BattleState.EndBattle;
    //        //m_inBattle = false;
    //        m_won = true;
    //    }
    //}
    ///// <summary>
    ///// 死亡プレイヤーを現在戦闘ユニットリストから消し、全滅したなら敗北でバトル終了
    ///// </summary>
    ///// <param name="deadPlayer"></param>
    //public void DeletePlayerList(BattlePlayerController deadPlayer)
    //{
    //    m_allUnits.Remove(deadPlayer);
    //    m_playerUnits.Remove(deadPlayer);
    //    if (m_playerUnits.Count == 0)
    //    {
    //        m_battleState = BattleState.EndBattle;
    //        //m_inBattle = false;
    //        m_won = false;
    //    }
    //}

    //N
    /// <summary>
    /// 死亡ユニットの陣営が全滅しているなら戦闘終了//ユニット指定なし？
    /// </summary>
    /// <param name="deadUnit"></param>
    public void CheckWinOrLose(BattleStatusControllerBase deadUnit)
    {
        if (deadUnit is BattleEnemyController)
        {
            if (!m_enemyUnits.Any(x => x.Alive == true))//生きているユニットがいるのでは無いなら
            {
                m_battleState = BattleState.EndBattle;
                m_won = true;
            }
        }
        else if (deadUnit is BattlePlayerController)
        {
            if (!m_playerUnits.Any(x => x.Alive == true))//生きているユニットがいるのでは無いなら
            {
                m_battleState = BattleState.EndBattle;
                m_won = false;
            }
        }
    }

    /// <summary>
    /// ActionTextを出す
    /// </summary>
    /// <param name="actionText"></param>
    public void ShowActionText(string actionText)
    {
        GameObject go = Instantiate(m_ActionTextPrefab, GameObject.FindWithTag("MainCanvas").transform);
        go.GetComponentInChildren<TextMeshProUGUI>().text = actionText;
        DOTween.To(() => go.transform.localPosition - new Vector3(500, 0, 0), x => go.transform.localPosition = x, go.transform.localPosition, 0.05f);
        Destroy(go, 1f);
    }
}
