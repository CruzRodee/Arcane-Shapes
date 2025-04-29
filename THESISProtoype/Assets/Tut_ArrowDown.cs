using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tut_ArrowDown : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //bouncing arrow
        float y = Input.GetAxis("Vertical");
        transform.position = transform.position + new Vector3(0, y * 2f * Time.deltaTime, 0);
        Wait(1f);
        transform.position = transform.position + new Vector3(0, -y * 2f * Time.deltaTime, 0);
        Wait(1f);

    }

    private IEnumerator Wait(float delay){      
        while(true)
        {
            yield return new WaitForSeconds(delay);
        }
    }
}