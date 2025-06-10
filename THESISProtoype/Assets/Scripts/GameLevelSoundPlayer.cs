using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelSoundPlayer : MonoBehaviour
{
    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    private AudioSource sfxSource;
    private float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    void Awake()
    {
        //Create and attach AudioSource
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        sfxSource.playOnAwake = false;
        if (GlobalVariables.isMute) //Mute function
            volumeFactor = 0f;
    }

    public void PlayBGM(int clipIndex, float pitch = 1f, float volume = 1f)
    {
        if (sfxSource != null && sfxSet.Length > 0 && sfxSet[clipIndex] != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.loop = true; //Needs to loop since bgm
            sfxSource.volume = volume * volumeFactor;
            sfxSource.clip = sfxSet[clipIndex];
        } 

        sfxSource.Play();
    }
}
