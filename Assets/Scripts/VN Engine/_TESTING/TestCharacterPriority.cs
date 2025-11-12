using System.Collections;
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace TESTING
{

    public class TestCharacterPriority : MonoBehaviour
    {
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Guard = CreateCharacter("Guard1 as Generic") as Character_Sprite;
            Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

            GuardRed.SetColor(Color.red);

            Raelin.SetPosition(new Vector2(0.3f, 0));
            Student.SetPosition(new Vector2(0.45f, 0));
            Guard.SetPosition(new Vector2(0.6f, 0));
            GuardRed.SetPosition(new Vector2(0.75f, 0));


            yield return new WaitForSeconds(1f);

            GuardRed.SetPriority(1000);
            Student.SetPriority(15);
            Raelin.SetPriority(8);
            Guard.SetPriority(30);

            Debug.Log("Finished Testing Character Priority");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}