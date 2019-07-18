using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnCellView : MonoBehaviour
{
    [SerializeField] private GameObject debugColorImg;

    [SerializeField] private Image icon;

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
                img.color = Color.blue;
            }
            else
            {
                img.color = Color.red;
            }
        }
    }
}
