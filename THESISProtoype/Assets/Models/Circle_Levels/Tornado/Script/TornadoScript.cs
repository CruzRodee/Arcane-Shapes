using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TornadoScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float SCALING_VAR = 0.6f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 7f;
    private Vector3 ENDSCALE = new Vector3(0.12f, 0.12f, 0.12f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.2f, 0.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 6.0f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        //Enable spin
        base.SuccessfulCast();

        // Enable Model
        this.transform.Find("Sketchfab_model").gameObject.SetActive(true);

        // Scale larger
        StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));

        Invoke(nameof(Shaky), 0.6f);
    }

    private void Shaky()
    {
        //Shakycam
        cameraShakeScript.shakeDuration = CAST_DURATION * 1.1f;
        cameraShakeScript.shakeAmount = 0.1f;
    }
}
