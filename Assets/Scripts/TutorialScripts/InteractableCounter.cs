using UnityEngine;

public class InteractableCounter : MonoBehaviour
{
    //This is the door to the cashier part

    //What it does:
    //when player is near, show the TXT Interaction (F to open thingy),
    // which will open/close it

    [SerializeField] private Animator animCounter;
    [SerializeField] private MonoBehaviour cameraController;
    public GameObject pPressToInteract;
    [SerializeField] private DialogueSystem Msger;
    
    public bool isCounterUp = false;
    public bool isPlayerNear = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animCounter = transform.Find("CounterTop").GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerNear)   //check for player near
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                toggleCounter();    //test
                if(isCounterUp)
                {
                    //REMOVE THESE WHEN DONE TESTING THE CHOICES
                    cameraController.enabled = false; // Stop camera/player movement
                    Cursor.lockState = CursorLockMode.None; // Unlock the cursor for UI
                    Cursor.visible = true; // Show mouse
                }



            }
        }

        //check if other objects near (WIP idk if I will continue this)
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered Counter");
        //there can be multiple nearbys, player can pick up items
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            animCounter.SetBool("IsPlayerNearby", true);
            pPressToInteract.SetActive(true);
        }
    }


    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited Counter");
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
            animCounter.SetBool("IsPlayerNearby", false);
            pPressToInteract.SetActive(false);

            Msger.StartDialogue(2);    //is how its called, 1 is for number of chapter
        }
    }

    void toggleCounter(){
        isCounterUp = !isCounterUp;
        Debug.Log("Toggled Counter");
    }
}
