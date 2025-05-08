using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TimeStopScript : BaseLOScript
{
    private bool timeStopped = false; //Boolean for time stop effect
    
    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);

    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    //Arrays for stones
    public GameObject innerStonesCenter;
    public GameObject outerStonesCenter;
    public GameObject[] pillarStones;
    private float[] floatSpeeds;
    private float[] rotateSpeeds;
    private float floatHeight = 0.25f; //

    //Rotation speeds for stones, spins 360 n times per second
    private float innerRotationSpeed = 1f;
    private float outerRotationSpeed = 0.1f;

    private const float GROWTIME = 0.75f;
    private readonly Vector3 STOPAREA = new Vector3(20f, 20f, 20f);

    public GameObject stopBubble;

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 1.5f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();

        //Generate Random Float and rotate speeds
        floatSpeeds = new float[pillarStones.Length];
        for (int i = 0; i < floatSpeeds.Length; i++)
        {
            floatSpeeds[i] = Random.Range(1f, 3f);
        }
        rotateSpeeds = new float[pillarStones.Length];
        for (int i = 0; i < rotateSpeeds.Length; i++)
        {
            rotateSpeeds[i] = Random.Range(0.05f, 0.075f);
        }
    }

    // Apply Object animations here
    void Update()
    {
        if (timeStopped)
            return; //If time is stopped no animations

        //Rotate inner stones
        innerStonesCenter.transform.Rotate(Vector3.up, 360 * innerRotationSpeed * Time.deltaTime);

        //Rotate outer stones, rotate opposite of inner
        outerStonesCenter.transform.Rotate(Vector3.down, 360 * outerRotationSpeed * Time.deltaTime);

        //source: https://discussions.unity.com/t/how-to-make-an-object-move-up-and-down-on-a-loop/612962/4 by: Diericx
        //Pillar stones floating and movement
        for (int i = 0; i < pillarStones.Length; i++)
        {
            Vector3 pos = pillarStones[i].transform.localPosition; //Store current position
            //Generate new Y coord with a sin function so it goes up and down, modulate it with float height and center on y pos by adding
            float newY = Mathf.Sin(Time.time * floatSpeeds[i]) * floatHeight + innerStonesCenter.transform.localPosition.y;
            pillarStones[i].transform.localPosition = new Vector3(pos.x, newY, pos.z); //Update Y position of stone

            //Rotate stones
            pillarStones[i].transform.Rotate(Vector3.up, 360 * rotateSpeeds[i] * Time.deltaTime);
        }
    }

    public override void SuccessfulCast()
    {
        //Rainbow burst vfx
        vfxSet[0].GetComponent<VisualEffect>().enabled = true;

        //Show timestop sphere
        stopBubble.SetActive(true);

        //Shakycam
        cameraShakeScript.shakeDuration = GROWTIME;
        cameraShakeScript.shakeAmount = 0.15f;

        //Grow sphere to cover
        StartCoroutine(LocalScaleOverTime(stopBubble, GROWTIME, STOPAREA));

        //Delayed flip of timeStopped bool by GROWTIME
        Invoke(nameof(DelayedStop), GROWTIME);
    }

    private void DelayedStop()
    {
        timeStopped = true;
    }
}
