using System.Collections;
using UnityEngine;

public class BaseLOScript : MonoBehaviour
{
    public GameObject[] vfxSet;
    protected Animator animator;
    public bool TEST = false;
    private const float TESTDELAY = 0.01f;
    protected float SPELLDURATION = 2.0f; // Default value for extra delay on winning, change for longer spells

    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    private AudioSource sfxSource;
    public int spellSoundType = 0;
    protected float[] p = { 1, 1 }, v = { 1, 1 }; //Default pitch and vol value for MagicBurst SFX, just put here cuz lazy
    protected float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    protected void Awake()
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

    protected void PlaySFX(AudioClip clip, float pitch = 1f, float volume = 1f)
    {
        if (sfxSource != null)
            sfxSource.pitch = pitch;
        
        if (sfxSet.Length > 0 && clip != null)
            sfxSource.PlayOneShot(clip, volume * volumeFactor);
    }

    protected void PlayRandomSFX(int clipsMaxIndex, float[] pitch, float[] volume)
    {
        int i = Random.Range(0, clipsMaxIndex + 1); //Add plus 1 to reach MaxIndex since exclusive

        PlaySFX(sfxSet[i], pitch[i], volume[i]);
    }

    public float GetSpellDuration() //Method for getting value
    {
        return SPELLDURATION;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        //TESTING
        if (TEST)
            Invoke(nameof(SuccessfulCast), TESTDELAY);

        //Invoke(nameof(CleanUp), CLEANTIME); // Cleaning objects
    }

    // Update is called once per frame
    //void Update()
    //{
    //
    //}

    //private void CleanUp()
    //{
    //    foreach(GameObject obj in temp)
    //    {
    //        GameObject.Destroy(obj);
    //    }
    //    temp = null;
    //}

    // Source: https://discussions.unity.com/t/how-to-gradually-scale-an-object-between-different-sizes/883714/3 by: sonicbelmont
    protected IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
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

    protected IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        var startPosition = obj.transform.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    protected IEnumerator LocalMoveOverTime(GameObject obj, float duration, Vector3 endLocalPosition)
    {
        var startLocalPosition = obj.transform.localPosition;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localPosition = endLocalPosition;
    }

    protected IEnumerator LocalEulerOverTime(GameObject obj, float duration, Vector3 endEulerAngle)
    {
        var startAngle = obj.transform.localEulerAngles;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.localEulerAngles = Vector3.Lerp(startAngle, endEulerAngle, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localEulerAngles = endEulerAngle;
    }

    protected IEnumerator LightRangeOverTime(GameObject obj, float duration, float endRange)
    {
        Light light = obj.GetComponent<Light>();

        var startRange = light.range;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            light.range = Mathf.Lerp(startRange, endRange, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        light.range = endRange;
    }

    public virtual void SuccessfulCast()
    {
        //Get animator component
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("successfulCast");
        }
    }
}
