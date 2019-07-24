using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<AudioClip> listOfSounds;

    public static bool win = true;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
