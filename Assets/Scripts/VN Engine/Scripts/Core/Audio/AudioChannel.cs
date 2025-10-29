using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Audio Channel - [{0}]";
    public int channelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;

    public AudioTrack activeTrack { get; private set; } = null;
    private List<AudioTrack> tracks = new List<AudioTrack>();

    bool isLevelingVolume => co_volumeLeveling != null;
    Coroutine co_volumeLeveling = null;

    public AudioChannel(int channel)
    {
        channelIndex = channel;
        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channelIndex)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, string filePath)
    {
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            if (!existingTrack.isPlaying)
                existingTrack.Play();

            SetAsActiveTrack(existingTrack);

            return existingTrack;
        }

        AudioTrack newTrack = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, AudioManager.instance.musicMixer);
        newTrack.Play();

        SetAsActiveTrack(newTrack);

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

    public void SetAsActiveTrack(AudioTrack track)
    {
        if (!tracks.Contains(track))
            tracks.Add(track);

        activeTrack = track;

        TryStartVolumeLeveling();
    }

    private void TryStartVolumeLeveling()
    {
        if (!isLevelingVolume)
            co_volumeLeveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
    }
    private IEnumerator VolumeLeveling()
    {
        // Continue while there's still "work" to do:
        // - If we have an activeTrack: keep looping while either there are other tracks that need fading out
        //   (tracks.Count > 1) OR the active track hasn't reached its target volume
        //   (activeTrack.volume != activeTrack.volumeCap).
        // - If there is no activeTrack: keep looping while any tracks remain (so they can be faded out and destroyed).
        while ((activeTrack != null && (tracks.Count > 1 || activeTrack.volume != activeTrack.volumeCap))
        || activeTrack == null && tracks.Count > 0)
        {
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                AudioTrack track = tracks[i];

                float targetVol = activeTrack == track ? track.volumeCap : 0f;

                if (track == activeTrack && track.volume == track.volumeCap)
                    continue;

                track.volume = Mathf.MoveTowards(track.volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);

                if (track != activeTrack && track.volume == 0)
                {
                    DestroyTrack(track);
                }

                yield return null;
            }
            co_volumeLeveling = null;
        }
    }

    private void DestroyTrack(AudioTrack track)
    {
        if (tracks.Contains(track))
        {
            tracks.Remove(track);
        }

        Object.Destroy(track.root);
    }

    internal void StopTrack()
    {
        if (activeTrack == null)
            return;

        activeTrack = null;
        TryStartVolumeLeveling();
    }
}
