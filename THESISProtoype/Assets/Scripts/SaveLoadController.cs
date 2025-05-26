using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;


//save game file
[Serializable]
public class GameData
{
    public string playerName;
    public bool prefMute;   //settings stuff
    public string currRoom; //just room marker for the level selecvt thingy


    //levels
    public float squarePercent; //highest score - need to 100% a level in order to unlock next level?
    public int squareLvl;       //Lvl 0-3 - Beginner, Intermediate, Advanced
    public float circlePercent;
    public int circleLvl;
    public float scirclePercent;
    public int scircleLvl;
    public float rectPercent;
    public int rectLvl;
    public float triPercent;
    public int triLvl;

    //COMPOUND
    public int compLvl;
}

//basically saving loading from a JSON file lang lahat

public class SaveLoadController
{

    // private string savePath;
    public bool gameExists;

    public void saveGame(string savePath, string playerName, bool isMute, float squarePercent, int squareLvl,
                        float circlePercent, int circleLvl, float scirclePercent, int scircleLvl,
                        float rectPercent, int rectLvl, float triPercent, int triLvl, int compLvl, string currRoom)
    {
        GameData data = new GameData()
        {
            playerName = playerName,
            prefMute = isMute,

            squarePercent = squarePercent,
            squareLvl = squareLvl,
            circlePercent = circlePercent,
            circleLvl = circleLvl,
            scirclePercent = scirclePercent,
            scircleLvl = scircleLvl,
            rectPercent = rectPercent,
            rectLvl = rectLvl,
            triPercent = triPercent,
            triLvl = triLvl,

            currRoom = currRoom,

            compLvl = compLvl
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);

        Debug.Log("SUCCESS, Player Name:  " + data.playerName);

    }

    public void saveGame(string savePath, GameData data) // Just pass raw gamedata
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);

        Debug.Log("SUCCESS, Player Name:  " + data.playerName);

    }

    public void updateRoom(string savePath, GameData data, string newRoom) // Just pass raw gamedata
    {
        data.currRoom = newRoom;    //save only room
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);

        Debug.Log("SUCCESS, Player Name:  " + data.playerName);

    }


    public GameData loadGame(string savePath)
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("SUCCESS, Player Name:  " + loadedData.playerName);
            Debug.Log("Save File Found in: " + Application.persistentDataPath);
            return loadedData;
        }
        else
        {
            Debug.Log("No save data yet");
            return null;
        }
    }

    // // Start is called before the first frame update
    // void Start()
    // {
    //     savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
    //     if (File.Exists(savePath)){
    //         gameExists = true;
    //     }
    //     else{
    //         gameExists = false;
    //     }

    //     // if (loadGame() == 1) //success, open saved game

    //     //saveGame("Emerut", false,0,0,0,0,0,0,0,0,0,0,0);
    // }

    // savePath = Path.Combine(Application.persistentDataPath, "saveData.json"

}
