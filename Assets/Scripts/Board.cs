using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField]
    private Tetromino[] tetrominoes;
    private TetrisPiece activePiece;
    private Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

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

    public void SpawnActivePiece()
    {
        int randomIndex = Random.Range(0, tetrominoes.Length);
        activePiece = new TetrisPiece(tetrominoes[randomIndex]);

        SetPieceTile(activePiece);
    }

    public void SetPieceTile(TetrisPiece activePiece)
    {
        Vector2Int[] takingCoordinates = activePiece.tetromino.takingCoordinate;
        for (int i = 0; i < takingCoordinates.Length; i++)
        {
            Vector3Int tilePosition = (Vector3Int)takingCoordinates[i] + activePiece.position;
            tilemap.SetTile(tilePosition, activePiece.tetromino.tile);
        }
    }
}
