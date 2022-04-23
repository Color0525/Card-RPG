using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;

        StartCoroutine(SceneController.Instance.FadeIn());
    }

    /// <summary>
    /// ボタンからニューゲームのマップ読み込みを呼ぶ
    /// </summary>
    public void ButtonCallNewGameLoadMapScene()
    {
        SceneController.Instance.CallNewGameLoadMapScene();
    }
}
