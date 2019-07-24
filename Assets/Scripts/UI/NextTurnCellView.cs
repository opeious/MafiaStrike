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

    public void SetIcon(Sprite iconToSet)
    {
        icon.sprite = iconToSet;
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
