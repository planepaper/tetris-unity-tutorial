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
        SpawnActivePiece();
    }

    private void Update()
    {
        ClearPieceTile(activePiece);

        Vector3Int newPosition = activePiece.position;
        if (Input.GetKeyDown(KeyCode.A))
        {
            newPosition += Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            newPosition += Vector3Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            newPosition += Vector3Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            newPosition = GetHardDropPosition();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateActivePiece(RotateDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateActivePiece(RotateDirection.Right);
        }
        else
        {
        }

        if (IsValidPosition(newPosition))
        {
            activePiece.Move(newPosition);
        }

        SetPieceTile();
    }

    public void SpawnActivePiece()
    {
        int randomIndex = Random.Range(0, tetrominoes.Length);
        activePiece = new TetrisPiece(tetrominoes[randomIndex]);

        SetPieceTile();
    }

    public void SetPieceTile()
    {
        foreach (var takingCell in activePiece.TakingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCell + activePiece.position;
            tilemap.SetTile(tilePosition, activePiece.tetromino.tile);
        }
    }

    public void ClearPieceTile(TetrisPiece piece)
    {
        foreach (var takingCells in activePiece.TakingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCells + activePiece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private Vector3Int GetHardDropPosition()
    {
        Vector3Int newPosition = activePiece.position;
        while (IsValidPosition(newPosition))
        {
            newPosition += Vector3Int.down;
        }

        newPosition += Vector3Int.up;

        return newPosition;
    }

    public bool IsValidPosition(Vector3Int position)
    {
        RectInt offset = OffSet;

        foreach (var takingCells in activePiece.TakingCells)
        {
            Vector3Int tilePosition = (Vector3Int)takingCells + position;

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

    private void RotateActivePiece(RotateDirection direction)
    {
        float[,] rotateMatrix = new float[,] { { 0, (-1) * (int)direction }, { 1 * (int)direction, 0 } };

        Vector2Int[] curCell = activePiece.TakingCells;
        Vector2Int[] rotatedCells = new Vector2Int[curCell.Length];

        for (int i = 0; i < rotatedCells.Length; i++)
        {
            rotatedCells[i].x = Mathf.RoundToInt((rotateMatrix[0, 0] * curCell[i].x) + (rotateMatrix[0, 1] * curCell[i].y));
            rotatedCells[i].y = Mathf.RoundToInt((rotateMatrix[1, 0] * curCell[i].x) + (rotateMatrix[1, 1] * curCell[i].y));
        }

        activePiece.RotateShape(rotatedCells);
    }
}
