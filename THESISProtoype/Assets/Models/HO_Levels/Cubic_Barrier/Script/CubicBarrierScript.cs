using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CubicBarrierScript : BaseLOScript
{
    private const float SCALING_VAR = 10f;
    private readonly Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private readonly Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.15f, 0.0f);
    private readonly Vector3 SHIELDCENTER = new Vector3(0f, SCALING_VAR / 2, 00f);

    private const float TIMETOCENTER = 0.75f;
    private const float EXPANDTIME = 0.3f;

    private GameObject cube;

    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 6.0f; // Set custom spell duration for longer/shorter spells
        cube = this.transform.Find("BarrierCube").gameObject;
    }

    public override void SuccessfulCast()
    {
        //Play SFX burst at low volume
        v[0] = 0.5f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound

        //Enable cube
        cube.SetActive(true);

        //Make cube constantly rotate
        StartCoroutine(LocalEulerOverTime(cube, 20f, new Vector3(0f, 359f, 0f)));

        //Play cube hum at lower volume and higher pitch
        PlayContSFX(sfxSet[2], 2f, 0.05f);

        //Coroutine anim
        StartCoroutine(CubicBarrierAnims());
    }

    private void BurstVFX()
    {
        //BURST VFX FLASH
        vfxSet[0].GetComponent<VisualEffect>().enabled = true;
    }

    private IEnumerator CubicBarrierAnims()
    {
        //Levitate cube to center
        StartCoroutine(LocalMoveOverTime(cube, TIMETOCENTER, SHIELDCENTER));
        yield return new WaitForSeconds(TIMETOCENTER + 1.5f); //Has additional delay for emphasis

        //Expand to SCALING
        StartCoroutine(LocalScaleOverTime(cube, EXPANDTIME, SCALING));

        //play shield up
        PlaySFX(sfxSet[3], 0.75f, 0.6f);

        yield return new WaitForSeconds(0.1f);

        //Increase volume after delay
        sfxSource.volume = 1f;

        //vfxSet[1] activate
        vfxSet[1].GetComponent<VisualEffect>().enabled = true;
    }
}
