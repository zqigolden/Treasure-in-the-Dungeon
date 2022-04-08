using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class Grammer : MonoBehaviour
{
    //S: start, T: target, K: Key, L: Lock, E: enemy, V:Event, N: Nothing
    List<KeyValuePair<string, string>> grammers = new List<KeyValuePair<string, string>>();
    public GameObject[] objectTable;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        grammers.Add(new KeyValuePair<string, string>("ST", "SVT"));
        grammers.Add(new KeyValuePair<string, string>("V", "VKVLV"));
        grammers.Add(new KeyValuePair<string, string>("V", "VEV"));
        grammers.Add(new KeyValuePair<string, string>("V", "VNV"));
        Random.InitState(System.DateTime.Now.Millisecond);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<List<int>> TopologicalSorting()
    {
        int roomCount = WorldControl.roomCount;
        List<List<int>> order = new List<List<int>>();
        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();
        List<int> temp = new List<int>();
        visited.Add(1);
        queue.Enqueue(1);
        order.Add(new List<int>(){1});
        while (queue.Count > 0)
        {
            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                for (int i = 1; i <= roomCount; i++)
                {
                    if (WorldControl.roomConnect[cur, i] == 1 && !visited.Contains(i))
                    {
                        visited.Add(i);
                        temp.Add(i);
                    }
                }
            }
            foreach (int i in temp)
            {
                queue.Enqueue(i);
            }            
            if (temp.Count > 0)
            {
                order.Add(temp);
                temp = new List<int>();
            }
        }
        return order;
    }

    public string GrammerTransform(string input)
    {
        int nextGram = Random.Range(0, grammers.Count);
        MatchCollection matches = Regex.Matches(input, grammers[nextGram].Key);
        if (matches.Count > 0)
        {
            var match = matches[Random.Range(0, matches.Count)];
            input = input.Substring(0, match.Index) + grammers[nextGram].Value + input.Substring(match.Index + grammers[nextGram].Key.Length);
        }
        return input;
    }

    public static int GramLen(string gram)
    {
        while (gram.Contains("V"))
        {
            gram = gram.Replace("V", "");
        }
        return gram.Length;
    }
    public string Generate()
    {
        string gram = "ST";
        var topo = TopologicalSorting();
        while (GramLen(gram) < topo.Count)
        {
            gram = GrammerTransform(gram);
        }
        while (gram.Contains("V"))
        {
            gram = gram.Replace("V", "");
        }
        if (gram.Length > topo.Count)
        {
            gram = gram.Substring(0, topo.Count - 1) + "T";
        }
        return gram;
    }

    public void DeployObjects()
    {
        string gram = "";
        while (!gram.Contains("E"))
        {
            gram = Generate();
        }
        print("Grammer:" + gram);
        var topo = TopologicalSorting();
        for (int i = 0; i < gram.Length; i++)
        {
            int j = Random.Range(0, topo[i].Count);
            int roomId = topo[i][j];
            Vector2Int loc = WorldControl.RandomLoc(roomId);
            GameObject gameObject = null;
            if (gram[i] == 'S')
            {
                gameObject = player;
            }
            else if (gram[i] == 'T')
            {
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[4]);
                WorldControl.deployedObjects[loc] = gameObject;
            }
            else if (gram[i] == 'K')
            {
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[1]);
                WorldControl.deployedObjects[loc] = gameObject;
            }
            else if (gram[i] == 'L')
            {
                for (int p = 0; p < topo[i].Count; p++)
                {
                    for (int q = 0; q < topo[i + 1].Count; q++)
                    {
                        if (WorldControl.roomConnect[topo[i][p], topo[i + 1][q]] == 1)
                        {
                            gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[2]);
                            
                            Vector2Int gateLoc;
                            if (WorldControl.gateLoc.ContainsKey(new Vector2Int(topo[i][p], topo[i + 1][q])))
                            {
                                gateLoc = WorldControl.gateLoc[new Vector2Int(topo[i][p], topo[i + 1][q])];
                            } else
                            {
                                gateLoc = WorldControl.gateLoc[new Vector2Int(topo[i + 1][q], topo[i][p])];
                            }
                            gameObject.transform.position = new Vector3(gateLoc.x, gateLoc.y, -0.5f);
                            WorldControl.deployedObjects[gateLoc] = gameObject;
                        }
                    }
                }
                continue;
            }
            else if (gram[i] == 'E')
            {
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[3]);
                gameObject.transform.position = new Vector3(loc.x, loc.y, -0.5f);
                loc = WorldControl.RandomLoc(roomId);
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[3]);
                gameObject.transform.position = new Vector3(loc.x, loc.y, -0.5f);
                loc = WorldControl.RandomLoc(roomId);
                gameObject = (GameObject)PrefabUtility.InstantiatePrefab(objectTable[3]);

            }
            else if (gram[i] == 'N')
            {
                continue;
            }
            gameObject.transform.position = new Vector3(loc.x, loc.y, -0.5f);
        }
        WorldControl.gameOver = false;
    }
}
