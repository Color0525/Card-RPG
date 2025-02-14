﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody を使って Humanoid タイプのキャラクターをアニメーションさせながら動かすコンポーネント
/// 入力を受け取り、それに従ってオブジェクトをメインカメラと相対的な方向に動かす。
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class MapPlayerController : MonoBehaviour
{
    /// <summary>
    /// 接触時戦うPlayerPrefab
    /// </summary>
    public GameObject[] m_battlePlayerPrefabs;

    /// <summary>動く速さ</summary>
    [SerializeField] float m_movingSpeed = 5f;
    /// <summary>ターンの速さ</summary>
    [SerializeField] float m_turnSpeed = 3f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float m_jumpPower = 5f;
    /// <summary>接地判定の際、中心 (Pivot) からどれくらいの距離を「接地している」と判定するかの長さ</summary>
    [SerializeField] float m_isGroundedLength = 1.1f;
    /// <summary>攻撃判定のトリガー</summary>
    [SerializeField] Collider m_attackTrigger = null;

    Animator m_anim = null;
    Rigidbody m_rb = null;

    /// <summary>
    /// 制御状態
    /// </summary>
    bool m_stop = false;

    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();

        if (m_attackTrigger)
        {
            // 攻撃判定のオブジェクトを非アクティブにする
            m_attackTrigger.gameObject.SetActive(false);
        }

        if (SceneController.m_Instance.m_PlayerMapPosition != Vector3.zero)
        {
            transform.position = SceneController.m_Instance.m_PlayerMapPosition;
            transform.rotation = SceneController.m_Instance.m_PlayerMapRotation;
        }
    }

    void Update()
    {
        if (m_stop)
        {
            return;
        }

        // 方向の入力を取得し、方向を求める
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // 入力方向のベクトルを組み立てる
        Vector3 dir = Vector3.forward * v + Vector3.right * h;

        if (dir == Vector3.zero)
        {
            // 方向の入力がニュートラルの時は、y 軸方向の速度を保持するだけ
            m_rb.velocity = new Vector3(0f, m_rb.velocity.y, 0f);
        }
        else
        {
            // カメラを基準に入力が上下=奥/手前, 左右=左右にキャラクターを向ける
            dir = Camera.main.transform.TransformDirection(dir);    // メインカメラを基準に入力方向のベクトルを変換する
            dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            // 入力方向に滑らかに回転させる
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);  // Slerp を使うのがポイント

            Vector3 velo = dir.normalized * m_movingSpeed; // 入力した方向に移動する
            velo.y = m_rb.velocity.y;   // ジャンプした時の y 軸方向の速度を保持する
            m_rb.velocity = velo;   // 計算した速度ベクトルをセットする
        }

        if (m_jumpPower > 0)
        {
            // ジャンプの入力を取得し、接地している時に押されていたらジャンプする
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
                m_anim.SetTrigger("Jump");
            }
        }

        if (m_attackTrigger)
        {
            // 攻撃の入力を取得し、接地している時に押されていたら攻撃する
            if (Input.GetButtonDown("Fire1") && IsGrounded())
            {
                m_anim.SetTrigger("Attack");
                // TODO: 攻撃中に移動できないようにすると、より自然な動きになる
            }
        }
    }

    /// <summary>
    /// Update の後に呼び出される。Update の結果を元に何かをしたい時に使う。
    /// </summary>
    void LateUpdate()
    {
        // 水平方向の速度を求めて Animator Controller のパラメーターに渡す
        Vector3 horizontalVelocity = m_rb.velocity;
        horizontalVelocity.y = 0;
        m_anim.SetFloat("Speed", horizontalVelocity.magnitude);
    }

    /// <summary>
    /// 地面に接触しているか判定する
    /// </summary>
    /// <returns></returns>
    bool IsGrounded()
    {
        // Physics.Linecast() を使って足元から線を張り、そこに何かが衝突していたら true とする
        Vector3 start = this.transform.position;   // start: オブジェクトの中心
        Vector3 end = start + Vector3.down * m_isGroundedLength;  // end: start から真下の地点
        Debug.DrawLine(start, end); // 動作確認用に Scene ウィンドウ上で線を表示する
        bool isGrounded = Physics.Linecast(start, end); // 引いたラインに何かがぶつかっていたら true とする
        return isGrounded;
    }

    /// <summary>
    /// 制御を停止
    /// </summary>
    public void StopControl()
    {
        m_stop = true;
        m_anim.speed = 0;
        m_rb.isKinematic = true;
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
    }

    /// <summary>
    /// 攻撃判定を有効にする
    /// </summary>
    void BeginAttack()
    {
        if (m_attackTrigger)
        {
            m_attackTrigger.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 攻撃判定を無効にする
    /// </summary>
    void EndAttack()
    {
        if (m_attackTrigger)
        {
            m_attackTrigger.gameObject.SetActive(false);
        }
    }
}
