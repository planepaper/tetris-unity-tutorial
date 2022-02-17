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
    private readonly Vector3Int _SPAWNPOSITION = new Vector3Int(-1, 8, 0);
    private readonly Vector2Int _BOARDSIZE = new Vector2Int(10, 20);


    private float stepDelay = 1f;
    private float lockDelay = 0.5f;
    private float stepTimeCount;
    private float lockTimeCount;

    private RectInt OffSet
    {
        get
        {
            Vector2Int position = new Vector2Int(-_BOARDSIZE.x / 2, -_BOARDSIZE.y / 2);
            return new RectInt(position, _BOARDSIZE);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        foreach (var tetromino in tetrominoes)
        {
            //TODO: Have to change this Logic
            //Change inserting method of tiles from unity-editor to in-code.
            tetromino.InitializeShapeCells();
        }
    }

    private void Start()
    {
        SpawnNewPiece();
        SetPieceTile();
    }

    private void Update()
    {
        ClearPieceTile();

        stepTimeCount += Time.deltaTime;
        lockTimeCount += Time.deltaTime;

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
        }
        else
        {
            StepPieceAfterStepDelay();
        }

        SetPieceTile();
    }

    private void TryToMovePiece(Vector3Int translation)
    {
        if (IsValidPositionOfActivePiece(activePiece.position + translation))
        {
            activePiece.Move(translation);
            lockTimeCount = 0f;
        }
    }

    private void StepPieceAfterStepDelay()
    {
        if (stepTimeCount >= stepDelay)
        {
            if (IsValidPositionOfActivePiece(activePiece.position + Vector3Int.down))
            {
                activePiece.Move(Vector3Int.down);
                stepTimeCount = 0f;
                lockTimeCount = 0f;
            }
            else
            {
                if (lockTimeCount >= lockDelay)
                {
                    LockPiece();
                }
            }
        }
    }

    private void LockPiece()
    {
        SetPieceTile();
        ClearLines();
        SpawnNewPiece();
    }

    public void SpawnNewPiece()
    {
        int randomIndex = Random.Range(0, tetrominoes.Length);
        activePiece = new TetrisPiece(tetrominoes[randomIndex]);

        stepTimeCount = 0f;
        lockTimeCount = 0f;
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
        while (IsValidPositionOfActivePiece(newPosition))
        {
            newPosition += Vector3Int.down;
            translation += Vector3Int.down;
        }
        translation += Vector3Int.up;
        ClearPieceTile();
        activePiece.Move(translation);
        LockPiece();
    }

    public bool IsValidPosition(Vector3Int position, Vector2Int[] takingCells)
    {
        RectInt offset = OffSet;

        foreach (var takingCell in takingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCell + position;

            if (!offset.Contains((Vector2Int)tilePosition))
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

    public bool IsValidPositionOfActivePiece(Vector3Int position)
    {
        return IsValidPosition(position, activePiece.TakingCells);
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
                lockTimeCount = 0f;
                return;
            }
        }
    }

    private void ClearLines()
    {
        RectInt bounds = OffSet;
        int row = bounds.yMin;

        while (row < bounds.yMax)
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
        RectInt bounds = OffSet;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
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
        RectInt bounds = OffSet;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
