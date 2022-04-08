using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using NPBehave;

public class EnemyControl : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerControl player = null;
    public static List<GameObject> objects = new List<GameObject>();
    private Blackboard blackboard;
    Root behaviorTree;
    Root CreateBehaviourTree()
    {
        behaviorTree = new Root(new WaitForCondition(() => blackboard.Get<int>("ReadyMove") > 0,
            new Sequence(
                new Selector(
                    new Condition(()=> SeePlayer(), new NPBehave.Action(() => blackboard["Target"] = player.GetLoc())),
                    new Condition(()=> GetDisToPlayer() <= player.sound, new NPBehave.Action(
                        () => blackboard["Target"] = WorldControl.GetRoomCenter(WorldControl.GetRoomId(player.GetLoc())))),
                    new Condition(()=> blackboard.Get("Target") == null && GetRoomId() > 0, new NPBehave.Action(() =>
                    {
                        var rooms = WorldControl.AdjustRooms(GetRoomId());
                        var center = WorldControl.GetRoomCenter(rooms[UnityEngine.Random.Range(0, rooms.Count)]);
                        blackboard["Target"] = center;
                        print("Random Move to room " + center);
                    })),
                    new NPBehave.Action(() => RandomWalk())
                ),
                new NPBehave.Action(() => { 
                    blackboard["ReadyMove"] = blackboard.Get<int>("ReadyMove") - 1;
                    MoveTo(blackboard.Get<Vector2Int>("Target"));
                    if (GetLoc() == player.GetLoc())
                    {
                        FindObjectOfType<WorldControl>().Lose();
                    }
                    RemoveTargetIfArrived();
                })
                )));
        return behaviorTree;
    }

    public static void Trigger()
    {
        foreach (var obj in objects)
        {
            obj.GetComponent<EnemyControl>().blackboard["ReadyMove"] = obj.GetComponent<EnemyControl>().blackboard.Get<int>("ReadyMove") + 1;
        }
    }

    void RemoveTargetIfArrived()
    {
        if (blackboard.Get("Target") == null)
        {
            return;
        }
        //print("" + blackboard.Get<Vector2Int>("Target") + " " + GetLoc());
        var loc = blackboard.Get<Vector2Int>("Target");
        if (loc == GetLoc())
        {
            blackboard.Unset("Target");
            return;
        }
        if (WorldControl.GetTile(loc) == (int)WorldControl.TileType.Wall
            || WorldControl.GetTile(loc) == (int)WorldControl.TileType.OutOfBoard)
        {
            blackboard.Unset("Target");
            return;
        }
    }

    void Start()
    {
        player = FindObjectOfType<PlayerControl>();
        objects.Add(gameObject);
        blackboard = new Blackboard(UnityContext.GetClock());
        blackboard["ReadyMove"] = 0;
        behaviorTree = CreateBehaviourTree();
        behaviorTree.Start();
    }

    public Vector2Int GetLoc()
    {
        return WorldControl.GetLoc(transform.position);
    }

    public int GetRoomId()
    {
        return WorldControl.GetRoomId(GetLoc());
    }    

    public int GetDisToPlayer()
    {
        return WorldControl.GetDistance(GetLoc(), player.GetLoc());
    }
    public List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(target);
        while (cameFrom.ContainsKey(target))
        {
            target = cameFrom[target];
            path.Add(target);
        }
        path.Reverse();
        return path;
    }
    public List<Vector2Int> AStarSearch(Vector2Int start, Vector2Int target)
    {
        var closedSet = new HashSet<Vector2Int>();
        var openSet = new HashSet<Vector2Int>();
        openSet.Add(start);
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        gScore[start] = 0;
        var fScore = new Dictionary<Vector2Int, int>();
        fScore[start] = WorldControl.GetDistance(start, target);
        while (openSet.Count > 0)
        {
            var current = openSet.First(x => fScore[x] == openSet.Min(y => fScore[y]));
            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            closedSet.Add(current);
            //foreach (var neighbor in GetNeighbors(current))
            for (int i = 0; i < 4; i++)
            {
                var neighbor = current + WorldControl.GetDirection(i);
                if (WorldControl.GetTile(neighbor) == (int)WorldControl.TileType.Wall 
                    || WorldControl.GetTile(neighbor) == (int)WorldControl.TileType.OutOfBoard)
                {
                    continue;
                }
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }
                var tentativeGScore = gScore[current] + 1;
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + WorldControl.GetDistance(neighbor, target);
            }
        }
        return null;
    }
    public bool MoveTo(Vector2Int loc)
    {
        if (WorldControl.GetTile(loc) == (int)WorldControl.TileType.Wall
            || WorldControl.GetTile(loc) == (int)WorldControl.TileType.OutOfBoard)
        {
            return false;
        }
        if (GetLoc() == loc)
        {
            return true;
        }
        var path = AStarSearch(GetLoc(), loc);
        if (path == null || path.Count == 0)
        {
            return false;
        }
        var next = path[1];
        // print("" + next + " " + gameObject.transform.position + new Vector3(next.x, next.y, 0));
        transform.position = new Vector3(next.x, next.y, transform.position.z);
        return true;
    }
    public Vector2Int RandomWalk()
    {
        int dir = UnityEngine.Random.Range(0, 4);
        var loc = GetLoc() + WorldControl.GetDirection(dir);
        while (WorldControl.GetTile(loc) == (int)WorldControl.TileType.Wall
             || WorldControl.GetTile(loc) == (int)WorldControl.TileType.OutOfBoard)
        {
            dir = UnityEngine.Random.Range(0, 4);
            loc = GetLoc() + WorldControl.GetDirection(dir);
        }
        return loc;
    }


    public bool SeePlayer()
    {
        var loc = GetLoc();
        var diff = GetLoc() - player.GetLoc();
        while (diff.x != 0 || diff.y != 0)
        {
            if (WorldControl.GetTile(loc) == (int)WorldControl.TileType.Wall)
            {
                return false;
            }
            if (Math.Abs(diff.x) >= Math.Abs(diff.y))
            {
                loc.x -= Math.Sign(diff.x);
            }
            else
            {
                loc.y -= Math.Sign(diff.y);
            }
            diff = loc - player.GetLoc();
        }            
        return true;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
