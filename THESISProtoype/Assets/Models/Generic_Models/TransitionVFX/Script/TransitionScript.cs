using System.Collections;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    private const float LIFETIME = 3f;
    private const float SHRINKTIME = 0.3f;
    private const float DELAY = 2f;
    Vector3 BASESCALE = new Vector3(25, 10, 25);
    Vector3 SHRINKSCALE = new Vector3(0.01f, 25, 0.01f);

    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    protected AudioSource sfxSource;
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

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, LIFETIME);
        StartCoroutine(DelayScale());
    }

    private void PlaySFX(int sfxIndex, float pitch, float volume)
    {
        // Null Check
        if (sfxSource != null && sfxSet[sfxIndex] != null)
        {
            //Set pitch first
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(sfxSet[sfxIndex], volume * volumeFactor);
        }
    }

    private IEnumerator DelayScale()
    {
        //Play ForceField SFX
        PlaySFX(0, 1.0f, 1);
        
        yield return new WaitForSeconds(DELAY);

        StartCoroutine(LocalScaleOverTime(this.gameObject, SHRINKTIME, SHRINKSCALE)); //Shrink

        //Play ShieldOff SFX and turn off previous sfx
        if(sfxSource != null)
            sfxSource.Stop();
        PlaySFX(1, 1.0f, 0.5f);

        yield return new WaitForSeconds(SHRINKTIME + 0.1f);

        this.gameObject.GetComponent<Renderer>().enabled = false; //Invisible
    }

    // Source: https://discussions.unity.com/t/how-to-gradually-scale-an-object-between-different-sizes/883714/3 by: sonicbelmont
    private IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        var startScale = obj.transform.localScale;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }
}
