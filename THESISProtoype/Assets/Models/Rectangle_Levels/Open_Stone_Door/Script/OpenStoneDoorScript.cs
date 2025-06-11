using UnityEngine;

public class OpenStoneDoorScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float time = 2.0f;
    private Vector3 OFFSET = new Vector3(0f, 0f, 0f);
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 2.0f;
    private Vector3 MOVEOFFSET = new Vector3(9f, 0, 0);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 3.3f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        
        this.transform.localPosition += SPAWNOFFSET;

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        //Play SFX burst
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound

        // Get the two door meshes
        GameObject door1 = this.transform.Find("Door1").gameObject;
        GameObject door2 = this.transform.Find("Door2").gameObject;

        // MoveOvertime for both
        StartCoroutine(MoveOverTime(door1, CAST_DURATION, this.transform.position + MOVEOFFSET));
        StartCoroutine(MoveOverTime(door2, CAST_DURATION, this.transform.position - MOVEOFFSET));

        //Play SFX for shaky
        PlaySFX(sfxSet[2]);

        //Shakycam
        cameraShakeScript.shakeDuration = CAST_DURATION * 1.1f;
        cameraShakeScript.shakeAmount = 0.05f;

        //Stop VFX same as shaky
        Invoke(nameof(StopSFX), CAST_DURATION * 1.1f);
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
    }

    private void StopSFX()
    {
        sfxSource.Stop();
    }
}
