using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ChargedExplosionScript : BaseLOScript
{
    private const float SCALING_VAR = 1f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private float SCALETIME = 3f;
    private float SHAKETIME = 1.25f;

    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 6f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();
    }

    public override void SuccessfulCast()
    {
        // Show cube
        vfxSet[0].SetActive(true);

        // Rotate cube
        StartCoroutine(LocalEulerOverTime(vfxSet[0], 5f, new Vector3(0f, 359f, 0f)));

        //Enable vfx
        vfxSet[1].GetComponent<VisualEffect>().enabled = true;

        // Scaleup cube
        StartCoroutine(LocalScaleOverTime(vfxSet[0], SCALETIME, SCALING));

        // Coroutine for anims
        StartCoroutine(CESpell());
    }

    private IEnumerator CESpell()
    {
        yield return new WaitForSeconds(3f); //Wait until explosion

        // Remove cube
        vfxSet[0].SetActive(false);

        // Shaky cam
        cameraShakeScript.shakeAmount = 0.4f;
        cameraShakeScript.shakeDuration = SHAKETIME;
    }
}
