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
    private PieceAlphabet _pieceAlphabet;
    public PieceAlphabet PieceAlphabet
    {
        get => _pieceAlphabet;
    }
    public Tile tile;
    private Vector2Int[] _shapeCells;
    public Vector2Int[] ShapeCells
    {
        get
        {
            Vector2Int[] temp = new Vector2Int[_shapeCells.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = _shapeCells[i];
            }
            return temp;
        }
    }
    public void InitializeShapeCells()
    {
        _shapeCells = Data.Cells[_pieceAlphabet];
    }
}

public class TetrisPiece
{
    [SerializeField]
    public Tetromino tetromino { get; private set; }
    public Vector3Int position { get; private set; }
    private int _RotationIndex;
    public int RotationIndex
    {
        get => _RotationIndex;
        set => _RotationIndex = value % 4;
    }
    private Vector2Int[] _takingCells;
    public Vector2Int[] TakingCells
    {
        get
        {
            Vector2Int[] temp = new Vector2Int[_takingCells.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = _takingCells[i];
            }
            return temp;
        }
    }

    public TetrisPiece(Tetromino tetromino)
    {
        this.tetromino = tetromino;
        _takingCells = tetromino.ShapeCells;
        position = new Vector3Int(-1, 8, 0);
    }

    public TetrisPiece ShallowCopy()
    {
        TetrisPiece b = new TetrisPiece(tetromino);
        b.position = position;
        b._takingCells = _takingCells;

        return b;
    }

    public void Move(Vector3Int translation)
    {
        position += translation;
    }

    public void SetRotateStatus(RotateDirection rotationDirection, Vector2Int[] takingCells)
    {
        _RotationIndex += (int)rotationDirection;
        _RotationIndex %= _takingCells.Length;
        _RotationIndex = _RotationIndex < 0 ? _RotationIndex + _takingCells.Length : _RotationIndex;
        _takingCells = takingCells;
    }
}
