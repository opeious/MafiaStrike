using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenScript : MonoBehaviour
{
    public void ssbutton()
    {
        SceneManager.LoadScene("LoadingSplash", LoadSceneMode.Single);
        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
        }
    }
}
