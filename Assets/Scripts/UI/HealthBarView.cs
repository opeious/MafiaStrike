using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;

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

    [SerializeField] public Slider sliderMain;
    [SerializeField] public Slider sliderAnimation;
    

    private GameboardCharacterController healthBarOf;

    public bool takingWeakDamage = false;
    public bool takingStrongDamage = false;
    public bool takingNeutralDamage = false;

    public Animator strongAnimDmg;
    public Animator weakAnimDmg;
    public Animator neutralAnimDmg;
    
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

    public IEnumerator DoTakeDmgAniamtion(Animator animator) {
        animator.enabled = true;
        FixedUpdate();
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length - 0.01f);
        animator.enabled = false;
        takingWeakDamage = false;
        takingStrongDamage = false;
        takingNeutralDamage = false;
    }

    private void FixedUpdate() {
        if(!showNew && !showStrong && !showWeak) {
            if(sliderAnimation.gameObject.activeSelf) {
                sliderAnimation.gameObject.SetActive(false);
            }
        }
        if (showNew || takingNeutralDamage) {
            if(neutralFB.activeSelf == false) {
                neutralFB.SetActive(true);
                if(showNew)
                    sliderAnimation.gameObject.SetActive(true);
            }
        } else {
            if(neutralFB.activeSelf == true) {
                neutralFB.SetActive(false);
            }           
        }        
        if (showStrong || takingStrongDamage) {
            if(strongFB.activeSelf == false) {
                strongFB.SetActive(true);
                if(showStrong)
                    sliderAnimation.gameObject.SetActive(true);
            }
        } else {
            if(strongFB.activeSelf == true) {
                strongFB.SetActive(false);
            }           
        }
        if (showWeak || takingWeakDamage) {
            if(weakFB.activeSelf == false) {
                weakFB.SetActive(true);
                if(showWeak)
                    sliderAnimation.gameObject.SetActive(true);
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

        float sliderNormalizedValue = (float)((healthBarOf.Data.maxHealth - healthBarOf.currentHealth)  + dmg) / (float)healthBarOf.Data.maxHealth;
        sliderNormalizedValue = Mathf.Clamp(sliderNormalizedValue, 0f, 1f);
        sliderAnimation.value = sliderNormalizedValue;
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
