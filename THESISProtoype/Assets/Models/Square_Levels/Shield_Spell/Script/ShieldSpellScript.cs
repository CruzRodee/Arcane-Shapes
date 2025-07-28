using System.Collections;
using UnityEngine;

public class ShieldSpellScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float time = 0.5f;
    private Vector3 OFFSET = new Vector3(0, 0, -7f);
    private const float SCALING_VAR = 0.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 0.75f;
    private Vector3 ENDSCALE = new Vector3(0.75f, 0.75f, 0.75f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 5.0f, 7.0f);
    private new void Awake()
    {
        base.Awake();

        this.transform.localPosition += SPAWNOFFSET;

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    new void Start()
    {
        base.Start();

        //Play burts SFX at low volume
        v[0] = 0.1f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);

        //VFX
        Invoke(nameof(IndicatorFlash), 0.05f);
    }

    private void IndicatorFlash()
    {
        // Idicator VFX
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
    }

    public override void SuccessfulCast()
    {
        //Check if sfx is loaded, do nothing if not
        if (sfxSet.Length < 5)
            return;

        // Spawn shield spell with effect
        this.GetComponent<Renderer>().enabled = true;
        StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));
        PlaySFX(sfxSet[2]); //Play Shield Up SFX

        //Detonation vfx and stuff
        StartCoroutine(CastAnimAndSFX());
    }

    private IEnumerator CastAnimAndSFX()
    {
        Instantiate(vfxSet[1], this.transform.position + OFFSET, Quaternion.identity);

        yield return new WaitForSeconds(CAST_DURATION + 0.25f); //Wait for vfx to match shaky cam and maybe sfx

        PlaySFX(sfxSet[3]); //Fire spell sfx
        Shaky(); //Shaky cam

        yield return new WaitForSeconds(0.25f); //Slight delay before shield reflect sound

        PlaySFX(sfxSet[4]);//Shield Reflect SFX
    }

    private void Shaky()
    {
        //Shakycam
        cameraShakeScript.shakeDuration = CAST_DURATION * 0.5f;
        cameraShakeScript.shakeAmount = 0.1f;
    }
}
