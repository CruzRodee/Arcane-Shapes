using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace TESTING
{

    public class TestCharacters2 : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        // Start is called before the first frame update
        void Start()
        {

            // Character raelin = CharacterManager.instance.CreateCharacter("Raelin");
            // Character Oz = CharacterManager.instance.CreateCharacter("Oz");
            // Character Student = CharacterManager.instance.CreateCharacter("S");
            // Character Player = CharacterManager.instance.CreateCharacter("Player");

            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            // Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");

            // yield return new WaitForSeconds(1f);

            // yield return Raelin.Hide();

            // yield return new WaitForSeconds(1f);

            // yield return Raelin.Show();

            // yield return Raelin.Say("Hello there!");

            Character guard1 = CreateCharacter("Guard1 as Generic");
            Character guard2 = CreateCharacter("Guard2 as Generic");
            Character guard3 = CreateCharacter("Guard3 as Generic");

            guard1.Show();
            guard2.Show();
            guard3.Show();

            guard1.SetDialogueFont(tempFont);
            guard1.SetNameFont(tempFont);
            guard2.SetDialogueColor(Color.cyan);
            guard3.SetNameColor(Color.red);

            yield return guard1.Say("Halt! Who goes there?");
            yield return guard2.Say("State your business!");
            yield return guard3.Say("You shall not pass!");

            Debug.Log("Finished Testing Characters 2");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}