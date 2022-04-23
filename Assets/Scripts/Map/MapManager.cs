using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// MapSceneを管理
/// </summary>
public class MapManager : MonoBehaviour
{
    [SerializeField] bool m_hideSystemMouseCursor = false;

    //カットシーン
    [SerializeField] PlayableDirector m_openingCutScene = default;
    [SerializeField] PlayableDirector m_continueCutScene = default;

    //プレイヤー
    [SerializeField] GameObject m_mapPlayer = default;
    [SerializeField] GameObject m_mainVirtualCamera = default;

    //UI
    [SerializeField] GameObject m_UI = default;
    [SerializeField] TextMeshProUGUI m_questTaskText = default;
    [SerializeField] GameObject m_questClearText = default;

    void Start()
    {
        //マウスカーソル非表示
        if (m_hideSystemMouseCursor)
        {
            Cursor.visible = false;
        }

        StartCoroutine(SceneController.Instance.FadeIn());

        //状態に応じCutScene再生
        if (SceneController.Instance.m_NewGame)
        {
            SceneController.Instance.SetNewGameFalse();
            SceneController.Instance.SetInirialQuest();
            SceneController.Instance.ResetPosition();
            StartCoroutine(PlayOpeningCutScene());
        }
        else if (SceneController.Instance.m_GameOver)
        {
            StartCoroutine(PlayContinueCutScene());
        }
        else
        {
            Activation();
        }
    }

    void Update()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            SceneController.Instance.CallLoadTitleScene();
        }    
    }

    /// <summary>
    /// OPを再生し、ActivationPlayer()
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayOpeningCutScene()
    {
        m_openingCutScene.gameObject.SetActive(true);
        m_openingCutScene.Play();
        while (m_openingCutScene.state == PlayState.Playing)
        {
            yield return new WaitForEndOfFrame();
        }
        Activation();
        m_openingCutScene.gameObject.SetActive(false);
    }

    /// <summary>
    /// CutSceneを再生し、ActivationPlayer()
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayContinueCutScene()
    {
        m_continueCutScene.gameObject.SetActive(true);
        m_continueCutScene.Play();
        while (m_continueCutScene.state == PlayState.Playing)
        {
            yield return new WaitForEndOfFrame();
        }
        Activation();
        m_continueCutScene.gameObject.SetActive(false);
    }

    /// <summary>
    /// Playerとメインバーチャルカメラ、UIをActiveにし、クエスト表示する
    /// </summary>
    void Activation()
    {
        m_mapPlayer.SetActive(true);
        m_mainVirtualCamera.SetActive(true);
        m_UI.SetActive(true);

        if (SceneController.Instance.m_CurrentQuest)
        {
            m_questTaskText.text = SceneController.Instance.m_CurrentQuest.QuestTaskText();

            if (SceneController.Instance.m_CurrentQuest.m_ClearFlag)
            {
                m_questClearText.SetActive(true);
                if (!SceneController.Instance.m_CurrentQuest.m_Clear)
                {
                    SceneController.Instance.m_CurrentQuest.QuestClear();
                    m_questClearText.GetComponent<Animator>().Play("Clear");
                    m_questClearText.GetComponent<AudioSource>().Play();
                }
            }
        }
        else
        {
            m_questTaskText.text = "クエストなし";
            //m_questTaskText.gameObject.GetComponent<Animator>().Play("None");
        }
    }

    /// <summary>
    /// MapUnitをコントロール不可にする
    /// </summary>
    public void Freeze()
    {
        foreach (var player in FindObjectsOfType<MapPlayerController>())
        {
            player.StopControl();
        }
        foreach (var enemy in FindObjectsOfType<MapEnemyController>())
        {
            enemy.StopControl();
        }
    }
}
