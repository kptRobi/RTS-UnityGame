using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour {

    [SerializeField]
    GameObject spawnPrefab;

    Button button;
    Text text;

    private void Awake()
    {
        button = GetComponentInChildren<Button>(true);
        text = GetComponentInChildren<Text>(true);
        Buyable buyable;
        if (spawnPrefab && (buyable = spawnPrefab.GetComponent<Buyable>()))
        {
            button.image.sprite = buyable.icon;
        }
    }

    private void Update()
    {
        Buyable buyable;
        if (spawnPrefab && (buyable = spawnPrefab.GetComponent<Buyable>()))
        {
            text.text = buyable.cost + "W€";
            button.interactable = Money.HaveEnoughMoney(buyable.cost);
        }
    }


    public void SpawnUnit()
    {
        CameraControl.SpawnUnits(spawnPrefab);
    }
}
