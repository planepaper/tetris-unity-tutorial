using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PieceAlphabet
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

[Serializable]
public class Tetromino
{
    [SerializeField]
    private PieceAlphabet pieceAlphabet;
    public Tile tile;
    public Vector2Int[] takingCoordinate { get; private set; }

    public void SetTakingCoordinate()
    {
        takingCoordinate = Data.Cells[pieceAlphabet];
    }
}

public class TetrisPiece : MonoBehaviour
{
    [SerializeField]
    public Tetromino tetromino { get; private set; }
    public Vector3Int position { get; private set; } = new Vector3Int(-1, 8, 0);

    public TetrisPiece(Tetromino tetromino)
    {
        this.tetromino = tetromino;
    }
}
