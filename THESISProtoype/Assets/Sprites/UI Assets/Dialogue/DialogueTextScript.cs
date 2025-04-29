using UnityEngine;

public class DialogueTextScript : MonoBehaviour
{
    
    public string text;
    // public Text textbox;
    private int lineNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // textbox.text = "test"; //throw errors if unassigned to text (legacy)
        lineNum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))    //check if working with gesture
        {
            switch(lineNum){
                case 0:
                    text = "Eme di ko pa napagisipan anu sasabhiin nya";
                    break;
                default: break;
            }
        }

        // textbox.text = text;
    }

}
