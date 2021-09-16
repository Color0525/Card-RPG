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
    //public List<BattlePlayerController> PlayerUnits => m_playerUnits;
    //public List<BattleEnemyController> EnemyUnits => m_enemyUnits;
    /// <summary>
    /// すべての現在戦闘ユニット
    /// </summary>
    [SerializeField] List<BattleStatusControllerBase> m_allUnits = new List<BattleStatusControllerBase>();
    BattlePlayerController m_currentCommandActor = default;
    SkillDatabase m_currentCommandSkill = default;
    //List<TargetButtonController> m_currentCommandTargets = new List<TargetButtonController>();
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
    //ユニットの位置
    [SerializeField] Transform[] m_playerBattlePosition = default;
    [SerializeField] Transform[] m_enemyBattlePosition = default;
    //開始演出
    //[SerializeField] CinemachineVirtualCamera m_beginBattleCamera = default;
    [SerializeField] float m_beginBattleTime = 2f;
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
    [SerializeField] GameObject m_targetButtonPrefab = default;
    [SerializeField] GameObject m_ActionTextPrefab = default;
    [SerializeField] GameObject m_damageTextPrefab = default;
    //カットシーン
    [SerializeField] PlayableDirector m_winCutScene = default;
    [SerializeField] PlayableDirector m_loseCutScene = default;
    //ディレイ
    //N//[SerializeField] float m_delayAtEndTurn = 1f;


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
        for (int i = 0; i < m_playerPrefabs.Length; i++)
        {
            GameObject player = Instantiate(m_playerPrefabs[i], m_playerBattlePosition[i]);
            m_playerUnits.Add(player.GetComponent<BattlePlayerController>());
            m_allUnits.Add(player.GetComponent<BattleStatusControllerBase>());
            m_cameraTargetGroup.AddMember(player.transform, 1, 1);
        }
        //foreach (var unit in m_playerPrefabs)
        //{
        //    GameObject player = Instantiate(unit, m_playerBattlePosition);
        //    m_playerUnits.Add(player.GetComponent<BattlePlayerController>());
        //    m_allUnits.Add(player.GetComponent<BattleStatusControllerBase>());
        //}
        for (int i = 0; i < m_enemyPrefabs.Length; i++)
        {
            GameObject enemy = Instantiate(m_enemyPrefabs[i], m_enemyBattlePosition[i]);
            m_enemyUnits.Add(enemy.GetComponent<BattleEnemyController>());
            m_allUnits.Add(enemy.GetComponent<BattleStatusControllerBase>());
            m_cameraTargetGroup.AddMember(enemy.transform, 1, 1);
        }
        //foreach (var unit in m_enemyPrefabs)
        //{
        //    GameObject enemy = Instantiate(unit, m_enemyBattlePosition);
        //    m_enemyUnits.Add(enemy.GetComponent<BattleEnemyController>());
        //    m_allUnits.Add(enemy.GetComponent<BattleStatusControllerBase>());
        //}

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

            case BattleState.EndBattle://戦闘終了したときResultTimelineを再生しEndWaitへ
                m_coolTimePanel.SetActive(false);//非アクティブ
                m_pleyerStatusPanel.SetActive(false);//非アクティブ
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
        yield return StartCoroutine(SceneController.m_Instance.SlideEffect());//スライドエフェクト
        yield return new WaitForSeconds(aimTime);
        m_mainCamera.gameObject.SetActive(true);//カメラ切り替え
        m_coolTimePanel.SetActive(true);//アクティブ
        m_pleyerStatusPanel.SetActive(true);//アクティブ
        m_enemyUnits.ForEach(x => x.SetupIcon(m_coolTimePanel));//アイコンセットアップ
        m_playerUnits.ForEach(x => x.SetupIcon(m_coolTimePanel, m_pleyerStatusPanel));//アイコンセットアップ
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
        //m_mainButtleCamera.Follow = m_normalFollowPosition.gameObject.transform;
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
    public void BeginPlayerCommandSelect(BattlePlayerController actor = null)
    {
        if (actor != null)
        {
            m_currentCommandActor = actor;
        }

        m_commandWindow.SetActive(true);

        foreach (var skill in m_currentCommandActor.HavesSkills)
        {
            GameObject go = Instantiate(m_commandButtonPrefab, m_commandArea);
            go.GetComponent<CommandButtonController>().SetupCammand(skill, m_currentCommandActor, m_commandInfoText);
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

    /// <summary>
    /// 対象選択を開始
    /// </summary>
    /// <param name="skill"></param>
    public void BeginPlayerTargetSelect(SkillDatabase skill)
    {
        EndPlayerCommandSelect();//コマンド非表示
        m_cancelTargetingButton.SetActive(true);//キャンセルボタン表示

        m_currentCommandSkill = skill;

        if (skill.Renge != SkillDatabase.TargetRenge.myself)
        {
            
            if (skill.Renge == SkillDatabase.TargetRenge.Single)
            {
                foreach (var enemy in m_enemyUnits)
                {
                    InstantiateTargetButton(enemy, true);
                }
                TargetButtonController.OthersTargetButton.First().SelectTarget();//一番前をture
            }
            else if (skill.Renge == SkillDatabase.TargetRenge.Overall)
            {
                foreach (var enemy in m_enemyUnits)
                {
                    InstantiateTargetButton(enemy, false);
                }
                TargetButtonController.OthersTargetButton.ForEach(x => x.SelectTarget()); //すべてture
            }
        }
        else
        {
            InstantiateTargetButton(m_currentCommandActor, false);
            TargetButtonController.OthersTargetButton.First().SelectTarget();//一つをture
        }
    }
    /// <summary>
    /// ターゲットボタンを生成しセットアップする
    /// </summary>
    /// <param name="target"></param>
    /// <param name="canSelect"></param>
    void InstantiateTargetButton(BattleStatusControllerBase target, bool canSelect)
    {
        GameObject go = Instantiate(m_targetButtonPrefab, GameObject.FindWithTag("MainCanvas").transform);
        go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, target.CenterPosition.position);
        go.GetComponent<TargetButtonController>().SetupTarget(target, canSelect);
        //if (canSelect)
        //{
        //    targetUI.SetupTarget(target, TargetButtonController.m_othersTargetButton);
        //}
        //else
        //{
        //    targetUI.SetupTarget(target);
        //}
    }
    /// <summary>
    /// プレイヤー行動を実行
    /// </summary>
    public void PlayPlayerAction()
    {
        BattleStatusControllerBase[] targets = TargetButtonController.OthersTargetButton.Where(x => x.Selected).Select(x => x.ThisUnit).ToArray();
        m_currentCommandActor.PlayerAction(m_currentCommandSkill, targets);

        EndPlayerTargetSelect();
    }
    /// <summary>
    /// 対象選択を終了
    /// </summary>
    void EndPlayerTargetSelect()
    {
        m_cancelTargetingButton.SetActive(false);//キャンセルボタン非表示
        //ターゲットボタンの削除
        TargetButtonController.OthersTargetButton.ForEach(x => Destroy(x.gameObject));
        TargetButtonController.OthersTargetButton.Clear();
        //m_currentCommandTargets.ForEach(x => Destroy(x.gameObject));
        //m_currentCommandTargets.Clear();
    }
    /// <summary>
    /// 対象選択をキャンセルしてコマンド選択に戻る
    /// </summary>
    public void CancelPlayerTargetSelect()
    {
        EndPlayerTargetSelect();
        BeginPlayerCommandSelect();
    }

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

    /// <summary>
    /// DamageTextを出す
    /// </summary>
    /// <param name="centerPos"></param>
    /// <param name="damage"></param>
    public void DamageText(Vector3 centerPos, int damage)
    {
        GameObject go = Instantiate(m_damageTextPrefab, GameObject.FindWithTag("MainCanvas").transform);
        go.GetComponent<RectTransform>().position = RectTransformUtility.WorldToScreenPoint(Camera.main, centerPos);
        go.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
        Destroy(go, 1f);
    }
}
