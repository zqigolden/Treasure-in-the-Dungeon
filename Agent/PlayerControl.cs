using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public int keyCount = 0;
    public float moveValue = 0;
    public int sound = 0;
    public Text keyText;
    public Text soundText;
    public Text speedText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateKeyText()
    {
        keyText.text = "Keys: " + keyCount;
    }

    void UpdateSpeedText(string speed)
    {
        speedText.text = "Speed: " + speed;
    }
    void UpdateSoundText()
    {
        soundText.text = "Footstep Sound: " + sound + " blocks";
    }

    void OnMove(InputValue inputValue)
    {
        if (WorldControl.gameOver)
        {
            return;
        }
        Vector2 move = inputValue.Get<Vector2>();
        int x = Convert.ToInt32(move.x);
        int y = Convert.ToInt32(move.y);
        if ((x != 0 && y != 0) || (x == 0 && y == 0))
        {
            return;
        }
        Vector2Int newLoc = GetLoc() + new Vector2Int(x, y);
        if (WorldControl.GetTile(newLoc) == (int)WorldControl.TileType.Wall
            || WorldControl.GetTile(newLoc) == (int)WorldControl.TileType.OutOfBoard)
        {
            return;
        }
        if (WorldControl.deployedObjects.ContainsKey(newLoc) && WorldControl.deployedObjects[newLoc].tag == "Key")
        {
            keyCount++;
            WorldControl.deployedObjects[newLoc].SetActive(false);
            WorldControl.deployedObjects.Remove(newLoc);
        }
        if (WorldControl.deployedObjects.ContainsKey(newLoc) && WorldControl.deployedObjects[newLoc].tag == "Gate")
        {
            if (keyCount == 0)
            {
                return;
            }
            keyCount--;
            WorldControl.deployedObjects[newLoc].SetActive(false);
            WorldControl.deployedObjects.Remove(newLoc);
        }
        if (WorldControl.deployedObjects.ContainsKey(newLoc) && WorldControl.deployedObjects[newLoc].tag == "Target")
        {
            FindObjectOfType<WorldControl>().Win();
        }
        
        transform.position += new Vector3(x, y, 0);
        
        switch (WorldControl.GetTile(newLoc))
        {
            case (int)WorldControl.TileType.Grass:
                moveValue += 1.25f;
                sound = 5;
                UpdateSpeedText("A little slow");
                break;
            case (int)WorldControl.TileType.Sand:
                moveValue += 1f;
                sound = 15;
                UpdateSpeedText("Normal speed");
                break;
            case (int)WorldControl.TileType.Road:
                moveValue += 0.5f;
                sound = 15;
                UpdateSpeedText("Running on the road");
                break;
            case (int)WorldControl.TileType.Water:
                moveValue += 2f;
                sound = 30;
                UpdateSpeedText("Trapped in the water");
                break;
            default:             
                break;
        }
        UpdateSoundText();
        UpdateKeyText();

        while (moveValue >= 1.0f)
        {
            EnemyControl.Trigger();
            moveValue -= 1.0f;
        }

        // print("" + GetLoc() + " " + WorldControl.GetRoomId(GetLoc()));
        // WorldControl control = FindObjectOfType<WorldControl>();
        // control.ReplaceTile(GetLoc(), 0);
    }

    public Vector2Int GetLoc()
    {
        return WorldControl.GetLoc(transform.position);
    }
}
