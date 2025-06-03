using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class GravityWellScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

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

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // Enable Model
        this.transform.Find("EventHorizon").gameObject.GetComponent<Renderer>().enabled = true;

        //Enable VFX
        this.GetComponent<VisualEffect>().enabled = true;

        // Black hole cubes after delay
        StartCoroutine(BlackHoleEffect());
    }

    private IEnumerator BlackHoleEffect()
    {
        //Suction effect

        yield return new WaitForSeconds(DELAY);

        //Shakycam
        cameraShakeScript.shakeDuration = CAST_DURATION * 1.75f;
        cameraShakeScript.shakeAmount = 0.3f;

        foreach (GameObject c in cubes)
        {
            StartCoroutine(MoveOverTime(c, CAST_DURATION, SINGULARITY));
        }

        //Cleanup

        yield return new WaitForSeconds(CAST_DURATION * 1.5f);

        foreach (GameObject c in cubes)
        {
            Destroy(c);
        }
    }
}
