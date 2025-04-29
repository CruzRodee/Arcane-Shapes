using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ShieldSpellScript : BaseLOScript
{
    private const float time = 0.5f;
    private Vector3 OFFSET = new Vector3(0, 0, -7f);
    private const float SCALING_VAR = 0.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 0.75f;
    private Vector3 ENDSCALE = new Vector3(0.75f, 0.75f, 0.75f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 5.0f, 7.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    new void Start()
    {
        base.Start();

        try
        {
            // Idicator VFX
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        }
    }

    public override void SuccessfulCast()
    {
        // Spawn shield spell with effect
        this.GetComponent<Renderer>().enabled = true;
        StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));

        try
        {
            // TODO: Add detonation vfx
            temp.Add(Instantiate(vfxSet[1], this.transform.position + OFFSET, Quaternion.identity));
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        }
    }
}
