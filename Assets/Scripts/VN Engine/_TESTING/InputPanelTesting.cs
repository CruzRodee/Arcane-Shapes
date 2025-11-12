using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;

public class InputPanelTesting : MonoBehaviour
{
    public InputPanel inputPanel; // ASSIGN THE INPUT PANEL MANAGER HERE

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        Character Raelin = CharacterManager.instance.CreateCharacter("Raelin", revealAfterCreation: true);

        yield return Raelin.Say("Hello! Please enter your name.");

        inputPanel.Show("Enter your name:");

        while (inputPanel.isWaitingOnUserInput)
        {
            yield return null;
        }

        string characterName = inputPanel.lastInput;

        yield return Raelin.Say($"Nice to meet you, {characterName}!");
    }
}
