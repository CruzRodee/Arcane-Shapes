using UnityEngine;
using UnityEngine.VFX;

public class SlashScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    public GameObject cube1, cube2;

    private const float SCALING_VAR = 1.2f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private Vector3 CUTDIF = new Vector3(0, 0, 0.2f);
    private const float FLASHDELAY = 0.2f;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

    private new void Awake()
    {
        base.Awake();
        
        this.transform.localPosition += SPAWNOFFSET;

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        // Enable VFX
        this.GetComponent<VisualEffect>().enabled = true;

        // VFX flash and move
        Invoke(nameof(Flash), FLASHDELAY + 0.05f);
    }

    private void Flash()
    {
        Instantiate(vfxSet[0], this.transform.Find("Spark").position, this.transform.rotation).transform.localScale = SCALING;

        // Move Cubes sideways a bit for slash gap
        cube1.transform.localPosition -= CUTDIF;
        cube2.transform.localPosition += CUTDIF;

        //Shakycam
        cameraShakeScript.shakeDuration = 0.5f;
        cameraShakeScript.shakeAmount = 0.1f;

        //Play Random Slash Sound Effect
        float[] pitch = { 1, 1, 1, 1, 1 };
        float[] volume = { 1, 1, 1, 1, 1 };
        PlayRandomSFX(2, pitch, volume);
    }
}
