using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTurnView : MonoBehaviour
{

    [SerializeField]
    private GameObject CellPrefab;

    private List<GameObject> cells;

    void Start()
    {
        TurnManager.RefreshViews += OnRefresh;
        cells = new List<GameObject>();
        OnRefresh();
    }

    private void OnDestroy()
    {
        TurnManager.RefreshViews -= OnRefresh;
    }

    void OnRefresh()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Destroy(cells[i]);
        }
        
        var tOrder = TurnManager.Instance.TurnOrder;
        foreach (var singleTurn in tOrder)
        {
            var newTurnGO = Instantiate(CellPrefab);
            newTurnGO.transform.SetParent(gameObject.transform);
            var nextTurnCell = newTurnGO.GetComponent<NextTurnCellView>(); 
            nextTurnCell.DebugTextSetup(singleTurn.Data.debugId);
            nextTurnCell.SetColor(singleTurn.Data.teamId);
            nextTurnCell.SetIcon(singleTurn.Data.icon);
            cells.Add(newTurnGO);
        }
    }
}
