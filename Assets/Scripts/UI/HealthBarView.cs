using UnityEngine.UI;
using UnityEngine;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] public Image fillBarImg;
    
    public void SetupColor(bool isCurrentTeamHB)
    {
        if (fillBarImg != null)
        {
            if (isCurrentTeamHB)
            {
                fillBarImg.color = Color.blue;
            }
            else
            {
                fillBarImg.color = Color.red;
            }
        }
    }
}
