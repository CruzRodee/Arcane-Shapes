using UnityEngine;

public class EventManager : MonoBehaviour
{
    //the main gamne flow should be here
    [SerializeField] private DialogueSystem Msger;

    [SerializeField] private GameObject cupDetails;


    //Scripting of all event related stuff here
    //Like for example, when you get your 10th customer, the fucntion CALL_10THCUSTOMER()
    //And let's say that function will trigger to play the dialogue with script inside Msger (all script is there for dialogue)
    //And aside from that it triggers also the chime for closing to ring for example


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cupDetails.SetActive(true);   //need to connect this to the DrinkSystem.cs file for each drink that you are holding

        //test (WORKS NOW)
        // Msger.StartDialogue(1);    //is how its called, 1 is for number of chapter
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
