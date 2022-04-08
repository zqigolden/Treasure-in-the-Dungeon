using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEngine.UI;
public class WorldControl : MonoBehaviour
{
    public static int height = 43;
    public static int width = 78;
    public GameObject[] tilePrefabs;
    static int[,] map = new int[height, width];
    static GameObject[,] objMap = new GameObject[height, width];
    static int[,] roomMap = new int[height, width];
    static bool mapLoad = false;
    public static int roomCount = 0;
    public static int[,] roomConnect = new int[100, 100];
    public static Dictionary<Vector2Int, Vector2Int> gateLoc = new Dictionary<Vector2Int, Vector2Int>();
    static System.Random random = new System.Random();
    public static Dictionary<Vector2Int, GameObject> deployedObjects = new Dictionary<Vector2Int, GameObject>();
    public GameObject gameResult;
    public GameObject overlapObj = null;
    public static bool gameOver = true;
    public enum TileType
    {
        OutOfBoard = -1,
        Wall = 0,
        Grass = 1,
        Sand = 2,
        Road = 3,
        Water = 4
    };
    // Start is called before the first frame update
    void Start()
    {
    }

    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public void Win()
    {
        print("Win");
        gameResult.SetActive(true);
        gameResult.GetComponent<Text>().text = "You Win!";
        gameOver = true;
    }

    public void Lose()
    {
        print("Lose");
        gameResult.SetActive(true);
        gameResult.GetComponent<Text>().text = "You Lose";
        gameOver = true;
    }
    public static int TagToInt(string tag)
    {
        return tag switch
        {
            "Wall" => (int)TileType.Wall,
            "Grass" => (int)TileType.Grass,
            "Sand" => (int)TileType.Sand,
            "Road" => (int)TileType.Road,
            "Water" => (int)TileType.Water,
            _ => (int)TileType.OutOfBoard,
        };
    }

    public static Vector2Int GetLoc(Vector3 vec3)
    {
        int x = Convert.ToInt32(vec3.x);
        int y = Convert.ToInt32(vec3.y);
        return new Vector2Int(x, y);
    }
    public void LoadMap()
    {
        mapLoad = false;
        roomCount = 0;
        roomConnect = new int[100, 100];
        gateLoc = new Dictionary<Vector2Int, Vector2Int>();
        deployedObjects = new Dictionary<Vector2Int, GameObject>();
        var tilesObj = overlapObj.transform.GetChild(0).GetChild(0);
        //var tilesObj = GameObject.Find("/Overlap/output-overlap/tiles");
        if (tilesObj == null || tilesObj.transform.childCount == 0)
        {
            return;
        }
        for (int i = 0; i < tilesObj.transform.childCount; i++)
        {
            var tile = tilesObj.transform.GetChild(i);
            var tag = tile.tag;
            var loc = GetLoc(tile.transform.position);
            var tileInt = TagToInt(tag);
            map[loc.y, loc.x] = tileInt;
            objMap[loc.y, loc.x] = tile.gameObject;
            if (tileInt == 0)
            {
                roomMap[loc.y, loc.x] = -1;
            }
            else
            {
                roomMap[loc.y, loc.x] = 0;
            }
        }
        print("Map load tiles: " + tilesObj.transform.childCount);
        mapLoad = true;
    }

    public static int GetTile(Vector2Int loc)
    {
        return GetTile(loc.x, loc.y);
    }

    public static List<int> AdjustRooms(int roomId)
    {
        var adjusts = new List<int>();
        for (int i = 0; i < roomCount; i++)
        {
            if (roomConnect[roomId, i] == 1)
            {
                adjusts.Add(i);
            }
        }
        return adjusts;
    }
    public static int GetTile(int x, int y)
    {
        if (map is null)
        {
            return -1;
        }
        if (y < 0 || y >= map.GetLength(0) || x < 0 || x >= map.GetLength(1))
        {
            return -1;
        }
        return map[y, x];
    }

    public void ReplaceTile(Vector2Int loc, int tile)
    {
        ReplaceTile(loc.x, loc.y, tile);
    }
    public void ReplaceTile(int x, int y, int tile)
    {
        if (map[y, x] == tile)
        {
            return;
        }
        map[y, x] = tile;
        var oldObj = objMap[y, x];
        var newType = tilePrefabs[tile];
        var newObj = GameObject.Instantiate(newType, parent: oldObj.transform.parent);
        newObj.transform.position = oldObj.transform.position;
        newObj.transform.rotation = oldObj.transform.rotation;
        newObj.transform.parent = oldObj.transform.parent;
        objMap[y, x] = newObj;
        //print("replace tile: " + x + " " + y + " from " + oldObj.tag + " to " + newType.tag);
        if (oldObj != null)
        {
            Destroy(oldObj);
        }
    }

    public static void AssignRoom(Vector2Int loc, int roomId)
    {
        AssignRoom(loc.x, loc.y, roomId);
    }
    public static void AssignRoom(int x, int y, int roomId)
    {
        if (y < 0 || y >= roomMap.GetLength(0)
            || x < 0 || x >= roomMap.GetLength(1)
            || roomMap[y, x] == roomId)
        {
            return;
        }
        if (roomMap[y, x] == -1)
        {
            return;
        }        
        {
            roomMap[y, x] = roomId;
            AssignRoom(x + 1, y, roomId);
            AssignRoom(x - 1, y, roomId);
            AssignRoom(x, y + 1, roomId);
            AssignRoom(x, y - 1, roomId);
        }
        
    }

    public static Vector2Int GetRoomCenter(int roomId) //room Id should larger than 0
    {
        int x = 0;
        int y = 0;
        int count = 0;
        for (int i = 0; i < roomMap.GetLength(0); i++)
        {
            for (int j = 0; j < roomMap.GetLength(1); j++)
            {
                if (roomMap[i, j] == roomId)
                {
                    x += j;
                    y += i;
                    count++;
                }
            }
        }
        var center = new Vector2Int(x / count, y / count);
        while (GetTile(center) == (int)TileType.Wall)
        {
            var randDir = GetDirection(UnityEngine.Random.Range(0, 4));
            center += randDir;
        }
        return center;
    }
    
    public void GenerateStochasticAgent()
    {
        int x = 0, y = 0;
        while (GetTile(x, y) == 0)
        {
            x++;
            y++;
        }
        if (GetRoomId(x, y) == 0)
        {
            AssignRoom(x, y, ++roomCount);
        }
        List<StochasticAgent> agentList = new List<StochasticAgent>();
        agentList.Add(new StochasticAgent(1));
        agentList.Add(new StochasticAgent(1));
        while (agentList.Count > 0)
        {
            agentList.AddRange(agentList[0].Run(1));
            agentList.RemoveAt(0);
        }
    }

    public static int GetRoomId(Vector2Int loc)
    {
        return GetRoomId(loc.x, loc.y);
    }
    public static int GetRoomId(int x, int y)
    {
        if (x < 0 || x >= roomMap.GetLength(1) || y < 0 || y >= roomMap.GetLength(0))
        {
            return -1;
        }
        return roomMap[y, x];
    }

    public static Vector2Int GetDirection(int dir)
    {
        switch (dir)
        {
            case 0:
                return new Vector2Int(0, 1);
            case 1:
                return new Vector2Int(1, 0);
            case 2:
                return new Vector2Int(0, -1);
            case 3:
                return new Vector2Int(-1, 0);
        }
        return new Vector2Int(0, 0);
    }
    public static List<Vector2Int> GetPathBetweenRooms(Vector2Int startLoc, int dir)
    {
        int roomId = GetRoomId(startLoc);
        var pathes = new List<Vector2Int>();
        while (GetRoomId(startLoc) == roomId)
        {
            startLoc = GetDirection(dir) + startLoc;
        }
        if (GetTile(startLoc) == -1)
        {
            return null;
        }
        if (GetTile(startLoc) == 0)
        {
            pathes.Add(startLoc);
            while (GetTile(startLoc) == 0)
            {
                pathes.Add(startLoc);
                startLoc = GetDirection(dir) + startLoc;
            }
            if (GetTile(startLoc) == -1)
            {
                return null;
            }
            pathes.Add(startLoc);
            return pathes;
        }
        return null;
    }
    public static Vector2Int RandomLoc(int roomId)
    {
        if (roomId <= roomCount && roomId > 0)
        {
            var locs = new List<Vector2Int>();
            for (int i = 0; i < roomMap.GetLength(0); i++)
            {
                for (int j = 0; j < roomMap.GetLength(1); j++)
                {
                    if (roomId == roomMap[i, j])
                    {
                        locs.Add(new Vector2Int(j, i));
                    }
                }
            }
            int index = random.Next(locs.Count);
            return (locs[index]);
        }
        return new Vector2Int(-1, -1);
    }
    static void DrawMapBase(int [,] map)
    {
        for (int y = map.GetLength(0) - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < map.GetLength(1); x++)
            {
                line += map[y, x];
            }
            print(line);
        }
    }

    public static void DrawRoomMap()
    {
        DrawMapBase(roomMap);
    }

    public static void DrawMap()
    {
        DrawMapBase(map);
    }

    // Update is called once per frame
    void Update()
    {
        if (!mapLoad)
        {
            LoadMap();
        }
    }
}
