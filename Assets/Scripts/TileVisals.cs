using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TileData))]
public class TileVisuals : MonoBehaviour
{
    private TileData tileData;

    [SerializeField] public BoardColors boardColorPalette;
    [SerializeField] private GameObject visualContainer;
    [SerializeField] private MeshRenderer boardToken;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private Transform gearCW;
    [SerializeField] private Transform gearCCW;
    private void Awake()
    {
        tileData = GetComponent<TileData>();
        tileData.onValueChanged.AddListener(SetVisuals);
        tileData.onStatusChanged.AddListener(SetLoopStatus);

        SetVisuals();
        SetLoopStatus();
    }

    private void SetLoopStatus()
    {
        if (tileData.isInvalid) boardToken.material.color = boardColorPalette.invalidTile;
        if (tileData.isOnLoop) boardToken.material.color = boardColorPalette.loopTile;
        else boardToken.material.color = boardColorPalette.validTile;
    }

    public void SetVisuals()
    {
        if (tileData.value == 0)
        {
            visualContainer.SetActive(false);
            return;
        }
        else if (tileData.value >= 1 && tileData.value <= 7)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(true);

            label.color = SetColor(tileData.value);
            label.text = tileData.value.ToString();
            return;
        }
        else if (tileData.value == 8)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(true);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);

            return;
        }
        else if (tileData.value == 9)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(true);
            label.gameObject.SetActive(false);

            return;
        }
        else return;
    }


    private Color SetColor(int value)
    {
        switch (value)
        {
            case 1:
            case 2:
                return boardColorPalette.pair12;

            case 3:
            case 4:
                return boardColorPalette.pair34;

            case 5:
            case 6:
                return boardColorPalette.pair56;

            case 7:
                return boardColorPalette.tile7;
            case 8:
            case 9:
                return boardColorPalette.gears;

            default:
                return Color.white;
        }
    }
}

