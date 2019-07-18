using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public void OnTapToContinue()
    {
        SceneManager.LoadScene("LoadingSplash", LoadSceneMode.Single);
        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
        }
    }
}
