using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CubicBarrierScript : BaseLOScript
{
    private const float SCALING_VAR = 10f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.15f, 0.0f);
    private Vector3 SHIELDCENTER = new Vector3(0f, SCALING_VAR/2, 00f);

    private float TIMETOCENTER = 0.5f;
    private float EXPANDTIME = 0.2f;

    private GameObject cube;

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 3.0f; // Set custom spell duration for longer/shorter spells
        cube = this.transform.Find("BarrierCube").gameObject;
    }

    public override void SuccessfulCast()
    {
        //BURST VFX FLASH
        vfxSet[0].GetComponent<VisualEffect>().enabled = true;

        //Enable cube
        cube.SetActive(true);

        //Make cube constantly rotate
        StartCoroutine(LocalEulerOverTime(cube, 20f, new Vector3(0f, 359f, 0f)));

        //Coroutine anim
        StartCoroutine(CubicBarrierAnims());
    }

    private IEnumerator CubicBarrierAnims()
    {
        //Levitate cube to center
        StartCoroutine(LocalMoveOverTime(cube, TIMETOCENTER, SHIELDCENTER));
        yield return new WaitForSeconds(TIMETOCENTER + 0.3f); //Has additional delay for emphasis

        //Expand to SCALING
        StartCoroutine(LocalScaleOverTime(cube, EXPANDTIME, SCALING));

        //vfxSet[1] activate
        vfxSet[1].GetComponent<VisualEffect>().enabled = true;
    }
}
