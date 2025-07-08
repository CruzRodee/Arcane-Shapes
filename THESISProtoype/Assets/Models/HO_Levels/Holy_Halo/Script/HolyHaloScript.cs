using System.Collections;
using UnityEngine;

public class HolyHaloScript : BaseLOScript
{
    private const float SCALING_VAR = 8f;
    private readonly Vector3 SCALING = new Vector3(SCALING_VAR, 4f, SCALING_VAR);

    private readonly Vector3 SPAWNOFFSET = new Vector3(0.0f, 0f, 0.0f);
    private readonly Vector3 HALORISEOFFSET = new Vector3(0.0f, 6.0f, 0.0f);
    private const float HALORISETIME = 1.0f;
    private const float HALOEXPANDTIME = 0.75f;

    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 6f; // Set custom spell duration for longer/shorter spells
    }

    public override void SuccessfulCast()
    {
        //Show Halo
        foreach (GameObject o in vfxSet)
        {
            o.SetActive(true);
        }

        //Play Bell SFX
        PlaySFX(sfxSet[0], 1, 0.1f);

        StartCoroutine(HaloAnims());
    }

    private IEnumerator HaloAnims()
    {
        yield return new WaitForSeconds(1.5f); //Wait for halo flash to finish and pause

        //Make Halo Rise
        foreach (GameObject o in vfxSet)
        {
            StartCoroutine(LocalMoveOverTime(o, HALORISETIME, HALORISEOFFSET));
        }

        //Play Lower pitch and volume halo sfx for rise
        PlaySFX(sfxSet[1], 0.75f, 0.5f);

        yield return new WaitForSeconds(HALORISETIME + 0.5f); //Wait for halo rise and dramatic pause

        //EXPAND HALO
        StartCoroutine(LocalScaleOverTime(vfxSet[0], HALOEXPANDTIME, SCALING));

        //Brighten Halo overtime
        StartCoroutine(LightRangeOverTime(vfxSet[1], HALOEXPANDTIME, 200f));

        //Play halo SFX after stopping previous instance
        sfxSource.Stop();
        PlaySFX(sfxSet[1], 1, 0.75f);
    }
}
