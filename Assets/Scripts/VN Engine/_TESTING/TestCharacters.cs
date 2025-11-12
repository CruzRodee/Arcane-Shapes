using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace TESTING
{

    public class TestCharacters : MonoBehaviour
    {
        public TMP_FontAsset tempFont;

        // Start is called before the first frame update
        void Start()
        {
            // Character Oz = CharacterManager.instance.CreateCharacter("Oz");
            // Character Student = CharacterManager.instance.CreateCharacter("S");
            // Character Player = CharacterManager.instance.CreateCharacter("Player");

            StartCoroutine(Test2());
        }

        IEnumerator Test()
        {
            // List<string> lines = new List<string>()
            // {
            //     "Oz \"Hello! This is a test of the Say function.\"",
            //     "S \"This is another line of dialogue.\"",
            //     "Player \"And another one!\"",
            //     "",
            //     "This line has no speaker, so it should be treated as narration.",
            //     "Oz \"Finally, this is the last line.\""
            // };

            // yield return VNDialogueSystem.instance.Say(lines);

            Character Oz = CharacterManager.instance.CreateCharacter("Oz");
            Character Student = CharacterManager.instance.CreateCharacter("S");
            Character Player = CharacterManager.instance.CreateCharacter("Player");

            List<string> ozLines = new List<string>()
            {
                "Hello! This is a test of the Say function.",
                "This is the second line of dialogue.",
                "And this is the third and final line."
            };

            yield return Oz.Say(ozLines);

            // Test changing text customizations at runtime
            Oz.SetNameColor(Color.cyan);
            Oz.SetNameFont(tempFont);
            Oz.SetDialogueColor(Color.yellow);
            Oz.SetDialogueFont(tempFont);
            Oz.UpdateTextCustomizationsOnScreen();

            yield return Oz.Say("Now my name is cyan, my dialogue is yellow, and my font has changed!");

            Oz.ResetConfigurationData();
            Oz.UpdateTextCustomizationsOnScreen();

            yield return Oz.Say("Now I've reset my configuration data back to the original.");

            List<string> studentLines = new List<string>()
            {
                "Hi Oz!{c} This is Student speaking.",
                "I'm testing the Say function as well.",
                "It's. . .{wa 2} working great!"
            };

            yield return Student.Say(studentLines);

            yield return Player.Say("Hey everyone, Player here!{a} Just wanted to say hi.");

            Debug.Log("Finished");
        }


        IEnumerator Test2()
        {
            CharacterManager cm = CharacterManager.instance;
            Character Monk = cm.CreateCharacter("Monk as Generic");

            yield return Monk.Say("Normal dialogue configuration.");

            Monk.SetDialogueColor(Color.red);
            Monk.SetNameColor(Color.green);

            yield return Monk.Say("Modified dialogue configuration.");

            Monk.ResetConfigurationData();

            yield return Monk.Say("Reset dialogue configuration.");
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}