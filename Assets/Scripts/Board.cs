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

        Vector3Int translation = Vector3Int.zero;
        if (Input.GetKeyDown(KeyCode.A))
        {
            translation = Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            translation = Vector3Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            translation = Vector3Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            translation = GetHardDropTranslation();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            TryToRotateActivePiece(RotateDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryToRotateActivePiece(RotateDirection.Right);
        }
        else
        {
        }

        if (IsValidPositionOfActivePiece(activePiece.position + translation))
        {
            activePiece.Move(translation);
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

    private Vector3Int GetHardDropTranslation()
    {
        Vector3Int newPosition = activePiece.position;
        Vector3Int translation = Vector3Int.zero;
        while (IsValidPositionOfActivePiece(newPosition))
        {
            newPosition += Vector3Int.down;
            translation += Vector3Int.down;
        }

        return translation + Vector3Int.up;
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

    private void TryToRotateActivePiece(RotateDirection direction)
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
}
