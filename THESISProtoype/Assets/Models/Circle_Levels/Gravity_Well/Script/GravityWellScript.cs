using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GravityWellScript : BaseLOScript
{
    private const float SCALING_VAR = 0.6f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 1.5f;
    private const float DELAY = 2.9f;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 2.0f, 0.0f);
    private Vector3 SINGULARITY;
    public GameObject[] cubes;

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        SINGULARITY = this.transform.position;
        this.SPELLDURATION = 6.0f; // Set custom spell duration for longer/shorter spells
    }

    public override void SuccessfulCast()
    {
        try
        {
            // VFX Graph flash
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Enable Model
            this.transform.Find("EventHorizon").gameObject.GetComponent<Renderer>().enabled = true;

            //Enable VFX
            this.GetComponent<VisualEffect>().enabled = true;

            // Black hole cubes after delay
            StartCoroutine(BlackHoleEffect());
        }
    }

    private IEnumerator BlackHoleEffect()
    {
        //Suction effect
        
        yield return new WaitForSeconds(DELAY);
        
        foreach (GameObject c in cubes)
        {
            StartCoroutine(MoveOverTime(c, CAST_DURATION, SINGULARITY));
        }

        //Cleanup

        yield return new WaitForSeconds(CAST_DURATION*1.5f);

        foreach (GameObject c in cubes)
        {
            Destroy(c);
        }
    }
}
