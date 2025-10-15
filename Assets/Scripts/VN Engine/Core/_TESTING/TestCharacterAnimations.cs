using System.Collections;
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace TESTING
{

    public class TestCharacterAnimations : MonoBehaviour
    {
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

            Raelin.SetPosition(new Vector2(0, 0));
            Student.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1f);

            Student.TransitionSprite(Student.GetSprite("female student 2 - surprised"));
            Student.TransitionSprite(Student.GetSprite("female student 2 - sad"));
            Student.Animate("Hop");
            yield return Student.Say("Where did this wind chill come from");

            Raelin.FaceRight();
            Student.Flip();
            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.MoveToPosition(new Vector2(0.1f, 0));
            Raelin.Animate("Shiver", true);
            yield return Raelin.Say("I don't know - but i hate it! {a} it's making me cold. It's freezing");

            Student.TransitionSprite(Student.GetSprite("female student 2 - happy"));
            yield return Student.Say("Oh, it's over!");

            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.Animate("Shiver", false);
            yield return Raelin.Say("Thank the Lord. . .{a} I'm not wearing enough clothes for that crap.");

            Debug.Log("Finished Testing Character Animations");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}