using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    ////シングルトン
    //static UnitsManager m_instance;
    //static public UnitsManager Instance => m_instance;
    //private void Awake() => m_instance = this;

    /// <summary>
    ///戦うプレイヤー
    /// </summary>
    [SerializeField] GameObject[] m_playerPrefabs;
    /// <summary>
    /// 戦うエネミー
    /// </summary>
    [SerializeField] GameObject[] m_enemyPrefabs;

    //ユニットの位置
    [SerializeField] Transform[] m_playerBattlePosition = default;
    [SerializeField] Transform[] m_enemyBattlePosition = default;

    //プレイヤー、エネミー、全ての戦闘ユニットリスト
    [SerializeField] List<BattleStatusControllerBase> m_playerUnits = new List<BattleStatusControllerBase>();
    [SerializeField] List<BattleEnemyController> m_enemyUnits = new List<BattleEnemyController>();
    [SerializeField] List<BattleStatusControllerBase> m_allUnits = new List<BattleStatusControllerBase>();

    //public GameObject[] PlayerPrefabs => m_playerPrefabs;
    //public GameObject[] EnemyPrefabs => m_enemyPrefabs;
    public List<BattleStatusControllerBase> PlayerUnits => m_playerUnits;
    public List<BattleEnemyController> EnemyUnits => m_enemyUnits;
    public List<BattleStatusControllerBase> AllUnits => m_allUnits;

    /// <summary>
    /// SceneControllerからユニット情報を取得してセット
    /// </summary>
    public void SetEncountEnemies()
    {
        SceneController sc = SceneController.Instance;
        if (sc.m_PlayerPrefabs != null)
        {
            m_playerPrefabs = sc.m_PlayerPrefabs;
        }
        if (sc.m_EnemyPrefabs != null)
        {
            m_enemyPrefabs = sc.m_EnemyPrefabs;
        }
    }

    /// <summary>
    /// ユニットを生成してListに追加
    /// </summary>
    public void GeneratUnits()
    {
        for (int i = 0; i < m_playerPrefabs.Length; i++)
        {
            GameObject player = Instantiate(m_playerPrefabs[i], m_playerBattlePosition[i]);
            m_playerUnits.Add(player.GetComponent<BattlePlayerController>());
            m_allUnits.Add(player.GetComponent<BattlePlayerController>());
            //m_cameraTargetGroup.AddMember(player.transform, 1, 1);
        }
        for (int i = 0; i < m_enemyPrefabs.Length; i++)
        {
            GameObject enemy = Instantiate(m_enemyPrefabs[i], m_enemyBattlePosition[i]);
            m_enemyUnits.Add(enemy.GetComponent<BattleEnemyController>());
            m_allUnits.Add(enemy.GetComponent<BattleEnemyController>());
            //m_cameraTargetGroup.AddMember(enemy.transform, 1, 1);
        }
    }

    /// <summary>
    /// 死亡ユニットの陣営が全滅しているかどうか
    /// </summary>
    /// <param name="deadUnit"></param>
    public void CheckWinOrLose(BattleStatusControllerBase deadUnit)
    {
        if (deadUnit is BattleEnemyController)
        {
            if (!m_enemyUnits.Any(x => x.IsAlive == true))//生きているユニットがいるのでは無いなら
            {
                Debug.Log("勝利");
                //m_inBattle = false;
                //m_won = true;
            }
        }
        else if (deadUnit is BattlePlayerController)
        {
            if (!m_playerUnits.Any(x => x.IsAlive == true))//生きているユニットがいるのでは無いなら
            {
                Debug.Log("敗北");
                //m_inBattle = false;
                //m_won = false;
            }
        }
    }

    //public void SetupIcon()
    //{
    //    m_enemyUnits.ForEach(x => x.SetupIcon(m_coolTimePanel));//アイコンセットアップ
    //    m_playerUnits.ForEach(x => x.InstantiateStatusIcon(m_pleyerStatusPanel));//アイコン生成
    //    m_playerUnits.ForEach(x => x.SetupIcon(m_coolTimePanel));//アイコンセットアップ
    //}

    //優先して行動するユニット
    //[SerializeField] List<BattleStatusControllerBase> m_priorityUnit = new List<BattleStatusControllerBase>();
}
