using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            // Character Oz = CharacterManager.instance.CreateCharacter("Oz");
            // Character Student = CharacterManager.instance.CreateCharacter("S");
            // Character Player = CharacterManager.instance.CreateCharacter("Player");

            StartCoroutine(Test());
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

        // Update is called once per frame
        void Update()
        {

        }
    }
}