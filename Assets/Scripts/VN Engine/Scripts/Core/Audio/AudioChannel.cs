using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Audio Channel - [{0}]";
    public int channelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;

    private List<AudioTrack> tracks = new List<AudioTrack>();

    public AudioChannel(int channel)
    {
        channelIndex = channel;
        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channelIndex)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, string filePath)
    {
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            if (!existingTrack.isPlaying)
                existingTrack.Play();
            return existingTrack;
        }

        AudioTrack newTrack = new AudioTrack(clip, loop, startingVolume, volumeCap, this, AudioManager.instance.musicMixer);
        newTrack.Play();
        return newTrack;
    }

    public bool TryGetTrack(string trackName, out AudioTrack value)
    {
        trackName = trackName.ToLower();

        foreach (var track in tracks)
        {
            if (track.name.ToLower() == trackName)
            {
                value = track;
                return true;
            }
        }

        value = null;
        return false;
    }
}
