using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class ChargedExplosionScript : BaseLOScript
{
    private const float SCALING_VAR = 1f;
    private readonly Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private readonly Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private const float SCALETIME = 3f;
    private const float SHAKETIME = 1.25f;

    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    private new void Awake()
    {
        base.Awake();
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

        //Play humming SFX
        PlayContSFX(sfxSet[0], 0.9f, 5f);

        // Scaleup cube
        StartCoroutine(LocalScaleOverTime(vfxSet[0], SCALETIME, SCALING));

        // Coroutine for anims
        StartCoroutine(CESpell());
    }

    private IEnumerator CESpell()
    {
        yield return new WaitForSeconds(SCALETIME); //Wait until explosion

        // Remove cube
        vfxSet[0].SetActive(false);

        //Stop Previous SFX
        sfxSource.Stop();

        //Play Explosion SFX
        PlaySFX(sfxSet[1], 1, 5f);

        // Shaky cam
        cameraShakeScript.shakeAmount = 0.4f;
        cameraShakeScript.shakeDuration = SHAKETIME;
    }
}
