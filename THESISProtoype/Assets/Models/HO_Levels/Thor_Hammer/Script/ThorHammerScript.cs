using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class ThorHammerScript : BaseLOScript
{
    private readonly Vector3 SCALING = new Vector3(0.25f, 0.1370049f, 0.25f);

    private const float SCALETIME = 0.75f;
    private const float SWINGBACKTIME = 1.0f;
    private const float SLAMTIME = 0.15f;
    private const float SHAKETIME = 1.5f;

    private Material[] hammerMaterials; //Store original materials of object
    public Material manaMateriall;

    private GameObject hammer;

    //Gameobject for camera, aquired by name and used for shaky cam
    private CameraShake cameraShakeScript;
    public string cameraName = "ClassroomCamera"; //Default name

    public GameObject[] rainClouds; //Contains references to rain vfx objects

    private new void Awake()
    {
        base.Awake();
        //Transforms and duration
        this.SPELLDURATION = 6.0f; // Set custom spell duration for longer/shorter spells

        //Get Camera object
        cameraShakeScript = GameObject.Find(cameraName).GetComponent<CameraShake>();

        //Get hammer object
        hammer = this.gameObject.transform.Find("Hammer").gameObject;

        //Store original materials of object and change materials to mana material
        hammerMaterials = hammer.GetComponent<Renderer>().materials;
        Material[] tempManaMats = new Material[hammerMaterials.Length];
        hammerMaterials.CopyTo(tempManaMats, 0);
        for (int i = 0; i < tempManaMats.Length; i++)
        {
            tempManaMats[i] = manaMateriall;
        }
        hammer.GetComponent<Renderer>().materials = tempManaMats;
    }

    public override void SuccessfulCast()
    {
        //Show Hammer
        hammer.SetActive(true);

        //Scale to Max size
        StartCoroutine(LocalScaleOverTime(hammer, SCALETIME, SCALING));

        //Play humming SFX
        PlaySFX(sfxSet[2], 1, 0.5f);

        //Coroutine for anims
        StartCoroutine(ThorSpellAnims());
    }

    private Vector3 RandomRainPos()
    {
        float maxXY = 7f;
        return new Vector3(Random.Range(-maxXY, maxXY), Random.Range(3f, 7f), Random.Range(-maxXY, maxXY));
    }

    private IEnumerator ThorSpellAnims()
    {
        // Wait for scaling time
        yield return new WaitForSeconds(SCALETIME + 0.1f);

        //MagicBurst vfx1 + sfx + stop hum
        sfxSource.Stop();
        v[0] = 0.5f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);
        yield return new WaitForSeconds(0.05f); //Delay
        transform.Find("MagicalBurst1").gameObject.GetComponent<VisualEffect>().enabled = true;

        //Remove Mana Material
        hammer.GetComponent<Renderer>().materials = hammerMaterials;

        // Hold position for dramatic effect
        yield return new WaitForSeconds(0.25f);

        //Swing back to 45
        StartCoroutine(LocalEulerOverTime(hammer, SWINGBACKTIME, new Vector3(45f, 0f, 0f)));

        //Play Hammer SFX
        PlaySFX(sfxSet[3], 1, 1);

        // Hold position for dramatic effect
        yield return new WaitForSeconds(0.5f + SWINGBACKTIME);

        //Swing to -90
        StartCoroutine(LocalEulerOverTime(hammer, SLAMTIME, new Vector3(-90f, 0f, 0f)));

        // Delay effects until slam hits
        yield return new WaitForSeconds(0.05f + SLAMTIME);

        //Shaky cam
        cameraShakeScript.shakeDuration = SHAKETIME;

        //MagicBurst2 and Rain vfx(give random pos and enable VisualEffect)
        transform.Find("MagicalBurst2").gameObject.GetComponent<VisualEffect>().enabled = true;
        foreach (GameObject rain in rainClouds)
        {
            rain.transform.localPosition = RandomRainPos();
            rain.GetComponent<VisualEffect>().enabled = true;
        }

        //Play Rain SFX
        PlaySFX(sfxSet[4], 1, 0.5f);

        //Deactivate hammer (Hammer exploded on impact with ground)
        hammer.SetActive(false);
    }
}
