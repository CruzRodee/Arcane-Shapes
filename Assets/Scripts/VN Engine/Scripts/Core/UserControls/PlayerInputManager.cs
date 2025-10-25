using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Desktop:
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[Desktop] Space key pressed");
                PromptAdvance();
            }

            // Mobile:
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("[Mobile] Screen tapped");
                PromptAdvance();
            }
        }

        public void PromptAdvance()
        {
            VNDialogueSystem.instance.OnUserPrompt_Next();
        }
    }
}


