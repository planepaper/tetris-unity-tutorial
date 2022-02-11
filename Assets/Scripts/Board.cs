using UnityEngine;
using UnityEngine.Tilemaps;

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
            tetromino.SetTakingCoordinate();
        }
    }

    private void Start()
    {
        SpawnActivePiece();
    }

    private void Update()
    {
        ClearPieceTile(activePiece);

        Vector2Int direction = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Vector2Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Vector2Int.down;
        }
        else
        {
        }

        Vector3Int newPosition = activePiece.position;
        newPosition.x += direction.x;
        newPosition.y += direction.y;

        if (IsValidPosition((Vector3Int)newPosition))
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
        foreach (var takingCoordinate in activePiece.tetromino.takingCoordinates)
        {
            Vector3Int tilePosition = (Vector3Int)takingCoordinate + activePiece.position;
            tilemap.SetTile(tilePosition, activePiece.tetromino.tile);
        }
    }

    public void ClearPieceTile(TetrisPiece piece)
    {
        foreach (var takingCoordinate in activePiece.tetromino.takingCoordinates)
        {
            Vector3Int tilePosition = (Vector3Int)takingCoordinate + activePiece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    //TODO: HARDDROP
    // private Vector3Int HardDrop()
    // {
    //     Vector3Int newPosition = activePiece.position;
    //     while (IsValidPosition(newPosition))
    //     {
    //         newPosition.x += Vector2Int.down.x;
    //         newPosition.y += Vector2Int.down.y;
    //     }


    //     activePiece.Move(newPosition);
    // }

    public bool IsValidPosition(Vector3Int position)
    {
        RectInt offset = OffSet;

        foreach (var takingCoordinate in activePiece.tetromino.takingCoordinates)
        {
            Vector3Int tilePosition = (Vector3Int)takingCoordinate + position;

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
}
