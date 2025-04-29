using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    private const float LIFETIME = 3f;
    private const float SHRINKTIME = 0.3f;
    private const float DELAY = 2f;
    Vector3 BASESCALE = new Vector3(25, 10, 25);
    Vector3 SHRINKSCALE = new Vector3(0.01f, 25, 0.01f);

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, LIFETIME);
        StartCoroutine(DelayScale());
    }

    private IEnumerator DelayScale()
    {
        yield return new WaitForSeconds(DELAY);
        StartCoroutine(LocalScaleOverTime(this.gameObject, SHRINKTIME, SHRINKSCALE)); //Shrink
        yield return new WaitForSeconds(SHRINKTIME+0.1f);
        this.gameObject.GetComponent<Renderer>().enabled = false; //Invisible
    }

    // Source: https://discussions.unity.com/t/how-to-gradually-scale-an-object-between-different-sizes/883714/3 by: sonicbelmont
    private IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        var startScale = obj.transform.localScale;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }
}
