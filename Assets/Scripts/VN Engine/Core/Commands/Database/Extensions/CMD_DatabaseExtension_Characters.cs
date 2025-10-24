using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHARACTERS;
using UnityEngine;


namespace COMMANDS
{
    public class CMD_DatabaseExtension_Characters : CMD_DatabaseExtension
    {
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enable", "-enabled" };
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAM_XPOS => new string[] { "-x", "-xpos", "-positionx" };
        private static string[] PARAM_YPOS => new string[] { "-y", "-ypos", "-positiony" };
        private static string[] PARAM_SPEED => new string[] { "-s", "-spd", "-speed" };
        private static string[] PARAM_SMOOTHING => new string[] { "-sm", "-smooth", "-smoothing" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
        }

        public static void CreateCharacter(string[] data)
        {
            string characterName = data[0];
            bool enable = false;
            bool immediate = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            Character character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enable)
                return;
            if (immediate)
                character.isVisible = true;
            else
                character.Show();
        }

        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null)
            {
                Debug.LogWarning($"CMD_DatabaseExtension_Characters.MoveCharacter: Character '{characterName}' not found.");
                yield break;
            }

            float x = 0, y = 0;
            float speed = 1;
            bool smoothing = false;
            bool immediate = false;

            var parameters = ConvertDataToParameters(data);

            // try to get the x axis position
            parameters.TryGetValue(PARAM_XPOS, out x);
            // try to get the y axis position
            parameters.TryGetValue(PARAM_YPOS, out y);
            // try to get the speed
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            // try to get the smoothing
            parameters.TryGetValue(PARAM_SMOOTHING, out smoothing, defaultValue: false);
            // try to get the immediate setting of position
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);


            Vector2 position = new Vector2(x, y);

            if (immediate)
            {
                Debug.Log($"CMD_DatabaseExtension_Characters.MoveCharacter: Moving character '{characterName}' to position {position} immediately.");
                character.SetPosition(position);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    Debug.Log($"CMD_DatabaseExtension_Characters.MoveCharacter: Completed moving character '{characterName}' to position {position}.");
                    character.SetPosition(position);
                });
                Debug.Log($"CMD_DatabaseExtension_Characters.MoveCharacter: Moving character '{characterName}' to position {position} with speed {speed} and smoothing {smoothing}.");
                yield return character.MoveToPosition(position, speed, smoothing);
            }
        }


        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("CMD_DatabaseExtension_Characters.ShowAll: No valid characters found to show.");
                yield break;
            }

            // Convert the data array to a parameter container
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // call the logic on all the characters
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;
                else
                    character.Show();
            }

            if (!immediate)
            {
                while (characters.Any(c => c.isRevealing))
                    yield return null;
            }
        }

        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("CMD_DatabaseExtension_Characters.ShowAll: No valid characters found to show.");
                yield break;
            }

            // Convert the data array to a parameter container
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // call the logic on all the characters
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = false;
                else
                    character.Hide();
            }

            if (!immediate)
            {
                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }


    }
}