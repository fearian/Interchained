using System;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TileData))]
public class TileVisuals : MonoBehaviour
{
    private TileData tileData;

    [SerializeField] public BoardColors boardColorPalette;
    [SerializeField] public TileLabels tileLabels;
    [SerializeField] private GameObject visualContainer;
    [SerializeField] private MeshRenderer boardToken;
    private MeshFilter tokenMesh;
    [SerializeField] private MeshRenderer pairMarker;
    private MeshFilter pairMesh;
    [SerializeField] private TextMeshPro label;
    [SerializeField] private Transform gearCW;
    [SerializeField] private Transform gearCCW;
    [SerializeField] private Transform blocker;
    private Rotator cwRotator;
    private Rotator ccwRotator;
    
    private void Awake()
    {
        tileData = GetComponent<TileData>();
        tileData.onValueChanged.AddListener(SetVisuals);
        tileData.onLoopChanged.AddListener(SetLoopStatus);
        cwRotator = gearCW.GetComponentInChildren<Rotator>();
        ccwRotator = gearCCW.GetComponentInChildren<Rotator>();
        tokenMesh = boardToken.GetComponent<MeshFilter>();
        pairMesh = pairMarker.GetComponent<MeshFilter>();

        SetVisuals();
        SetLoopStatus();
    }

    private void SetLoopStatus()
    {
        if (tileData.IsMarkedForLoop && tileData.IsInvalid)
        {
            boardToken.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        else
        {
            boardToken.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private Hex GetPairedDirection()
    {
        if (tileData.IsPaired == false) return Hex.zero;

        return tileData.hex - tileData.pairedTile.hex;
    }

    public void SetVisuals()
    {
        if (tileData.IsPaired && tileData.IsLowerOfPair)
        {
            pairMesh.mesh = boardColorPalette.pairMarker;
            pairMarker.gameObject.SetActive(true);
            pairMarker.material.color = SetColor(tileData.Value);
            int dir = Array.IndexOf(Hex.AXIAL_DIRECTIONS, GetPairedDirection());
            pairMarker.transform.localRotation = Quaternion.Euler(-90, 0, -60f * dir);
            //Debug.DrawLine(tileData.hex.ToWorld() + new Vector3(0,0,0.3f), tileData.pairedTile.hex.ToWorld(), Color.cyan, 1.5f);
        }
        else if (tileData.Value == 7 && tileData.IsInvalid == false)
        {
            pairMesh.mesh = boardColorPalette.singleMarker;
            pairMarker.gameObject.SetActive(true);
            pairMarker.material.color = SetColor(tileData.Value);
        }
        else
        {
            pairMarker.gameObject.SetActive(false);
        }
        
        if (tileData.IsBlocker)
        {
            tokenMesh.gameObject.SetActive(false);
        }
        else
        {
            tokenMesh.gameObject.SetActive(true);
        }
        // set locked mesh
        if (tileData.IsLocked)
        {
            tokenMesh.mesh = boardColorPalette.tokenMeshLocked;
        }
        else
        {
            tokenMesh.mesh = boardColorPalette.tokenMesh;
        }
        
        // Set Token visuals
        if (tileData.IsInvalid)
        {
            cwRotator?.IsRotating(false);
            ccwRotator?.IsRotating(false);
            boardToken.material.color = boardColorPalette.invalidTile;
        }
        else if (tileData.IsMarkedForLoop)
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
        if (tileData.IsBlocker)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            blocker.gameObject.SetActive(true);

            return;
        }
        else if (tileData.Value == 0 && !tileData.IsMarkedForLoop)
        {
            visualContainer.SetActive(false);
            
            return;
        }
        else if (tileData.Value == 0 && tileData.IsMarkedForLoop)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            blocker.gameObject.SetActive(false);

            return;
        }
        else if (tileData.Value >= 1 && tileData.Value <= 7)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(true);
            blocker.gameObject.SetActive(false);

            label.color = SetColor(tileData.Value);
            label.text = tileLabels.GetLabel(tileData.Value);
            return;
        }
        else if (tileData.Value == 8)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(true);
            gearCCW.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            blocker.gameObject.SetActive(false);

            return;
        }
        else if (tileData.Value == 9)
        {
            visualContainer.SetActive(true);
            gearCW.gameObject.SetActive(false);
            gearCCW.gameObject.SetActive(true);
            label.gameObject.SetActive(false);
            blocker.gameObject.SetActive(false);

            return;
        }
        else return;
    }

    public void SwapLabelVisuals(TileLabels newLabelPalette)
    {
        tileLabels = newLabelPalette;
        SetVisuals();
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

