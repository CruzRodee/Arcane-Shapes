using System.Collections;
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
            // Basic Test on Show/Hide
            // Character Raelin = CharacterManager.instance.CreateCharacter("Raelin");

            // yield return new WaitForSeconds(1f);

            // yield return Raelin.Hide();

            // yield return new WaitForSeconds(1f);

            // yield return Raelin.Show();

            // yield return Raelin.Say("Hello there!");


            // Guards Test
            // Character guard1 = CreateCharacter("Guard1 as Generic");
            // Character guard2 = CreateCharacter("Guard2 as Generic");
            // Character guard3 = CreateCharacter("Guard3 as Generic");

            // guard1.SetPosition(Vector2.zero);
            // guard2.SetPosition(new Vector2(0.5f, 0.5f));
            // guard3.SetPosition(Vector2.one);

            // guard1.Show();
            // guard2.Show();
            // guard3.Show();

            // guard1.SetDialogueFont(tempFont);
            // guard1.SetNameFont(tempFont);
            // guard2.SetDialogueColor(Color.cyan);
            // guard3.SetNameColor(Color.red);

            // yield return guard1.Say("Halt! Who goes there?");
            // yield return guard2.Say("State your business!");
            // yield return guard3.Say("You shall not pass!");

            Character Guard = CreateCharacter("Guard1 as Generic");
            Character Raelin = CreateCharacter("Raelin");
            Character Student = CreateCharacter("Female Student 2");

            Guard.SetPosition(Vector2.zero);
            Raelin.SetPosition(new Vector2(0.5f, 0.5f));
            Student.SetPosition(Vector2.one);

            Raelin.Show();
            Student.Show();

            yield return Guard.Show();
            yield return Guard.MoveToPosition(Vector2.one, smooth: true);
            yield return Guard.MoveToPosition(Vector2.zero, smooth: true);

            Guard.SetDialogueFont(tempFont);
            Guard.SetNameFont(tempFont);
            Raelin.SetDialogueColor(Color.cyan);
            Student.SetNameColor(Color.red);

            yield return Guard.Say("Halt! Who goes there?");
            yield return Raelin.Say("State your business!");
            yield return Student.Say("You shall not pass!");


            Debug.Log("Finished Testing Characters 2");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}