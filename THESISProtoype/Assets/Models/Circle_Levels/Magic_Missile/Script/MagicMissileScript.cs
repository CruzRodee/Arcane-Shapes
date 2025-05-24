using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class MagicMissileScript : BaseLOScript
{
    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private const float SCALING_VAR = 0.3f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 15.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 1.5f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // Enable VFX
        this.GetComponent<VisualEffect>().enabled = true;

        Invoke(nameof(Shaky), 0.2f);
    }

    private void Shaky()
    {
        //Shakycam
        cameraShakeScript.shakeDuration = 0.5f;
        cameraShakeScript.shakeAmount = 0.6f;
    }
}
