using UnityEngine;

public class BellScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float SCALING_VAR = 1.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

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
        //Play SFX burst
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // Enable Mesh
        this.GetComponent<Renderer>().enabled = true;

        // Enable VFX
        this.transform.Find("MusicBurst").gameObject.SetActive(true);

        Invoke(nameof(Shaky), 2.1f);
        Invoke(nameof(Shaky), 4.1f);
        Invoke(nameof(Shaky), 6.1f);
    }

    private void Shaky()
    {
        //Shakycam
        cameraShakeScript.shakeDuration = 1.0f;
        cameraShakeScript.shakeAmount = 0.3f;

        //Bell SFX
        PlaySFX(sfxSet[2]);
    }
}
