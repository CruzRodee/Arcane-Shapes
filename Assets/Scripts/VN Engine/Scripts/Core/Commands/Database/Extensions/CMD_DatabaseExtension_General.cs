using System;
using System.Collections;
using System.Collections.Generic;
using COMMANDS;
using UnityEngine;


namespace Commands
{
    public class CMD_DatabaseExtension_General : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));

            // Dialogue Controls
            database.AddCommand("showdb", new Func<IEnumerator>(ShowDialogueBox));
            database.AddCommand("hidedb", new Func<IEnumerator>(HideDialogueBox));
        }

        public static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float waitTime))
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                Debug.LogWarning($"CMD_DatabaseExtension_General.Wait: Invalid wait time '{data}'.");
            }
        }

        private static IEnumerator ShowDialogueBox()
        {
            VNDialogueSystem.instance.dialogueContainer.Show();
            yield return null;
        }

        private static IEnumerator HideDialogueBox()
        {
            VNDialogueSystem.instance.dialogueContainer.Hide();
            yield return null;
        }
    }
}