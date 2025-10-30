using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class ChoicePanelTest : MonoBehaviour
    {
        ChoicePanel panel;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(TestChoice());
        }

        IEnumerator TestChoice()
        {
            yield return null;

            panel = ChoicePanel.instance;
            string[] choices = new string[]
            {
                "Witness? Is that camera on?",
                "I choose you, Pikachu!",
                "To be, or not to be, that is the question.",
                "All your base are belong to us.",
                "The cake is a lie.",
            };

            panel.Show("Select your favorite quote:", choices);

            while (panel.isWaitingOnUserChoice)
            {
                yield return null;
            }

            var decision = panel.lastDecision;
            Debug.Log($"Made choice {decision.answerIndex}: {decision.choices[decision.answerIndex]}");
        }
    }
}