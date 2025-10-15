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
            Character_Sprite Raelin = (Character_Sprite)CharacterManager.instance.CreateCharacter("Raelin");

            yield return new WaitForSeconds(1f);

            // Set whole model to red
            //Raelin.SetColor(Color.red);

            // Set individual layers to different colors
            //Raelin.spriteLayers[1].SetColor(Color.red);

            yield return Raelin.TransitionColor(Color.red, speed: 0.3f);
            Debug.Log("Set to Red");
            yield return Raelin.TransitionColor(Color.blue);
            Debug.Log("Set to Blue");
            yield return Raelin.TransitionColor(Color.yellow);
            Debug.Log("Set to Yellow");
            yield return Raelin.TransitionColor(Color.white);
            Debug.Log("Set to White");

            Debug.Log("Finished Testing Character Colors");

            yield return null;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}