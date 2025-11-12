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
        private static string[] PARAM_COLOR_NAME => new string[] { "-c", "-color", "-colorname" };
        private static string[] PARAM_ONLY => new string[] { "-o", "-only" };
        private static string[] PARAM_SPRITE => new string[] { "-s", "-sprite" };
        private static string[] PARAM_LAYER => new string[] { "-l", "-layer" };


        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            database.AddCommand("sort", new Action<string[]>(Sort));
            database.AddCommand("highlight", new Func<string[], IEnumerator>(HighlightAll));
            database.AddCommand("unhighlight", new Func<string[], IEnumerator>(UnHighlightAll));

            // add commands to characters
            CommandDatabase baseCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(Show));
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(Hide));
            baseCommands.AddCommand("setpriority", new Action<string[]>(SetPriority));
            baseCommands.AddCommand("setposition", new Action<string[]>(SetPosition));
            baseCommands.AddCommand("setColor", new Func<string[], IEnumerator>(SetColor));
            baseCommands.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));
            baseCommands.AddCommand("unhighlight", new Func<string[], IEnumerator>(Unhighlight));

            // Add character specific databases
            CommandDatabase spriteCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_SPRITE);
            spriteCommands.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));
            spriteCommands.AddCommand("flip", new Func<string[], IEnumerator>(Flip));
            spriteCommands.AddCommand("faceleft", new Func<string[], IEnumerator>(FaceLeft));
            spriteCommands.AddCommand("faceright", new Func<string[], IEnumerator>(FaceRight));
        }

        #region GLOBAL COMMANDS

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

        private static void Sort(string[] data)
        {
            Debug.Log("[CMD_DatabaseExtension_Characters] Sort: Sorting characters.");
            CharacterManager.instance.SortCharacters();
        }
        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] MoveCharacter: Character '{characterName}' not found.");
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
                Debug.Log($"[CMD_DatabaseExtension_Characters] MoveCharacter: Moving character '{characterName}' to position {position} immediately.");
                character.SetPosition(position);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    Debug.Log($"[CMD_DatabaseExtension_Characters] MoveCharacter: Completed moving character '{characterName}' to position {position}.");
                    character.SetPosition(position);
                });
                Debug.Log($"[CMD_DatabaseExtension_Characters] MoveCharacter: Moving character '{characterName}' to position {position} with speed {speed} and smoothing {smoothing}.");
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
                Debug.LogWarning("[CMD_DatabaseExtension_Characters] ShowAll: No valid characters found to show.");
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
                Debug.LogWarning("[CMD_DatabaseExtension_Characters] ShowAll: No valid characters found to show.");
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

        public static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handleUnspecifiedCharacters = true;
            List<Character> unspecifiedCharacters = new List<Character>();

            //Add any characters specified to be highlighted
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("[CMD_DatabaseExtension_Characters] HighlightAll: No valid characters found to highlight.");
                yield break;
            }

            // Grab the extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_ONLY, out handleUnspecifiedCharacters, defaultValue: true);

            //Make all characters perform the logic
            foreach (Character character in characters)
                character.Highlight(immediate: immediate);

            //If we are forcing any unspecified characters to use the opposite highlighted status
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspecifiedCharacters.Add(character);
                    character.UnHighlight(immediate: immediate);
                }
            }

            // Wait for all characters to finish highlighting
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.Highlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (Character character in unspecifiedCharacters)
                        character.UnHighlight(immediate: true);
                });

                while (characters.Any(c => c.isHighlighting) || (handleUnspecifiedCharacters && unspecifiedCharacters.Any(c => c.isUnHighlighting)))
                    yield return null;
            }
        }

        public static IEnumerator UnHighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handleUnspecifiedCharacters = true;
            List<Character> unspecifiedCharacters = new List<Character>();

            //Add any characters specified to be highlighted
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("[CMD_DatabaseExtension_Characters] UnHighlightAll: No valid characters found to unhighlight.");
                yield break;
            }

            // Grab the extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_ONLY, out handleUnspecifiedCharacters, defaultValue: true);

            //Make all characters perform the logic
            foreach (Character character in characters)
                character.UnHighlight(immediate: immediate);

            //If we are forcing any unspecified characters to use the opposite highlighted status
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspecifiedCharacters.Add(character);
                    character.Highlight(immediate: immediate);
                }
            }

            // Wait for all characters to finish highlighting
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.UnHighlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (Character character in unspecifiedCharacters)
                        character.Highlight(immediate: true);
                });

                while (characters.Any(c => c.isUnHighlighting) || (handleUnspecifiedCharacters && unspecifiedCharacters.Any(c => c.isHighlighting)))
                    yield return null;
            }
        }
        #endregion GLOBAL COMMANDS

        #region  BASE CHARACTER COMMANDS

        private static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] Show: Character '{data[0]}' not found.");
                yield break;
            }

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = true;
            else
            {
                // A long running process should have a stop action to cancel out the coroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.isVisible = true;
                });

                yield return character.Show();
            }
        }

        private static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] Hide: Character '{data[0]}' not found.");
                yield break;
            }

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = false;
            else
            {
                // A long running process should have a stop action to cancel out the coroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.isVisible = false;
                });

                yield return character.Hide();
            }
        }

        public static void SetPosition(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            float x, y;

            if (character == null || data.Length < 2)
                return;

            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_XPOS, out x, defaultValue: 0);
            parameters.TryGetValue(PARAM_YPOS, out y, defaultValue: 0);

            character.SetPosition(new Vector2(x, y));
        }

        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            int priority;

            if (character == null || data.Length < 2)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetPriority: Character '{data[0]}' not found or invalid priority value.");
                return;
            }
            if (!int.TryParse(data[1], out priority))
            {
                priority = 0;
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetPriority: Invalid priority value '{data[1]}'. Defaulting to 0.");
            }

            character.SetPriority(priority);
        }

        public static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            string colorName;
            float speed;
            bool immediate;

            if (character == null || data.Length < 2)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetColor: Character '{data[0]}' not found or invalid color value.");
                yield break;
            }

            //Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            //Try to get color name
            parameters.TryGetValue(PARAM_COLOR_NAME, out colorName, defaultValue: data[1]);
            //Try to get speed of transition
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            //Try to get immediate flag
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            else
                immediate = false;

            // Get color value from the name
            Color color = Color.white;
            color = color.GetColorFromName(colorName);

            if (immediate)
                character.SetColor(color);
            else
            {
                // A long running process should have a stop action to cancel out the coroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.SetColor(color);
                });

                character.TransitionColor(color, speed);
            }

            yield break;
        }

        public static IEnumerator Highlight(string[] data)
        {
            //format: SetSprite(character sprite)
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character;

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetSprite: Character '{data[0]}' not found.");
                yield break;
            }

            bool immediate = false;

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
                character.Highlight(immediate: true);
            else
            {
                // A long running process should have a stop action to cancel out the coroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.Highlight(immediate: true);
                });

                yield return character.Highlight();
            }
        }

        public static IEnumerator Unhighlight(string[] data)
        {
            //format: SetSprite(character sprite)
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character;

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetSprite: Character '{data[0]}' not found.");
                yield break;
            }

            bool immediate = false;

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            if (immediate)
                character.UnHighlight(immediate: true);
            else
            {
                // A long running process should have a stop action to cancel out the coroutine and run logic that should complete this command
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.UnHighlight(immediate: true);
                });

                yield return character.UnHighlight();
            }
        }
        #endregion

        #region SPRITE CHARACTER COMMANDS

        public static IEnumerator SetSprite(string[] data)
        {
            //format: SetSprite(character sprite)
            Character_Sprite character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character_Sprite;
            int layer = 0;
            string spriteName;
            bool immediate = false;
            float speed;

            if (character == null || data.Length < 2)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] SetSprite: Character '{data[0]}' not found or is not a sprite character.");
                yield break;
            }

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            // Try to get the sprite name
            parameters.TryGetValue(PARAM_SPRITE, out spriteName, defaultValue: data[1]);
            // Try to get the layer
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);

            //Try to get the transition speed
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            //Try to get immediate flag
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            //Run the logic
            Sprite sprite = character.GetSprite(spriteName);
            if (sprite == null)
                yield break;

            if (immediate)
            {
                character.SetSprite(sprite, layer);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetSprite(sprite, layer); });
                yield return character.TransitionSprite(sprite, layer, speed);
            }
        }

        public static IEnumerator Flip(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            float speed;
            bool immediate;

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] Flip: Character '{data[0]}' not found.");
                yield break;
            }

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            // Try to get speed of transition
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // Try to get immediate flag
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            else
                immediate = false;

            if (immediate)
            {
                character.Flip(immediate: true);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.Flip(immediate: true);
                });

                yield return character.Flip(speed);
            }
        }

        public static IEnumerator FaceLeft(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            float speed;
            bool immediate;

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] FaceLeft: Character '{data[0]}' not found.");
                yield break;
            }

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            // Try to get speed of transition
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // Try to get immediate flag
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            else
                immediate = false;

            if (immediate)
            {
                character.FaceLeft(immediate: true);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.FaceLeft(immediate: true);
                });

                yield return character.FaceLeft(speed);
            }
        }

        public static IEnumerator FaceRight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            float speed;
            bool immediate;

            if (character == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_Characters] FaceRight: Character '{data[0]}' not found.");
                yield break;
            }

            // Grab extra parameters
            var parameters = ConvertDataToParameters(data, startingIndex: 1);

            // Try to get speed of transition
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // Try to get immediate flag
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            else
                immediate = false;

            if (immediate)
            {
                character.FaceRight(immediate: true);
            }
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    if (character != null)
                        character.FaceRight(immediate: true);
                });

                yield return character.FaceRight(speed);
            }
        }


        #endregion
    }
}