using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    /// <summary>
    /// Testing_Architect is a MonoBehaviour script for testing the TextArchitect class.
    /// It displays random lines of dialogue using TextArchitect when the user presses the spacebar (desktop)
    /// or taps the screen (mobile).
    /// </summary>
    public class Testing_Architect : MonoBehaviour
    {
        /// <summary>
        /// Array of sample dialogue lines to display.
        /// </summary>
        string[] lines = new string[]
        {
            "Welcome to Arcana Academy!",
            "You must be one of the new students. Fresh faces always brighten the halls.",
            "Here, you'll learn how shapes and mana come together to form spells, wards, and enchantments that even the greatest mages rely on.",
            "Don't worry if it feels overwhelming at first. Every great wizard started with the basics, and trust me, the basics are far more important than you think.",
            "I once knew a student who dismissed the simple square as 'boring,' only to discover later that it formed the foundation of the most powerful barrier spells ever cast in our history.",
            "Remember this: every triangle, circle, and shape you draw is more than a lesson. It's a stepping stone toward shaping reality itself, bending the arcane into patterns you can command.",
            "For now, though, you should probably focus on finding your class before you get lost in these endless corridors."
        };
        /// <summary>
        /// Reference to the main dialogue system singleton.
        /// </summary>
        VNDialogueSystem ds;

        /// <summary>
        /// Reference to the TextArchitect instance for building dialogue text.
        /// </summary>
        TextArchitect architect;

        /// <summary>
        /// Initializes the dialogue system and TextArchitect on start.
        /// </summary>
        void Start()
        {
            Debug.Log("Testing TextArchitect");
            ds = VNDialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.instant;
        }

        /// <summary>
        /// Checks for user input each frame.
        /// On desktop, pressing the spacebar displays a random line.
        /// On mobile, tapping the screen displays a random line.
        /// </summary>
        void Update()
        {
            // Desktop: Press spacebar to display a random line
            if (Input.GetKeyDown(KeyCode.Space))
            {
                architect.Build(lines[Random.Range(0, lines.Length)]);
            }

            // Mobile: Tap the screen to display a random line
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                architect.Build(lines[Random.Range(0, lines.Length)]);
            }
        }
    }
}

