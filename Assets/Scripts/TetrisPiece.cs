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
    public Vector2Int[] takingCoordinates { get; private set; }

    public void SetTakingCoordinate()
    {
        takingCoordinates = Data.Cells[pieceAlphabet];
    }
}

public class TetrisPiece : MonoBehaviour
{
    [SerializeField]
    public Tetromino tetromino { get; private set; }
    public Vector3Int position = new Vector3Int(-1, 8, 0);

    public TetrisPiece(Tetromino tetromino)
    {
        this.tetromino = tetromino;
    }

    public void Move(Vector3Int newPosition)
    {
        position.x = newPosition.x;
        position.y = newPosition.y;
    }
}
