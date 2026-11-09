using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
    }

    private CellData[,] m_BoardData;

    private Tilemap m_Tilemap;

    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    public PlayerController Player;

    public Algorithms algorithm = Algorithms.Empty;

    public enum Algorithms
    {
        Empty,
        RandomWalk,
        SpelunkyStyle
    };

    // Start is called before the first frame update
    void Start()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();

        m_BoardData = new CellData[Width, Height];

        switch (algorithm)
        {
            case Algorithms.Empty:
                EmptyMap();
                break;
            case Algorithms.RandomWalk:
                RandomWalkWalls();
                break;
            case Algorithms.SpelunkyStyle:
                SpelunkyLikeWalls();
                break;
        }

        Player.Spawn(this, new Vector2Int(1, 1));
    }

    // This is the default map generation from the tutorial, I just extracted the tile selection in SetWall/SetGround to make it easier to reuse
    void EmptyMap()
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    SetWall(x, y);
                }
                else
                {
                    SetGround(x, y);
                }
            }
        }
    }

    //creates walls by making a constrained walk in the level (throw a 5 sided dice: 1-2 go left, 3-4 go right, 5 go up)
    void SpelunkyLikeWalls()
    {

        //the agent starts at (1,1)
        int agentX = 1;
        int agentY = 1;
        //in this list we save the visited nodes in the path
        List<Vector2Int> path = new List<Vector2Int>();
        //The CarveHole function "makes a hole" (meant as a ground tile) in the specified position and saves it in the path list
        CarveHole(agentX, agentY, ref path);

        /*
         * IMPLEMENT SPELUNKY-LIKE PATH CONSTRUCTION HERE
         */

        //Finally we fill all positions not part of path with walls
        CarveEntirePath(path);
    }

    //creates a path by randomly moving through the level
    void RandomWalkWalls()
    {
        //the agent starts at (1,1)
        int agentX = 1;
        int agentY = 1;
        //in this list we save the visited nodes in the path
        List<Vector2Int> path = new List<Vector2Int>();
        //The CarveHole function "makes a hole" (meant as a ground tile) in the specified position and saves it in the path list
        CarveHole(agentX, agentY, ref path);

        /*
         * IMPLEMENT RANDOM WALK HERE
         */

        //Finally we fill all positions not part of path with walls
        CarveEntirePath(path);
    }

    //this function checks if the position we're visiting has already been carved (is already in path)
    //if not, it adds it to the path
    void CarveHole(int x, int y, ref List<Vector2Int> path)
    {
        Vector2Int newPosition = new Vector2Int(x, y);
        if (!path.Contains(newPosition))
            path.Add(newPosition);
    }

    //this function fills the level by adding a random inner wall tile in every tile that is not in the calculated path
    void CarveEntirePath(List<Vector2Int> path)
    {
        //Fill in the level
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                m_BoardData[x, y] = new CellData();
                Vector2Int currentPosition = new Vector2Int(x, y);
                if (!path.Contains(currentPosition) ||
                (x == 0 || y == 0 || x == Width - 1 || y == Height - 1))
                {
                    SetWall(x, y);
                }
                else
                {
                    SetGround(x, y);
                }

            }
        }
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Tilemap.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    // I got annoyed about writing the Tile selection and updating the BoardData manually, so I decided to encapsulate it in a general SetTile function + added some shortcuts to set ground and wall tiles based on it

    /// <summary>
    /// Quick shortcut to set a random wall tile at (x,y), also updates the BoardData
    /// </summary>
    void SetWall(int x, int y)
    {
        SetTile(x, y, WallTiles, false);
    }

    /// <summary>
    /// Quick shortcut to set a random ground tile at (x,y), also updates the BoardData
    /// </summary>
    void SetGround(int x, int y)
    {
        SetTile(x, y, GroundTiles, true);
    }

    /// <summary>
    /// Selects a sprite tile, adds it to the Tilemap and updates data in the BoardData
    /// </summary>
    void SetTile(int x, int y, Tile[] potentialTiles, bool passable)
    {
        Tile tile = potentialTiles[Random.Range(0, potentialTiles.Length)];
        m_BoardData[x, y].Passable = passable;
        m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
}
