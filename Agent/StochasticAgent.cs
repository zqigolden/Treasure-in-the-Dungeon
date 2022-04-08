using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StochasticAgent: MonoBehaviour
{
    int startRoomId;
    int endRoomId = -1;
    public StochasticAgent(int roomId)
    {
        startRoomId = roomId;
    }
    public List<StochasticAgent> Run(int pathNeed=1)
    {
        print("Run StochasticAgent at room: " + startRoomId + "/" + WorldControl.roomCount);
        var retry = 0;
        var newAgents = new List<StochasticAgent>();
        while (pathNeed > 0 && retry < 20)
        {
            retry++;
            // 0: top, 1: right, 2: bottom, 3: left
            var next = Random.Range(0, 4);

            Vector2Int startLoc = WorldControl.RandomLoc(startRoomId);
            if (startLoc == new Vector2Int(-1, -1))
            {
                return newAgents;
            }
            List<Vector2Int> path = WorldControl.GetPathBetweenRooms(startLoc, next);
            if (path == null)
            {
                continue;
            }
            Vector2Int endLoc = path[path.Count - 1];
            endRoomId = WorldControl.GetRoomId(endLoc);
            if (endRoomId == -1 || WorldControl.roomConnect[startRoomId, endRoomId] == 1)
            {
                continue;
            }
            if (endRoomId == 0)
            {
                WorldControl.AssignRoom(endLoc, ++WorldControl.roomCount);
                endRoomId = WorldControl.roomCount;
            }
            newAgents.Add(new StochasticAgent(endRoomId));
            path.Remove(endLoc);
            foreach (var loc in path)
            {
                WorldControl control = FindObjectOfType<WorldControl>();
                int[] tiles = { WorldControl.GetTile(startLoc), WorldControl.GetTile(endLoc) };
                control.ReplaceTile(loc, tiles[Random.Range(0, 2)]);
                
            }
            WorldControl.gateLoc[new Vector2Int(startRoomId, endRoomId)] = path[0];
            WorldControl.roomConnect[startRoomId, endRoomId] = 1;
            WorldControl.roomConnect[endRoomId, startRoomId] = 1;
            pathNeed--;
        }
        return newAgents;
    }
}
