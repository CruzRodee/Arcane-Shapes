using System;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public class CommandDatabase
    {
        private Dictionary<string, Delegate> database = new Dictionary<string, Delegate>();

        public bool HasCommand(string commandName) => database.ContainsKey(commandName.ToLower());

        public void AddCommand(string commandName, Delegate command)
        {
            commandName = commandName.ToLower();

            if (!database.ContainsKey(commandName))
                database.Add(commandName, command);
            else
                Debug.LogWarning($"Command '{commandName}' already exists in the database.");
        }

        public Delegate GetCommand(string commandName)
        {
            commandName = commandName.ToLower();

            if (!database.ContainsKey(commandName))
            {
                Debug.LogWarning($"Command '{commandName}' does not exist in the database.");
                return null;
            }
            else
                return database[commandName];
        }

    }
}


