using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class FPStracker : MonoBehaviour
{
    public int avgFrameRate;

    public TextMeshProUGUI dText;
 
    public void Update ()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = (int)current;
        if (dText != null)
        {
            dText.text = avgFrameRate.ToString() + " FPS";   
        }
    }
}