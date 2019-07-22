using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] public Image fillBarImg;

    [SerializeField] private GameObject corpIcon;
    [SerializeField] private GameObject mercIcon;
    [SerializeField] private GameObject streetIcon;
    [SerializeField] private GameObject neutralFB;
    [SerializeField] private TextMeshProUGUI neutralText;
    [SerializeField] private GameObject strongFB;
    [SerializeField] private TextMeshProUGUI strongText;
    [SerializeField] private GameObject weakFB;
    [SerializeField] private TextMeshProUGUI weakText;

    private GameboardCharacterController healthBarOf;
    
    public void Setup(GameboardCharacterController gcc) {
        healthBarOf = gcc;
        if(gcc.Data.charType == CharacterClassTypes.CORPORATE) {
            corpIcon.SetActive(true);
        }
        if(gcc.Data.charType == CharacterClassTypes.MERCENARY) {
            mercIcon.SetActive(true);
        }
        if(gcc.Data.charType == CharacterClassTypes.STREET) {
            streetIcon.SetActive(true);
        }
    }

    public void SetTexts(int dmg) {
        neutralText.text = "-" + dmg;
        strongText.text = "-" + dmg;
        weakText.text = "-" + dmg;
    }

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
