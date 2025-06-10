using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelSoundPlayer : MonoBehaviour
{
    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    void Awake()
    {
        //Create and attach AudioSource for SFX
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        sfxSource.playOnAwake = false;
        if (GlobalVariables.isMute) //Mute function
            volumeFactor = 0f;

        //Create and attach AudioSource for BGM
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        bgmSource.playOnAwake = false;
        if (GlobalVariables.isMute) //Mute function
            volumeFactor = 0f;
    }

    public void PlayBGM(int clipIndex, float pitch = 1f, float volume = 1f)
    {
        if (bgmSource != null && sfxSet.Length > 0 && sfxSet[clipIndex] != null)
        {
            bgmSource.pitch = pitch;
            bgmSource.loop = true; //Needs to loop since bgm
            bgmSource.volume = volume * volumeFactor;
            bgmSource.clip = sfxSet[clipIndex];
        } 

        bgmSource.Play();
    }
    public void PlaySFX(int clipIndex, float pitch = 1f, float volume = 1f)
    {
        if (sfxSource != null)
            sfxSource.pitch = pitch;

        if (sfxSet.Length > 0 && sfxSet[clipIndex] != null)
            sfxSource.PlayOneShot(sfxSet[clipIndex], volume * volumeFactor);
    }
}
