using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CreateHouseScript : BaseLOScript
{
    private const float SCALING_VAR = 0.1f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 ROTATEOFFSET = new Vector3(0.0f, -90.0f, 0.0f);
    private float ANIMTIME = 2.5f;
    private Material obj2mat, obj3mat; //Store original materials of object
    public Material manaMateriall;

    private GameObject house;

    private void Awake()
    {
        //Transforms and duration
        this.transform.localEulerAngles += ROTATEOFFSET;
        this.SPELLDURATION = 5.0f; // Set custom spell duration for longer/shorter spells

        //Get house...
        house = this.gameObject.transform.Find("HouseObject").gameObject;

        //Store original materials of object
        obj2mat = house.transform.Find("Object_2").gameObject.GetComponent<Renderer>().material;
        obj3mat = house.transform.Find("Object_3").gameObject.GetComponent<Renderer>().material;

        //Change materials to mana material
        house.transform.Find("Object_2").gameObject.GetComponent<Renderer>().material = manaMateriall;
        house.transform.Find("Object_3").gameObject.GetComponent<Renderer>().material = manaMateriall;

        // and deactivate object later
        house.SetActive(false);
    }

    public override void SuccessfulCast()
    {
        //Show House
        house.SetActive(true);

        //Scale to Max size
        StartCoroutine(LocalScaleOverTime(house, ANIMTIME-1f, SCALING));

        //Remove Mana Material
        Invoke(nameof(ShowObject), ANIMTIME);
    }

    private void ShowObject()
    {
        //Change materials back to orig
        house.transform.Find("Object_2").gameObject.GetComponent<Renderer>().material = obj2mat;
        house.transform.Find("Object_3").gameObject.GetComponent<Renderer>().material = obj3mat;

        //VFX
        this.transform.Find("MagicalBurst").gameObject.GetComponent<VisualEffect>().enabled = true;
    }
}
