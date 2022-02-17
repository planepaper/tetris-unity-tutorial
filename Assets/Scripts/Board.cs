using UnityEngine;
using UnityEngine.Tilemaps;

public enum RotateDirection
{
    Right = 1,
    Left = -1,
}

public class Board : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField]
    private Tetromino[] tetrominoes;
    private TetrisPiece activePiece;
    private readonly Vector3Int _SPAWNPOSITION;
    private readonly Vector2Int _BOARDSIZE;
    private readonly RectInt _OffSet;

    [SerializeField]
    private float stepDelay = 1f;
    [SerializeField]
    private float lockDelay = 0.5f;

    public Board()
    {
        _SPAWNPOSITION = new Vector3Int(-1, 8, 0);
        _BOARDSIZE = new Vector2Int(10, 20);
        Vector2Int position = new Vector2Int(-_BOARDSIZE.x / 2, -_BOARDSIZE.y / 2);
        _OffSet = new RectInt(position, _BOARDSIZE);
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        foreach (var tetromino in tetrominoes)
        {
            //TODO: Have to change this Logic
            //Change inserting method of tiles from unity-editor to in-code.
            tetromino.InitializeShapeCells();
        }
    }

    private void Start()
    {
        TryToSpawnNewPiece();
        SetPieceTile();
    }

    private void Update()
    {
        ClearPieceTile();

        activePiece.CountTime();

        if (Input.GetKeyDown(KeyCode.A))
        {
            TryToMovePiece(Vector3Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryToMovePiece(Vector3Int.right);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            TryToMovePiece(Vector3Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            TryToRotatePiece(RotateDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryToRotatePiece(RotateDirection.Right);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDropPiece();
            SetPieceTile();
            ClearFullLines();
            TryToSpawnNewPiece();
        }
        else
        {
            TryToStepPieceAfterTimesUp();
        }

        SetPieceTile();
    }

    private void TryToMovePiece(Vector3Int translation)
    {
        if (IsValidPosition(activePiece.position + translation))
        {
            activePiece.Move(translation);
        }
    }

    private void TryToStepPieceAfterTimesUp()
    {
        if (activePiece.stepTimeCount >= stepDelay)
        {
            if (IsValidPosition(activePiece.position + Vector3Int.down))
            {
                activePiece.Move(Vector3Int.down);
                activePiece.InitializeStepCount();
            }
            else
            {
                if (activePiece.lockTimeCount >= lockDelay)
                {
                    SetPieceTile();
                    ClearFullLines();
                    TryToSpawnNewPiece();
                }
            }
        }
    }

    public void TryToSpawnNewPiece()
    {
        int randomIndex = Random.Range(0, tetrominoes.Length);
        activePiece = new TetrisPiece(tetrominoes[randomIndex]);

        //GameOver
        if (!IsValidPosition(activePiece.position))
        {
            tilemap.ClearAllTiles();
        }
    }

    public void SetPieceTile()
    {
        foreach (var takingCell in activePiece.TakingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCell + activePiece.position;
            tilemap.SetTile(tilePosition, activePiece.tetromino.tile);
        }
    }

    public void ClearPieceTile()
    {
        foreach (var takingCells in activePiece.TakingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCells + activePiece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private void HardDropPiece()
    {
        Vector3Int newPosition = activePiece.position;
        Vector3Int translation = Vector3Int.zero;
        while (IsValidPosition(newPosition))
        {
            newPosition += Vector3Int.down;
            translation += Vector3Int.down;
        }
        translation += Vector3Int.up;
        activePiece.Move(translation);
    }

    public bool IsValidPosition(Vector3Int position)
    {
        return IsValidPosition(position, activePiece.TakingCells);
    }

    public bool IsValidPosition(Vector3Int position, Vector2Int[] takingCells)
    {
        foreach (var takingCell in takingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCell + position;

            if (!_OffSet.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    private void TryToRotatePiece(RotateDirection direction)
    {
        float[,] rotateMatrix = new float[,] { { 0, 1 * (int)direction }, { -1 * (int)direction, 0 } };

        Vector2Int[] curCell = activePiece.TakingCells;
        Vector2Int[] rotatedCells = new Vector2Int[curCell.Length];

        for (int i = 0; i < rotatedCells.Length; i++)
        {
            rotatedCells[i].x = Mathf.RoundToInt((rotateMatrix[0, 0] * curCell[i].x) + (rotateMatrix[0, 1] * curCell[i].y));
            rotatedCells[i].y = Mathf.RoundToInt((rotateMatrix[1, 0] * curCell[i].x) + (rotateMatrix[1, 1] * curCell[i].y));
        }

        TetrisPiece rotatingPiece = activePiece.ShallowCopy();
        rotatingPiece.SetRotateStatus(direction, rotatedCells);

        Vector2Int[,] wallKicks = Data.WallKicks[rotatingPiece.tetromino.PieceAlphabet];

        int wallKickIndex = rotatingPiece.RotationIndex * 2;
        int rotationIndexLength = rotatingPiece.TakingCells.Length;
        if (direction == RotateDirection.Left)
        {
            wallKickIndex--;
        }
        wallKickIndex %= rotationIndexLength;

        for (int i = 0; i < rotationIndexLength; i++)
        {
            Vector3Int translation = (Vector3Int)wallKicks[wallKickIndex, i];
            Vector3Int translatedPosition = rotatingPiece.position + translation;
            if (IsValidPosition(translatedPosition, rotatedCells))
            {
                activePiece.SetRotateStatus(direction, rotatedCells);
                activePiece.Move(translation);
                return;
            }
        }
    }

    private void ClearFullLines()
    {
        int row = _OffSet.yMin;

        while (row < _OffSet.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        for (int col = _OffSet.xMin; col < _OffSet.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        for (int col = _OffSet.xMin; col < _OffSet.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        while (row < _OffSet.yMax)
        {
            for (int col = _OffSet.xMin; col < _OffSet.xMax; col++)
            {
                Vector3Int position;
                position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
