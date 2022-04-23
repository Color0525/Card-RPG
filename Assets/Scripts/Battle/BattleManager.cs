//using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

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
    enum Phase
    {
        Start,
        //BeginTurn,
        //ExecuteTurn,
        PlayersTurn,
        EnemiesTurn,
        //EndTurn,
        Result,
        End,
    }
    /// <summary>
    /// 現在の戦闘状態
    /// </summary>
    [SerializeField] BattleState m_battleState = BattleState.BeginBattle;
    [SerializeField] Phase m_phase = Phase.Start;
    //N//タイムラインの経過速度
    [SerializeField] float m_timeUpdateInterval = 0.1f;
    float m_timeCount = 0;
    ///// <summary>
    /////戦うプレイヤー
    ///// </summary>
    //[SerializeField] GameObject[] m_playerPrefabs;
    ///// <summary>
    ///// 戦うエネミー
    ///// </summary>
    //[SerializeField] GameObject[] m_enemyPrefabs;
    ////プレイヤー、エネミー、全ての戦闘ユニットリスト
    //[SerializeField] List<BattlePlayerController> m_playerUnits = new List<BattlePlayerController>();
    //[SerializeField] List<BattleEnemyController> m_enemyUnits = new List<BattleEnemyController>();
    ////public List<BattlePlayerController> PlayerUnits => m_playerUnits;
    ////public List<BattleEnemyController> EnemyUnits => m_enemyUnits;
    //[SerializeField] List<BattleStatusControllerBase> m_allUnits = new List<BattleStatusControllerBase>();
    ////優先して行動するユニット
    //[SerializeField] List<BattleStatusControllerBase> m_priorityUnit = new List<BattleStatusControllerBase>();
    //現在行動ユニット
    BattleStatusControllerBase m_currentActor = default;
    //コマンド選択スキル
    SkillDatabase m_currentCommandSkill = default;
    //BattlePlayerController m_currentCommandActor = default;
    //List<TargetButtonController> m_currentCommandTargets = new List<TargetButtonController>();
    /// <summary>
    /// m_allUnitに対応する現在の行動ユニット番号
    /// </summary>
    //N//int m_currentNum = 0;
    /// <summary>
    /// バトル中か
    /// </summary>
    bool m_inBattle = true;
    /// <summary>
    /// 勝利したか
    /// </summary>
    bool m_won = default;

    //ディレイ
    //[SerializeField] CinemachineVirtualCamera m_beginBattleCamera = default;
    [SerializeField] float m_beginDirectingTime = 2f;
    [SerializeField] float m_actionDelayTime = 1f;
    //UI
    [SerializeField] CinemachineVirtualCamera m_mainCamera = default;
    [SerializeField] CinemachineVirtualCamera m_backCamera = default;
    [SerializeField] CinemachineTargetGroup m_cameraTargetGroup = default;
    [SerializeField] GameObject m_coolTimePanel = default;
    [SerializeField] GameObject m_pleyerStatusPanel = default;
    [SerializeField] GameObject m_commandWindow = default;
    [SerializeField] Transform m_commandArea = default;
    [SerializeField] TextMeshProUGUI m_commandInfoText = default;
    [SerializeField] GameObject m_commandButtonPrefab = default;
    [SerializeField] GameObject m_cancelTargetingButton = default;

    [SerializeField] GameObject m_ActionTextPrefab = default;
    //カットシーン
    [SerializeField] PlayableDirector m_winCutScene = default;
    [SerializeField] PlayableDirector m_loseCutScene = default;

    //NEW　///////////////////////////////////////////////////////////////////////////
    //event Action m_beginTurnEvent = default; //イベント管理 専用クラスにする？
    //event Action m_endTurnEvent = default;
    //bool m_isFirstStrike = true; //とりあえず必ずプレイヤー先行
    UnitsManager m_unitM = default;
    [SerializeField] Button m_turnEndButton = default;
    [SerializeField] GameObject m_targetButtonPrefab;


    void Start()
    {
        Cursor.visible = true; //カーソル表示
        m_unitM = GetComponent<UnitsManager>();
        //UnitsManager.Instance.SetEncountEnemies();
        //UnitsManager.Instance.GeneratUnits();
        //yield return StartCoroutine(BeginDirecting(m_beginBattleTime));//戦闘開始演出
        Battle().Forget();//戦闘開始
        UniTaskHoge(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask UniTaskHoge(CancellationToken cancellation_token)              // IEnumerator→async UniTask
    {
        Debug.Log("UniTask:1フレーム目");
        await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);               // yield return null;→await UniTask.Yield( PlayerLoopTiming.Update, cancellation_token );
        Debug.Log("UniTask:2フレーム目");
    }
    void Update()
    {
        //State管理///////////////////////////////////////////////////////////////////////////////
        //switch (m_battleState)
        //{
        //    case BattleState.BeginBattle://開始演出待ち
        //        break;

        //    case BattleState.WaitTime://一定時間おきにクールを減らし、クールタイムが0になったユニットから現在ユニット行動開始
        //        if (!m_inBattle)
        //        {
        //            m_battleState = BattleState.EndBattle;
        //            return;
        //        }

        //        m_timeCount += Time.deltaTime;
        //        if (m_timeCount > m_timeUpdateInterval)
        //        {
        //            m_timeCount = 0;

        //            //優先ユニットの行動
        //            if (m_priorityUnit.Count > 0)
        //            {
        //                m_currentActor = m_priorityUnit[0];
        //                m_priorityUnit.RemoveAt(0);
        //                m_battleState = BattleState.InAction;
        //                m_currentActor.BeginAction();
        //                return;
        //            }

        //            //生きていてクールタイム0以下ユニットの行動
        //            //クールタイムは0以下になるようにする(状態異常やスキルなどでクール速度半減、倍増、などあるから)
        //            BattleStatusControllerBase[] actor = m_allUnits.Where(x => x.Alive).Where(x => x.CoolTime <= 0).ToArray();
        //            if (actor.Length > 0)
        //            {
        //                m_currentActor = actor[0];
        //                m_battleState = BattleState.InAction;
        //                m_currentActor.BeginAction();
        //                return;
        //            }

        //            //生きているユニットに時間経過
        //            m_allUnits.Where(x => x.Alive).ToList().ForEach(x => x.TimeElapsed());

        //            //foreach (var unit in m_allUnits)
        //            //{
        //            //    if (unit.Alive)
        //            //    {

        //            //        if (unit.CoolTime <= 0)
        //            //        {
        //            //            m_battleState = BattleState.InAction;
        //            //            unit.BeginAction();
        //            //            return;
        //            //        }
        //            //    }
        //            //}

        //            //foreach (var unit in m_allUnits)
        //            //{
        //            //    if (unit.Alive)
        //            //    {
        //            //        unit.TimeElapsed();
        //            //    }
        //            //}
        //        }

        //        //N//m_allUnits[m_currentNum].StartAction();
        //        break;

        //    case BattleState.InAction://行動中
        //        break;

        //    case BattleState.EndBattle://戦闘終了したときResultTimelineを再生しEndWaitへ
        //        m_coolTimePanel.SetActive(false);//非アクティブ
        //        m_pleyerStatusPanel.SetActive(false);//非アクティブ
        //        if (m_won)
        //        {
        //            //クエストのタスクチェック
        //            if (SceneController.m_Instance.m_CurrentQuest && !SceneController.m_Instance.m_CurrentQuest.m_Clear)
        //            {
        //                foreach (var enemyObj in m_enemyPrefabs)
        //                {
        //                    if (SceneController.m_Instance.m_CurrentQuest.CheckTarget(enemyObj))
        //                    {
        //                        SceneController.m_Instance.m_CurrentQuest.AddQuestCount();
        //                    }
        //                }
        //            }

        //            m_winCutScene.Play();
        //        }
        //        else
        //        {
        //            m_loseCutScene.Play();
        //        }

        //        m_battleState = BattleState.EndWait;

        //        //N//次のターンへ
        //        //m_currentNum++;
        //        //if (m_currentNum >= m_allUnits.Count)//_actionParty.Length) 
        //        //{
        //        //    m_currentNum = 0;
        //        //    //_isPlayersTurn = _isPlayersTurn ? false : true;
        //        //}
        //        //m_battleState = BattleState.WaitTime;
        //        break;

        //    case BattleState.EndWait://クリックでMapシーンへ
        //        if (Input.anyKeyDown)
        //        {
        //            SceneController.m_Instance.CallLoadMapScene(!m_won);
        //        }
        //        break;
        //    default:
        //        break;
        //}
    }


    //イベント管理///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 戦闘開始演出
    /// </summary>
    /// <param name="aimTime"></param>
    /// <returns></returns>
    IEnumerator BeginDirecting(float aimTime)
    {
        StartCoroutine(SceneController.Instance.FadeIn());//フェードイン
        yield return StartCoroutine(SceneController.Instance.SlideEffect());//スライドエフェクト
        yield return new WaitForSeconds(aimTime);//待ち
        m_mainCamera.gameObject.SetActive(true);//カメラ切り替え

        //↓「ターン開始」などの演出追加したい
        yield return new WaitForSeconds(2);
    }

    //void BeginBattle()
    //{
    //    //m_coolTimePanel.SetActive(true);//アクティブ
    //    //m_pleyerStatusPanel.SetActive(true);//アクティブ
    //    //m_enemyUnits.ForEach(x => x.SetupIcon(m_coolTimePanel));//アイコンセットアップ
    //    //m_playerUnits.ForEach(x => x.InstantiateStatusIcon(m_pleyerStatusPanel));//アイコン生成
    //    //m_playerUnits.ForEach(x => x.SetupIcon(m_coolTimePanel));//アイコンセットアップ
    //    //yield return new WaitForSeconds(Camera.main.gameObject.GetComponent<CinemachineBrain>().m_DefaultBlend.BlendTime);
    //    //m_battleState = BattleState.WaitTime;
    //    BeginTurn();
    //}
    //void BeginTurn()
    //{
    //    //m_beginTurnEvent?.Invoke();
    //    if (m_isFirstStrike)//先行ならBeginPlayerTurn();　後攻ならBeginEnemyTurn();
    //    {
    //        BeginPlayerTurn();
    //    }
    //    else
    //    {
    //        BeginEnemyTurn();
    //    }
    //}
    //void BeginPlayerTurn()
    //{
    //    m_unitM.PlayerUnits.ForEach(x => x.BeginTurn());

    //    //行動ユニット選択へ///行動ユニット選択はコマンドUIと一緒にする？
    //    TargetManager.Instance.StartTarget(m_unitM.PlayerUnits.Where(x => x.IsActive).ToList(), true, BeginAction);
    //}
    //void BeginAction(List<BattleStatusControllerBase> selectUnit)
    //{
    //    selectUnit.First().BeginAction();
    //}
    //void EndAction(List<BattleStatusControllerBase> selectUnit)
    //{
    //    selectUnit.First().BeginAction();
    //}
    //void EndPlayerTurn()//味方の行動終了時に、ほか全て終了しているならここへ
    //{

    //}
    //void BeginEnemyTurn()
    //{
    //    //順に敵の行動
    //}
    //void EndEnemyTurn()//敵の行動終了時に、ほか全て終了しているならここへ
    //{

    //}
    //void EndTurn()
    //{
    //    //m_endTurnEvent?.Invoke();
    //    BeginTurn();
    //}
    //void EndBattle()
    //{

    //}
    //void BeginUnitAction()
    //{
    //    m_beginUnitActEvent?.Invoke();
    //}
    //void EndUnitAction()
    //{
    //    m_endUnitActEvent?.Invoke();
    //}


    //コルーチンによるステート管理///////////////////////////////////////////////////////
    async UniTask Battle()
    {
        while (m_phase != Phase.End)
        {
            switch (m_phase)
            {
                case Phase.Start:
                    m_unitM.SetEncountEnemies();
                    m_unitM.GeneratUnits();
                    await BeginDirecting(m_beginDirectingTime);
                    m_phase = Phase.PlayersTurn;
                    break;
                case Phase.PlayersTurn:
                    m_unitM.PlayerUnits.ForEach(x => x.BeginTurn());//Player全てにターン開始時処理

                    CommandPhase commandPhase = CommandPhase.SelectActor;
                    while (commandPhase != CommandPhase.End)
                    {
                        BattleStatusControllerBase actor = null;//行動者
                        SkillDatabase action = null;//行動
                        List<BattleStatusControllerBase> target = null;//対象

                        switch (commandPhase)
                        {
                            case CommandPhase.SelectActor:
                                m_turnEndButton.gameObject.SetActive(true);//ターンエンドボタンを有効化
                                //クラス作ってそこに渡すか、○onClick.AddListenerでボタンに直接関数を渡すか、383でwhileでボタンが押されたときを取るか→✕383まで行かない////////////////
                                CancellationTokenSource cts = new CancellationTokenSource();
                                m_turnEndButton.onClick.AddListener(() =>//ターンエンドボタンにクリック時の処理を追加
                                {
                                    Debug.Log("TurnEndButton Click");
                                    cts.Cancel();
                                    commandPhase = CommandPhase.End;
                                });
                                actor = await SelectTarget(m_unitM.PlayerUnits, cts.Token);
                                m_turnEndButton.gameObject.SetActive(false);//ターンエンドボタンを無効化
                                commandPhase = CommandPhase.SelectAction;
                                break;
                            case CommandPhase.SelectAction:
                                while (true)
                                {
                                    Debug.Log($"{actor} SelectAction Phase");
                                    await UniTask.Yield();
                                }

                                //キャンセルボタンを有効化
                                //actor.移動可能;
                                //yield return StartCoroutine(SelectAction());
                                commandPhase = CommandPhase.SelectTarget;
                                break;
                            case CommandPhase.SelectTarget:
                                //範囲を表示
                                //範囲内の敵を取得
                                //yield return StartCoroutine(SelectTarget(範囲内の敵, 範囲から選択ならture, target));
                                //キャンセルボタンを無効化
                                //actor.移動不可;
                                commandPhase = CommandPhase.Execution;
                                break;
                            case CommandPhase.Execution:
                                //targetに実行
                                commandPhase = CommandPhase.SelectActor;
                                break;
                            case CommandPhase.End:
                                break;
                            default:
                                break;
                        }
                        await UniTask.Yield();
                    }

                    //yield return StartCoroutine(Command(m_unitM.PlayerUnits));
                    m_unitM.PlayerUnits.ForEach(x => x.BeginAction());

                    m_unitM.PlayerUnits.ForEach(x => x.EndAction());
                    m_phase = Phase.EnemiesTurn;
                    break;
                case Phase.EnemiesTurn:
                    m_unitM.EnemyUnits.ForEach(x => x.BeginTurn());
                    //順に敵AIの行動を取得→行動開始（行動情報）？
                    m_unitM.EnemyUnits.ForEach(x => x.BeginAction());
                    m_unitM.EnemyUnits.ForEach(x => x.EndAction());
                    m_phase = Phase.PlayersTurn;
                    break;
                case Phase.Result:
                    m_phase = Phase.End;
                    break;
                case Phase.End:
                    break;
                default:
                    break;
            }
            await UniTask.Yield();
        }
    }


    enum CommandPhase
    {
        SelectActor,
        SelectAction,
        SelectTarget,
        Execution,
        End,
    }
    //IEnumerator Command(List<BattleStatusControllerBase> players)
    //{
    //    CommandPhase m_scp = CommandPhase.SelectActor;
    //    while (m_scp != CommandPhase.End)
    //    {
    //        switch (m_scp)
    //        {
    //            case CommandPhase.SelectActor:
    //                //ターンエンドボタンを有効化
    //                List<BattleStatusControllerBase> target = default;
    //                yield return StartCoroutine(SelectTarget(players, true, target));
    //                break;
    //            case CommandPhase.SelectAction:
    //                break;
    //            case CommandPhase.SelectTarget:
    //                break;
    //            case CommandPhase.End:
    //                break;
    //            default:
    //                break;
    //        }
    //        yield return null;
    //        ////エンドボタン有効化
    //        //yield return StartCoroutine(Target(players, true, target));
    //        ////エンドボタン無効化
    //        //BattleStatusControllerBase actor = target.First();
    //        //yield return StartCoroutine(actor.BeginAction());
    //    }
    //}
    //IEnumerator EnemiesTurn(List<BattleEnemyController> enemies)
    //{
    //    enemies.ForEach(x => x.BeginTurn());
    //    enemies.ForEach(x => x.BeginAction());
    //    enemies.ForEach(x => x.EndAction());
    //}
    
    async UniTask<BattleStatusControllerBase> SelectTarget(List<BattleStatusControllerBase> targetList, CancellationToken token)
    {
        BattleStatusControllerBase selectUnit = null; //選択したものが入る
        List<TargetButtonController> tbcList = new List<TargetButtonController>();
        targetList.ForEach(unit => //生成、セットアップ
        {
            GameObject go = Instantiate(m_targetButtonPrefab, GameObject.FindWithTag("MainCanvas").transform);//Target用キャンバスに？
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, unit.CenterPosition.position);
            TargetButtonController tbc = go.GetComponent<TargetButtonController>();
            tbc.Setup(pos, unit, false, () => selectUnit = tbc.ThisUnit);//これでいく○selectUnitにいれる関数？変数？をSetup、onClick.AddListenerどっちで渡すか。もしくは538でボタンが押されたときを取るか→全てのボタンを取得する必要があるからダメ////////////////
            tbcList.Add(tbc);
        });
        token.Register(() => tbcList.ForEach(x => Destroy(x.gameObject)));

        await UniTask.WaitUntil(() => selectUnit != null, PlayerLoopTiming.Update, token);
        //while (selectUnit is null) await UniTask.Yield(token);

        tbcList.ForEach(x => Destroy(x.gameObject));
        return selectUnit;
    }
    //All用のターゲット//うまく行ったらAllをベースに関数をまとめる？
    async UniTask<List<BattleStatusControllerBase>> AllTarget(List<BattleStatusControllerBase> targetList, CancellationToken token)
    {
        //BattleStatusControllerBase selectUnit = null; //選択したものが入る
        bool isSelected = false;
        List<TargetButtonController> tbcList = new List<TargetButtonController>();
        targetList.ForEach(unit => //生成、セットアップ
        {
            GameObject go = Instantiate(m_targetButtonPrefab, GameObject.FindWithTag("MainCanvas").transform);//Target用キャンバスに？
            Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, unit.CenterPosition.position);
            TargetButtonController tbc = go.GetComponent<TargetButtonController>();
            tbc.Setup(pos, unit, true, () => isSelected = true);//これでいく○selectUnitにいれる関数？変数？をSetup、onClick.AddListenerどっちで渡すか。もしくは538でボタンが押されたときを取るか→全てのボタンを取得する必要があるからダメ////////////////
            tbcList.Add(tbc);
        });
        token.Register(() => tbcList.ForEach(x => Destroy(x.gameObject)));

        await UniTask.WaitUntil(() => isSelected, PlayerLoopTiming.Update, token);
        //while (selectUnit is null) await UniTask.Yield(token);

        List<BattleStatusControllerBase> selectUnits = tbcList.Where(x => x.Selected).Select(x => x.ThisUnit).ToList();
        tbcList.ForEach(x => Destroy(x.gameObject));
        return selectUnits;


        //yield return new WaitWhile(() => isSelected);//選択終了するまで待つ
        //outTarget = tbcList.Where(x => x.Selected).Select(x => x.ThisUnit).ToList(); //擬似的に選択したユニットを返す
        ////m_cancelTargetingButton.SetActive(false);//キャンセルボタン非表示←UIマネで？
        //tbcList.ForEach(x => Destroy(x.gameObject));//ターゲットボタンの削除
    }

    //N
    ///// <summary>
    ///// 戦闘状態を行動中にする
    ///// </summary>
    //public void BeginAction()
    //{
    //    m_battleState = BattleState.InAction;
    //}
    ///// <summary>
    ///// 待機時間に戻る
    ///// </summary>
    //public void ReturnWaitTime()
    //{
    //    m_battleState = BattleState.WaitTime;
    //    m_timeCount = -m_actionDelayTime;//Delayを与える
    //    //m_mainButtleCamera.Follow = m_normalFollowPosition.gameObject.transform;
    //    //N//StartCoroutine(DelayAndUpdateState(m_delayAtEndTurn, BattleState.EndBattle));//コルーチンやめてEndTurのカウントでディレイをかける
    //}
    /// <summary>
    /// Delayし、待機時間に戻る
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReturnWaitTime()
    {
        yield return new WaitForSeconds(m_actionDelayTime);
        m_battleState = BattleState.WaitTime;
        m_currentActor.EndAction();
    }

    /// <summary>
    /// 優先ユニットに追加
    /// </summary>
    /// <param name="unit"></param>
    //public void AddPriorityUnit(BattleStatusControllerBase unit)
    //{
    //    m_priorityUnit.Add(unit);
    //}

    /// <summary>
    /// コマンドセレクトを開始する
    /// </summary>
    /// <param name="actor"></param>
    public void BeginPlayerCommandSelect()//BattlePlayerController actor = null)
    {
        //if (actor != null)
        //{
        //    m_currentCommandActor = actor;
        //}

        m_commandWindow.SetActive(true);

        foreach (var skill in m_currentActor.HavesSkills)
        {
            GameObject go = Instantiate(m_commandButtonPrefab, m_commandArea);
            go.GetComponent<CommandButtonController>().SetupCammand(skill, /*m_currentCommandActor,*/ m_commandInfoText);
        }
    }
    /// <summary>
    /// コマンドセレクトを終了する
    /// </summary>
    public void EndPlayerCommandSelect()
    {
        CommandButtonController.OthersCommandButton.ForEach(x => Destroy(x.gameObject));
        CommandButtonController.OthersCommandButton.Clear();
        //foreach (Transform child in m_commandArea)
        //{
        //    Destroy(child.gameObject);
        //}
        m_commandWindow.SetActive(false);
    }

    ///// <summary>
    ///// 対象選択を開始
    ///// </summary>
    ///// <param name="skill"></param>
    //public void BeginPlayerTargetSelect(SkillDatabase skill)
    //{
    //    m_cancelTargetingButton.SetActive(true);//キャンセルボタン表示
    //    EndPlayerCommandSelect();//コマンド非表示

    //    m_currentCommandSkill = skill;

    //    if (skill.Renge != SkillDatabase.TargetRenge.Myself)
    //    {
    //        if (skill.Renge == SkillDatabase.TargetRenge.Single)
    //        {
    //            UnitsManager.Instance.EnemyUnits.Where(x => x.IsAlive).ToList().ForEach(x => InstantiateTargetButton(x, true));
    //            //foreach (var enemy in m_enemyUnits)
    //            //{
    //            //    InstantiateTargetButton(enemy, true);
    //            //}
    //            TargetManager.Instance.TargetButton.First().SelectTarget();//一番前をture
    //        }
    //        else if (skill.Renge == SkillDatabase.TargetRenge.Overall)
    //        {
    //            UnitsManager.Instance.EnemyUnits.Where(x => x.IsAlive).ToList().ForEach(x => InstantiateTargetButton(x, false));
    //            //foreach (var enemy in m_enemyUnits)
    //            //{
    //            //    InstantiateTargetButton(enemy, false);
    //            //}
    //            //TargetButtonController.OthersTargetButton.ForEach(x => x.SelectTarget()); //すべてture
    //        }
    //    }
    //    else
    //    {
    //        InstantiateTargetButton(m_currentActor, false);
    //        //TargetButtonController.OthersTargetButton.First().SelectTarget();//一つをture
    //    }
    //}
    ///// <summary>
    ///// ターゲットボタンを生成しセットアップする
    ///// </summary>
    ///// <param name="target"></param>
    ///// <param name="changeable"></param>
    //void InstantiateTargetButton(BattleStatusControllerBase target, bool changeable)
    //{
    //    GameObject go = Instantiate(m_targetButtonPrefab, GameObject.FindWithTag("MainCanvas").transform);
    //    go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, target.CenterPosition.position);
    //    go.GetComponent<TargetButtonController>().SetupTarget(target, changeable);
    //    //if (canSelect)
    //    //{
    //    //    targetUI.SetupTarget(target, TargetButtonController.m_othersTargetButton);
    //    //}
    //    //else
    //    //{
    //    //    targetUI.SetupTarget(target);
    //    //}
    //}
    ///// <summary>
    ///// プレイヤー行動を実行
    ///// </summary>
    //public void PlayPlayerAction()
    //{
    //    BattleStatusControllerBase[] targets = TargetButtonController.OthersTargetButton.Where(x => x.Selected).Select(x => x.ThisUnit).ToArray();
    //    m_currentActor.GetComponent<BattlePlayerController>().PlayerAction(m_currentCommandSkill, targets);

    //    EndPlayerTargetSelect();
    //}
    ///// <summary>
    ///// 対象選択を終了
    ///// </summary>
    //void EndPlayerTargetSelect()
    //{
    //    m_cancelTargetingButton.SetActive(false);//キャンセルボタン非表示
    //    //ターゲットボタンの削除
    //    TargetButtonController.OthersTargetButton.ForEach(x => Destroy(x.gameObject));
    //    TargetButtonController.OthersTargetButton.Clear();

    //    //m_currentCommandTargets.ForEach(x => Destroy(x.gameObject));
    //    //m_currentCommandTargets.Clear();
    //}
    ///// <summary>
    ///// 対象選択をキャンセルしてコマンド選択に戻る
    ///// </summary>
    //public void CancelPlayerTargetSelect()
    //{
    //    EndPlayerTargetSelect();
    //    BeginPlayerCommandSelect();
    //}

    /// <summary>
    /// バックカメラのフォロー対象を変え、アクティブにする
    /// </summary>
    /// <param name="targetPos"></param>
    public void BeginPlayerBackCamera(Transform targetPos)
    {
        //if (targetPos == null) targetPos = m_normalFollowPosition;
        m_backCamera.Follow = targetPos;
        m_backCamera.gameObject.SetActive(true);
    }
    /// <summary>
    /// バックカメラを非アクティブにする
    /// </summary>
    public void EndPlayerBackCamera()
    {
        m_backCamera.gameObject.SetActive(false);
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
    ///// <summary>
    ///// 死亡ユニットの陣営が全滅しているなら戦闘終了//ユニット指定なし？
    ///// </summary>
    ///// <param name="deadUnit"></param>
    //public void CheckWinOrLose(BattleStatusControllerBase deadUnit)
    //{
    //    if (deadUnit is BattleEnemyController)
    //    {
    //        if (!m_enemyUnits.Any(x => x.Alive == true))//生きているユニットがいるのでは無いなら
    //        {
    //            m_inBattle = false;
    //            m_won = true;
    //        }
    //    }
    //    else if (deadUnit is BattlePlayerController)
    //    {
    //        if (!m_playerUnits.Any(x => x.Alive == true))//生きているユニットがいるのでは無いなら
    //        {
    //            m_inBattle = false;
    //            m_won = false;
    //        }
    //    }
    //}

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
