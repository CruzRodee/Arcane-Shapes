using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions = new List<(InputAction, Action<InputAction.CallbackContext>)>();

        // Start is called before the first frame update
        private void Awake()
        {
            input = GetComponent<PlayerInput>();

            InitializeActions();
        }

        private void InitializeActions()
        {
            actions.Add((input.actions["Next"], OnNext));
        }

        private void OnEnable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed += inputAction.command;
        }

        private void OnDisable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed -= inputAction.command;
        }

        public void OnNext(InputAction.CallbackContext c)
        {
            Debug.LogFormat(
                "[Input] Action: {0}, Control: {1}, Device: {2}, Interaction: {3}, Phase: {4}",
                c.action?.name,
                c.control?.path,
                c.control?.device?.name,
                c.interaction?.GetType().Name ?? "None",
                c.phase
            );
            VNDialogueSystem.instance.OnUserPrompt_Next();
        }
    }
}


