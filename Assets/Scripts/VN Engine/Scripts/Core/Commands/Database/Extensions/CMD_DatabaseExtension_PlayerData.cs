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
            database.AddCommand("savecheckpoint", new Action<string[]>(SaveCheckpoint));
            database.AddCommand("startsession", new Action<string[]>(StartSession));
        }

        private static void StartSession(string[] data)
        {
            PlayerDataManager.EnsureInstance();
            PlayerDataManager.instance.StartNewSession();
            Debug.Log("[CMD] New player session started.");
        }

        private static void SaveCheckpoint(string[] data)
        {
            if (PlayerDataManager.instance == null)
            {
                Debug.LogError("[CMD] PlayerDataManager instance not found!");
                return;
            }

            // First argument is checkpoint name
            string checkpointName = data.Length > 0 ? data[0] : "Unnamed_Checkpoint";

            // Remaining arguments are optional key-value pairs
            Dictionary<string, object> additionalData = null;

            if (data.Length > 1)
            {
                additionalData = new Dictionary<string, object>();

                // Parse remaining arguments as key:value pairs
                for (int i = 1; i < data.Length; i++)
                {
                    string[] kvp = data[i].Split(':');
                    if (kvp.Length == 2)
                    {
                        string key = kvp[0].Trim();
                        string value = kvp[1].Trim();

                        // Check if the value is a variable reference (starts with <$ and ends with >)
                        if (value.StartsWith("<$") && value.EndsWith(">"))
                        {
                            // Extract variable name (remove <$ and >)
                            string varName = value.Substring(2, value.Length - 3);

                            // Try to get the variable value from VariableStore
                            if (VariableStore.TryGetValue(varName, out object varValue))
                            {
                                additionalData[key] = varValue;
                                Debug.Log($"[CMD] Resolved variable ${varName} = {varValue}");
                            }
                            else
                            {
                                Debug.LogWarning($"[CMD] Variable ${varName} not found in VariableStore!");
                                additionalData[key] = value; // Store as-is if variable not found
                            }
                        }
                        else
                        {
                            // Store as literal value
                            additionalData[key] = value;
                        }
                    }
                }
            }

            PlayerDataManager.instance.SaveCheckpoint(checkpointName, additionalData);
            Debug.Log($"[CMD] Checkpoint '{checkpointName}' saved with {additionalData?.Count ?? 0} data entries.");
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