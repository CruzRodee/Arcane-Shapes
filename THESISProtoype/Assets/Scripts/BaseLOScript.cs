using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BaseLOScript : MonoBehaviour
{
    public GameObject[] vfxSet;
    protected Animator animator;
    [SerializeReference] protected List<GameObject> temp;
    public bool TEST = false;
    private const float CLEANTIME = 15.0f;
    private const float TESTDELAY = 0.01f;
    protected float SPELLDURATION = 2.0f; // Default value for extra delay on winning, change for longer spells

    public float GetSpellDuration() //Method for getting value
    {
        return SPELLDURATION;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        //TESTING
        if(TEST)
            Invoke(nameof(SuccessfulCast), TESTDELAY);

        Invoke(nameof(CleanUp), CLEANTIME); // Cleaning objects
    }

    // Update is called once per frame
    //void Update()
    //{
    //
    //}

    private void CleanUp()
    {
        foreach(GameObject obj in temp)
        {
            GameObject.Destroy(obj);
        }
        temp = null;
    }

    // Source: https://discussions.unity.com/t/how-to-gradually-scale-an-object-between-different-sizes/883714/3 by: sonicbelmont
    protected IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
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

    protected IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        var startPosition = obj.transform.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    public virtual void SuccessfulCast()
    {
        //Get animator component
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("successfulCast");
        }
    }
}
