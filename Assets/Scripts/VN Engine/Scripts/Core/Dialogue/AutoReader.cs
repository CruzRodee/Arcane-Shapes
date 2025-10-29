using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DIALOGUE
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        private const float READ_TIME_PADDING = 0.5f;
        private const float MAX_READ_TIME = 99f;
        private const float MIN_READ_TIME = 1.0f;
        private const string STATUS_TEXT_AUTO = "Auto";
        private const string STATUS_TEXT_SKIPPING = "Skipping";

        private ConversationManager conversationManager;
        private TextArchitect architect => conversationManager.architect;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;

        public bool isOn => co_running != null;
        private Coroutine co_running = null;

        [SerializeField] private TextMeshProUGUI statusText;

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            statusText.text = string.Empty;
        }

        public void Enable()
        {
            if (isOn)
                return;

            Debug.Log("[AutoReader] Enabling Auto Reader");
            co_running = StartCoroutine(AutoRead());
        }

        public void Disable()
        {
            if (!isOn)
                return;

            Debug.Log("[AutoReader] Disabling Auto Reader");
            StopCoroutine(co_running);
            skip = false;
            co_running = null;
            statusText.text = string.Empty;
        }

        private IEnumerator AutoRead()
        {
            // DO nothing if conversation is not running
            Debug.Log("[AutoReader] Auto Reader started");
            if (!conversationManager.isRunning)
            {
                Debug.Log("[AutoReader] Conversation is not running");
                Disable();
                yield break;
            }

            // If text is not being built but there is text on screen, prompt to continue
            if (!architect.isBuilding && architect.currentText != string.Empty)
                VNDialogueSystem.instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                // Read and wait for text to finish building
                if (!skip)
                {
                    while (!architect.isBuilding && !conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeStarted = Time.time;

                    while (architect.isBuilding || conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeToRead = Mathf.Clamp(((float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStarted)), MIN_READ_TIME, MAX_READ_TIME);
                    timeToRead = (timeToRead / speed) + READ_TIME_PADDING;

                    yield return new WaitForSeconds(timeToRead);
                }
                //Skip to the end of the text if true
                else
                {
                    architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                VNDialogueSystem.instance.OnSystemPrompt_Next();
            }

            Disable();
        }

        public void Toggle_Auto()
        {
            Debug.Log("[AutoReader] Toggling Auto Reader");

            if (skip)
            {
                skip = false;
                statusText.text = STATUS_TEXT_AUTO;
                Enable();
            }
            else
            {
                if (!isOn)
                {
                    statusText.text = STATUS_TEXT_AUTO;
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

        public void Toggle_Skip()
        {
            Debug.Log("[AutoReader] Toggling Skip");

            if (!skip)
            {
                skip = true;
                statusText.text = STATUS_TEXT_SKIPPING;
                Enable();
            }
            else
            {
                if (!isOn)
                {
                    statusText.text = STATUS_TEXT_SKIPPING;
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }
    }
}

