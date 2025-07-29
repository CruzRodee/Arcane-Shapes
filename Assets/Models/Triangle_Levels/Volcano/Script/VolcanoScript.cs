using UnityEngine;
using UnityEngine.VFX;

public class VolcanoScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float time = 1.0f;
    private const float ERUPTDURATION = 8.0f;
    private Vector3 OFFSET = new Vector3(0f, 0.5f, 0f);
    private const float SCALING_VAR = 2.0f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private GameObject temp1;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 5.0f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        //Play SFX burst at max volume
        v[0] = 1f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound

        // Enable Volcano object and its children
        this.GetComponent<Renderer>().enabled = true;
        this.transform.Find("Lava Glow").gameObject.SetActive(true);

        //Invoke Eruption animation
        Invoke(nameof(Eruption), time);
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
    }

    private void Eruption()
    {
        //Volcano SFX
        PlaySFX(sfxSet[2]);

        Invoke(nameof(DelayEruptVFX), 0.05f);
    }

    private void DelayEruptVFX()
    {
        temp1 = Instantiate(vfxSet[1], this.transform.position + OFFSET * 2, Quaternion.identity);
        Invoke(nameof(StopErupt), ERUPTDURATION);

        //Shakycam
        cameraShakeScript.shakeDuration = ERUPTDURATION * 0.25f;
        cameraShakeScript.shakeAmount = 0.6f;
    }

    private void StopErupt()
    {
        temp1.GetComponent<VisualEffect>().SetBool("isErupting", false);
        sfxSource.Stop(); //Stop SFX too
    }
}
