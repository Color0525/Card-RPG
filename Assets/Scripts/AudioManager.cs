﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オーディオを管理
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip m_intro = default;
    [SerializeField] AudioClip m_loop = default;

    AudioSource m_audio;

    void Start()
    {
        m_audio = GetComponent<AudioSource>();
        StartCoroutine(PlayBGM());
    }

    /// <summary>
    /// BGMをPlay。イントロがあるなら
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayBGM()
    {
        if (m_intro)
        {
            m_audio.clip = m_intro;
            m_audio.loop = false;
            m_audio.Play();

            while (m_audio.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        m_audio.clip = m_loop;
        m_audio.loop = true;
        m_audio.Play();
    }
}
