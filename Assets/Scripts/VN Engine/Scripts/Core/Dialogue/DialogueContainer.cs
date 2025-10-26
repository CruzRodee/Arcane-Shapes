using System.Collections;
using CHARACTERS;
using GLTFast.Schema;
using TMPro;
using UnityEngine;


namespace DIALOGUE
{
    /// <summary>
    /// DialogueContainer holds references to the main dialogue UI elements for the visual novel system.
    /// </summary>
    [System.Serializable]
    public class DialogueContainer
    {
        private const float DEFAULT_FADE_SPEED = 3f;
        public GameObject dialogueBox;
        public NameContainer nameContainer;
        public TextMeshProUGUI dialogueText;

        private CanvasGroup dialogueBoxCG => dialogueBox.GetComponent<CanvasGroup>();

        private Coroutine co_showing = null;
        private Coroutine co_hiding = null;

        public bool isShowing => co_showing != null;
        public bool isHiding => co_hiding != null;
        public bool isFading => isShowing || isHiding;

        public bool isVisible => co_showing != null || dialogueBoxCG.alpha >= 1f;

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;

        public Coroutine Show()
        {
            if (isShowing)
                return co_showing;
            else if (isHiding)
            {
                VNDialogueSystem.instance.StopCoroutine(co_hiding);
                co_hiding = null;
            }

            co_showing = VNDialogueSystem.instance.StartCoroutine(Fading(1f));

            return co_showing;
        }

        public Coroutine Hide()
        {
            if (isHiding)
                return co_hiding;
            else if (isShowing)
            {
                VNDialogueSystem.instance.StopCoroutine(co_showing);
                co_showing = null;
            }

            co_hiding = VNDialogueSystem.instance.StartCoroutine(Fading(0f));

            return co_hiding;
        }

        private IEnumerator Fading(float alpha)
        {
            CanvasGroup cg = dialogueBoxCG;

            while (cg.alpha != alpha)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * DEFAULT_FADE_SPEED);
                yield return null;
            }

            co_showing = null;
            co_hiding = null;
        }
    }
}