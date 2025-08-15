// SOURCE: https://discussions.unity.com/t/how-do-i-detect-when-a-button-is-being-pressed-held-on-eventtype/596276/7

using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFXPlayer : MonoBehaviour, IPointerDownHandler
{
    public AudioClip btnSFX; //Audio clip containing the sound, assign in editor
    private AudioSource buttonAudio; //AudioSource to play the clip, assugn to the parent object during Awake()
    public float pitch = 3.0f; //Speed of audio clip, default 3.0f for radio button click
    public float volume = 1.0f;
    //Field that makes selecting the same type of button by code easier (0: Long sound, 1: short sound, more to come ???) 
    public int buttonSoundType = 0;

    private GameObject btnSFXObj; //Object that plays button sfx to prevent them from cutting out

    void Awake()
    {
        // Get existing AudioSource and object or add new one if it doesn't exist
        btnSFXObj = GameObject.Find("ButtonSFXPlayer");
        if(btnSFXObj == null)
        {
            btnSFXObj = new GameObject("ButtonSFXPlayer");
            DontDestroyOnLoad(btnSFXObj); //Keep this in memory to avoid recreating
        }

        buttonAudio = btnSFXObj.GetComponent<AudioSource>();
        if (buttonAudio == null)
        {
            buttonAudio = btnSFXObj.AddComponent<AudioSource>();
        }

        buttonAudio.playOnAwake = false;

        if (btnSFX == null)
            Debug.LogWarning($"ButtonSFXPlayer on {gameObject.name}: No audio clip assigned!");
    }

    //Detects whenever the button is pressed
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GlobalVariables.isMute && btnSFX != null)
        {
            buttonAudio.pitch = pitch;
            buttonAudio.volume = volume;
            buttonAudio.PlayOneShot(btnSFX);
        }   
    }
}
