using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyHaloScript : BaseLOScript
{
    private const float SCALING_VAR = 8f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, 4f, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0f, 0.0f);
    private Vector3 HALORISEOFFSET = new Vector3(0.0f, 6.0f, 0.0f);
    private float HALORISETIME = 1.0f;
    private float HALOEXPANDTIME = 0.75f;

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 5f; // Set custom spell duration for longer/shorter spells
    }

    public override void SuccessfulCast()
    {
        //Show Halo
        foreach(GameObject o in vfxSet)
        {
            o.SetActive(true);
        }

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
        yield return new WaitForSeconds(HALORISETIME + 0.5f); //Wait for halo rise and dramatic pause

        //EXPAND HALO
        StartCoroutine(LocalScaleOverTime(vfxSet[0], HALOEXPANDTIME, SCALING));

        //Brighten Halo overtime
        StartCoroutine(LightRangeOverTime(vfxSet[1], HALOEXPANDTIME, 200f));
    }
}
