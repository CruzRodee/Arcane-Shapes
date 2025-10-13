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
            Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");

            yield return new WaitForSeconds(1f);

            yield return Raelin.Hide();

            yield return new WaitForSeconds(1f);

            yield return Raelin.Show();

            yield return Raelin.Say("Hello there!");

            Debug.Log("Finished");

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}