using System.Collections;
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace TESTING
{

    public class TestCharacterColors : MonoBehaviour
    {
        public TMP_FontAsset tempFont;
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        // Start is called before the first frame update
        void Start()
        {

            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;

            // yield return new WaitForSeconds(1f);

            // Set whole model to red
            //Raelin.SetColor(Color.red);

            // Set individual layers to different colors
            //Raelin.spriteLayers[1].SetColor(Color.red);

            // Testing color transitions
            // yield return Raelin.TransitionColor(Color.red, speed: 0.3f);
            // Debug.Log("Set to Red");
            // yield return Raelin.TransitionColor(Color.blue);
            // Debug.Log("Set to Blue");
            // yield return Raelin.TransitionColor(Color.yellow);
            // Debug.Log("Set to Yellow");
            // yield return Raelin.TransitionColor(Color.white);
            // Debug.Log("Set to White");

            // Testing highlighting
            // yield return new WaitForSeconds(1);
            // Debug.Log("Testing Highlighting");
            // yield return Raelin.UnHighlight();
            // yield return new WaitForSeconds(1);
            // yield return Raelin.TransitionColor(Color.red);
            // yield return new WaitForSeconds(1);
            // yield return Raelin.Highlight();
            // yield return new WaitForSeconds(1);
            // yield return Raelin.TransitionColor(Color.white);

            Raelin.SetPosition(Vector2.zero);
            Student.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1f);

            yield return Raelin.Flip(0.3f);
            yield return Student.FaceRight(immediate: true);
            yield return Raelin.FaceLeft(immediate: true);

            Student.UnHighlight();
            yield return Raelin.Say("I want to say something.");

            Raelin.UnHighlight();
            Student.Highlight();
            yield return Student.Say("But I want to say something else! {c}Can I go first?");

            Raelin.Highlight();
            Student.UnHighlight();
            yield return Raelin.Say("Sure, {a} be my guest.");

            Student.Highlight();
            Raelin.UnHighlight();
            Student.TransitionSprite(Student.GetSprite("female student 2 - happy"));
            yield return Student.Say("Yay! Thanks!");



            Debug.Log("Finished Testing Character Colors");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}