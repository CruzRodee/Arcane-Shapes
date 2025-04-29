using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelScript : MonoBehaviour
{
    public Texture elfFace, swirlyFace;
    
    private Animator animator;
    private Renderer faceMeshRenderer;
    public bool TEST = false;
    private int testCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Get Animator component
        animator = GetComponent<Animator>();

        //Get Face mesh renderer
        faceMeshRenderer = GameObject.FindWithTag("PlayerFaceMesh").GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Test Delete Later Maybe
        testAnims();
    }

    //TestScript
    private void testAnims()
    {
        if (TEST)
        {
            if (Input.GetKeyDown("space"))
            {
                if (testCount == 0) { BadTrace(); }
                else if (testCount < 5) { GoodTrace(testCount); }
                else if (testCount == 5) { GoodCast(0); }
                else if (testCount == 6) { GoodCast(1); }
                else if (testCount == 7) { GoodCast(2); }
                else if (testCount == 8) { GoodCast(3); }
                else if (testCount == 9) { GoodCast(4); }
                else if (testCount == 10) { OverCast(); }
                else if (testCount == 11) { UnderCast(); }
                if (testCount > 11) { testCount = -1; }
                testCount++;
            }
        }
    }

    //Animation Scripts
    public void ElfFace()
    {
        faceMeshRenderer.material.SetTexture("_BaseMap", elfFace);
    }
    public void SwirlyFace()
    {
        faceMeshRenderer.material.SetTexture("_BaseMap", swirlyFace);
    }
    //-------------------------

    //Shapes Indexes: Square(0), Rectangle(1), Triangle(2), Circle(3), SemiCircle(4)
    public void GoodCast(int index)
    {
        animator.SetInteger("shapeIndex", index);
        animator.SetTrigger("goodCast");
        //Add VFX in state transitions
    }

    public void OverCast()
    {
        animator.SetTrigger("overCast");
        //Add VFX in state transitions
    }

    public void UnderCast()
    {
        animator.SetTrigger("underCast");
        //Add VFX in state transitions
    }

    public void BadTrace()
    {
        animator.SetTrigger("badTrace");
        //Add VFX in state transitions
    }

    // Use numbers between 1-4 for variation parameter. Should probably randomize this for variety
    public void GoodTrace(int variation)
    {
        switch (variation)
        {
            case 1:
                {
                    animator.SetTrigger("goodTrace1");
                    break;
                }
            case 2:
                {
                    animator.SetTrigger("goodTrace2");
                    break;
                }
            case 3:
                {
                    animator.SetTrigger("goodTrace3");
                    break;
                }
            case 4:
                {
                    animator.SetTrigger("goodTrace4");
                    break;
                }
            default:
                {
                    Console.WriteLine("Error in PlayerModelScript/GoodTrace()");
                    break;
                }
        }
    }
}
