using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnCellView : MonoBehaviour
{
    [SerializeField] private GameObject debugColorImg;

    [SerializeField] GameObject redTeam, blueTeam;
    [SerializeField] Image icon;

    [SerializeField] GameObject cropIcon, mercIcon, streetIcon;

    public void SetIcon(Sprite iconToSet)
    {
        icon.sprite = iconToSet;
    }

    public void SetType(CharacterClassTypes cct) {
        if(cct == CharacterClassTypes.CORPORATE) {
            cropIcon.SetActive(true);
        }
        if(cct == CharacterClassTypes.MERCENARY) {
            mercIcon.SetActive(true);
        }
        if(cct == CharacterClassTypes.STREET) {
            streetIcon.SetActive(true);
        }
    }

    public void SetColor(int teamId)
    {
        if (debugColorImg != null)
        {
            var img = debugColorImg.GetComponent<Image>();
            if (teamId == TurnManager.Instance.thisPlayerId)
            {
                redTeam.SetActive(false);
                blueTeam.SetActive(true);
            }
            else
            {
                redTeam.SetActive(true);
                blueTeam.SetActive(false);
            }
        }
    }
}
