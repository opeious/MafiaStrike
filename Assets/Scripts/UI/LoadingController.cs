﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public Slider slider;

    public Animator _anim;
    
    public float tdt;

    private bool loadingAtm;

    [SerializeField] private GameObject enableOnTime;

    private void Start()
    {
        PhotonNetwork.OfflineMode = false;
    }

    public bool playingAudio;

    private void Update()
    {
        tdt += Time.deltaTime;
        if (slider.value >= 0.75f)
        {
            _anim.SetBool("punchAnim", true);
            if(!playingAudio) {
                playingAudio = true;
                SoundManager.PlayAudio(1);
            }
            enableOnTime.SetActive(true);
        }

        if (slider.value >= 1f && !loadingAtm)
        {
            loadingAtm = true;
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        if (tdt > 0.02f)
        {
            tdt = 0f;
            slider.value += 0.01f;
        }
    }
}
