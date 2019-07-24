using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<AudioClip> listOfSounds;

    public AudioSource sfxPlayer;

    public static bool win = true;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public static void PlayAudio(int index) {
        if(Instance == null) {
            return;
        }
        if(index >= 0 && index < Instance.listOfSounds.Count) {
            if(Instance.listOfSounds!= null && Instance.listOfSounds[index] != null) {
                Instance.sfxPlayer.clip = Instance.listOfSounds[index];
                Instance.sfxPlayer.Play();
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
