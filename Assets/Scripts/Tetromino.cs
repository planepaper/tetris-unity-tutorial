using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
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
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;
    public Vector2Int[] cells { get; private set; }             //이거 왜 이런식으로 속성변경하는건지

    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino];
    }
}
