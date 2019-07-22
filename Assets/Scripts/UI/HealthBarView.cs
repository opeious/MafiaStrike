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

    private void FixedUpdate() {
        if (showNew) {
            if(neutralFB.activeSelf == false) {
                neutralFB.SetActive(true);
            }
        } else {
            if(neutralFB.activeSelf == true) {
                neutralFB.SetActive(false);
            }           
        }        
        if (showStrong) {
            if(strongFB.activeSelf == false) {
                strongFB.SetActive(true);
            }
        } else {
            if(strongFB.activeSelf == true) {
                strongFB.SetActive(false);
            }           
        }
        if (showWeak) {
            if(weakFB.activeSelf == false) {
                weakFB.SetActive(true);
            }
        } else {
            if(weakFB.activeSelf == true) {
                weakFB.SetActive(false);
            }           
        }        
    }

    public void ResetView() {
        showNew = showWeak = showStrong = false;
    }

    bool showNew, showWeak, showStrong;

    public void SetHealthbarAdvType(int type) {
        if(type == 0) {
            showNew = true;
        }
        if(type == -1 ) {
            showWeak = true;
        }
        if(type == 1) {
            showStrong = true;
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
