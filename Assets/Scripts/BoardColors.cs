using UnityEngine;

[CreateAssetMenu(fileName = "Board Color Palette", menuName = "Interchained/Board Colors", order = 1)]
public class BoardColors : ScriptableObject
{
    [Header("Tile Value Colors")]
    public Color pair12 = Color.green;
    public Color pair34 = Color.blue;
    public Color pair56 = Color.red;
    public Color tile7 = Color.yellow;
    public Color gears = Color.white;
    
    [Header("Board Colors")]
    public Color regionNorth = Color.gray;
    public Color regionEast = Color.gray;
    public Color regionSouth = Color.gray;
    public Color regionWest = Color.gray;
    [Space]
    public Color validTile = Color.grey;
    public Color invalidTile = Color.red;
    public Color loopTile = Color.blue;
    public Color loopComplete = Color.green;

    [Header("Token Meshes")]
    public Mesh tokenMesh;
    public Mesh tokenMeshLocked;

    [Header("Material Instances")]
    public Material tokenMat;
    public Material invalidTokenMat;
    public Material lockedTokenMat;
    public Material loopTokenMat;
    public Material loopCompleteTokenMat;

    private void Awake()
    {
        tokenMat.color = validTile;
        invalidTokenMat = new Material(tokenMat);
        invalidTokenMat.color = invalidTile;
        loopTokenMat = new Material(tokenMat);
        loopTokenMat.color = loopTile;
        loopCompleteTokenMat = new Material(tokenMat);
        loopCompleteTokenMat.color = loopComplete;
    }
}
