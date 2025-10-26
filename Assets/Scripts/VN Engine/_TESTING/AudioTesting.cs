using System.Collections;
using System.Collections.Generic;
using CHARACTERS;
using UnityEngine;

namespace TESTING
{
    public class AudioTesting : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Running2());
        }

        Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        IEnumerator Running() // single sound effect test
        {
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Raelin.Show();
            yield return new WaitForSeconds(0.5f);

            AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_strong_01");

            yield return new WaitForSeconds(1f);
            Raelin.Animate("Hop");
            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Scared"), 1);
            Raelin.Say("Yikes!");
        }

        IEnumerator Running2() //looped sound effect test
        {
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Raelin.Show();

            AudioManager.instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop: true);

            yield return Raelin.Say("I'm going to turn off the radio.");

            AudioManager.instance.StopSoundEffect("RadioStatic");

            yield return Raelin.Say("That's better.");
        }

    }
}