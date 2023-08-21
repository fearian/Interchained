using System;
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
    private Rotator cwRotator;
    private Rotator ccwRotator;
    
    private void Awake()
    {
        tileData = GetComponent<TileData>();
        tileData.onValueChanged.AddListener(SetVisuals);
        tileData.onLoopChanged.AddListener(SetLoopStatus);
        cwRotator = gearCW.GetComponentInChildren<Rotator>();
        ccwRotator = gearCCW.GetComponentInChildren<Rotator>();

        SetVisuals();
        SetLoopStatus();
    }

    private void Update()
    {
        SetVisuals();
    }

    private void SetLoopStatus()
    {
        if (tileData.IsOnLoopIncorrectly)
        {
            boardToken.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        else
        {
            boardToken.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    public void SetVisuals()
    {
        // Set Token visuals
        if (tileData.IsInvalid)
        {
            cwRotator?.IsRotating(false);
            ccwRotator?.IsRotating(false);
            boardToken.material.color = boardColorPalette.invalidTile;
        }
        else if (tileData.IsOnLoop)
        {
            cwRotator?.IsRotating(false);
            ccwRotator?.IsRotating(false);
            boardToken.material.color = boardColorPalette.loopTile;
        }
        else
        {
            cwRotator?.IsRotating(true);
            ccwRotator?.IsRotating(true);
            boardToken.material.color = boardColorPalette.validTile;
        }

        // Set Data Visuals
        if (tileData.Value == 0 && !tileData.IsOnLoop)
        {
            visualContainer.SetActive(false);
            
            return;
        }
        else if (tileData.Value == 0 && tileData.IsOnLoop)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);

            return;
        }
        else if (tileData.Value >= 1 && tileData.Value <= 7)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(true);

            label.color = SetColor(tileData.Value);
            label.text = tileData.Value.ToString();
            return;
        }
        else if (tileData.Value == 8)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(true);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);

            return;
        }
        else if (tileData.Value == 9)
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

