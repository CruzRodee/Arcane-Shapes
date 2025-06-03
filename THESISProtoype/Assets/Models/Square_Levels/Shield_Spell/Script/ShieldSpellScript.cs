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
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    new void Start()
    {
        base.Start();

        // Idicator VFX
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
    }

    public override void SuccessfulCast()
    {
        // Spawn shield spell with effect
        this.GetComponent<Renderer>().enabled = true;
        StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));

        // TODO: Add detonation vfx
        Instantiate(vfxSet[1], this.transform.position + OFFSET, Quaternion.identity);
        Invoke(nameof(Shaky), CAST_DURATION + 0.25f);
    }

    private void Shaky()
    {
        //Shakycam
        cameraShakeScript.shakeDuration = CAST_DURATION * 0.5f;
        cameraShakeScript.shakeAmount = 0.1f;
    }
}
