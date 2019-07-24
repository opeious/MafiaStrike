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

    public void PlayAudio(int index) {
        if(index >= 0 && index < listOfSounds.Count) {
            if(listOfSounds!= null && listOfSounds[index] != null) {
                sfxPlayer.clip = listOfSounds[index];
                sfxPlayer.Play();
                // listOfSounds[index].
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
