// SOURCE: https://discussions.unity.com/t/how-do-i-detect-when-a-button-is-being-pressed-held-on-eventtype/596276/7

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFXPlayer : MonoBehaviour, IPointerDownHandler
{
    public AudioClip btnSFX; //Audio clip containing the sound, assign in editor
    private AudioSource buttonAudio; //AudioSource to play the clip, assugn to the parent object during Awake()
    public float pitch = 3.0f; //Speed of audio clip, default 3.0f for radio button click
    
void Awake()
 {
    // Get existing AudioSource or add new one if it doesn't exist
    buttonAudio = GetComponent<AudioSource>();
    if (buttonAudio == null)
    {
        buttonAudio = gameObject.AddComponent<AudioSource>();
    }
    
    if (btnSFX == null)
    {
        Debug.LogWarning($"ButtonSFXPlayer on {gameObject.name}: No audio clip assigned!");
        return;
    }
    
     buttonAudio.clip = btnSFX; //Assign buttonSFX as audio clip
     buttonAudio.pitch = pitch; //Speed-up sfx
 }
    
    //Detects whenever the button is pressed
public void OnPointerDown(PointerEventData eventData)
 {
    if(!GlobalVariables.isMute && buttonAudio != null && buttonAudio.clip != null) //If sounds are not muted and audio is properly configured
         buttonAudio.Play(); //Play sound on button press
 }
}
