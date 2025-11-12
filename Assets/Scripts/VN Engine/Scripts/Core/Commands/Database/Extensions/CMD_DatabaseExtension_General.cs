using System;
using System.Collections;
using System.Collections.Generic;
using COMMANDS;
using DIALOGUE;
using UnityEngine;


namespace Commands
{
    public class CMD_DatabaseExtension_General : CMD_DatabaseExtension
    {
        private static readonly string[] PARAM_SPEED = new string[] { "-s", "-spd", "-speed" };
        private static readonly string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static readonly string[] PARAM_FILEPATH = new string[] { "-f", "-file", "-filepath" };
        private static readonly string[] PARAM_ENQUEUE = new string[] { "-e", "-enqueue" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));

            // Dialogue System Controls
            database.AddCommand("showui", new Func<string[], IEnumerator>(ShowDialogueSystem));
            database.AddCommand("hideui", new Func<string[], IEnumerator>(HideDialogueSystem));

            // Dialogue Controls
            database.AddCommand("showdb", new Func<string[], IEnumerator>(ShowDialogueBox));
            database.AddCommand("hidedb", new Func<string[], IEnumerator>(HideDialogueBox));

            database.AddCommand("load", new Action<string[]>(LoadNewDialogueFile));
        }

        private static void LoadNewDialogueFile(string[] data)
        {
            string fileName = string.Empty;
            bool enqueue = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_FILEPATH, out fileName);
            parameters.TryGetValue(PARAM_ENQUEUE, out enqueue, defaultValue: false);

            string filePath = FilePaths.GetPathToResource(FilePaths.resources_dialogueFiles, fileName);
            TextAsset file = Resources.Load<TextAsset>(filePath);

            if (file == null)
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_General.LoadNewDialogueFile] Could not find dialogue file at path '{filePath}'.");
                return;
            }

            List<string> lines = FileManager.ReadTextAsset(file, includeBlankLines: true);
            Conversation newConversation = new Conversation(lines);

            if (enqueue)
                VNDialogueSystem.instance.conversationManager.Enqueue(newConversation);
            else
                VNDialogueSystem.instance.conversationManager.StartConversation(newConversation);
        }

        public static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float waitTime))
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                Debug.LogWarning($"[CMD_DatabaseExtension_General.Wait] Invalid wait time '{data}'.");
            }
        }

        private static IEnumerator ShowDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return VNDialogueSystem.instance.dialogueContainer.Show(speed, immediate);
        }

        private static IEnumerator HideDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return VNDialogueSystem.instance.dialogueContainer.Hide(speed, immediate);
        }

        private static IEnumerator ShowDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return VNDialogueSystem.instance.Show(speed, immediate);
        }
        private static IEnumerator HideDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            yield return VNDialogueSystem.instance.Hide(speed, immediate);
        }
    }
}