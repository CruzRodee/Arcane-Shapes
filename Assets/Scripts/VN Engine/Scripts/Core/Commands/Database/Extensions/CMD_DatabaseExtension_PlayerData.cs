using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_PlayerData : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("saveplayerdata", new Action<string[]>(SavePlayerData));
            database.AddCommand("printplayerdata", new Action<string[]>(PrintPlayerData));
        }

        private static void SavePlayerData(string[] data)
        {
            if (PlayerDataManager.instance != null)
            {
                PlayerDataManager.instance.EndSession();
                Debug.Log("[CMD_DatabaseExtension_PlayerData] Player data saved successfully.");
            }
            else
            {
                Debug.LogError("[CMD_DatabaseExtension_PlayerData] PlayerDataManager instance not found!");
            }
        }

        private static void PrintPlayerData(string[] data)
        {
            if (PlayerDataManager.instance != null)
            {
                PlayerDataManager.instance.PrintCurrentSession();
            }
            else
            {
                Debug.LogError("[CMD_DatabaseExtension_PlayerData] PlayerDataManager instance not found!");
            }
        }
    }
}