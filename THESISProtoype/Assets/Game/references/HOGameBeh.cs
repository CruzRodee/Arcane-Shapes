using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOGameBeh : MonoBehaviour
{
    public const int UNUSED = -1;

    public SpellCastEvent spellCastEvent;
    public ShapeGenerator shapeGenerator;
    public ShapeClickManager shapeClickManager;
    public HOGameScript script;

   

    void Start()
    {
        
        
    }

    public void Initiate()
    {
        StartCoroutine(WaitForComponent());
    }

    IEnumerator WaitForComponent()
    {
        Debug.Log("HOGameBeh: WaitForComponent coroutine started.");
        // Wait until ShapeGenerator is found
        while (this.shapeGenerator == null)
        {
            GameObject sgObj = GameObject.Find("ShapeGenerator");
            if (sgObj != null)
            {
                this.shapeGenerator = sgObj.GetComponent<ShapeGenerator>();
                if (this.shapeGenerator != null)
                {
                    Debug.Log("HOGameBeh: ShapeGenerator component found!");
                }
            }

            if (this.shapeGenerator == null) // If still null after attempting to find
            {
                Debug.Log("HOGameBeh: ShapeGenerator not found yet, waiting a frame...");
                yield return new WaitForEndOfFrame(); // Wait a frame before trying again
            }
        }

        // ShapeGenerator is now guaranteed to be assigned.
        // Now find ShapeClickManager and subscribe to the event.
        GameObject scmObj = GameObject.Find("ShapeClickManager");
        if (scmObj != null)
        {
            this.shapeClickManager = scmObj.GetComponent<ShapeClickManager>();
            if (this.shapeClickManager != null)
            {
                Debug.Log("HOGameBeh: ShapeClickManager component found. Subscribing OnAnyShapeClicked to ShapeClickManager.OnShapeClicked.");
                // Defensive unsubscription first, then subscription, to prevent multiple subscriptions.
                ShapeClickManager.OnShapeClicked -= OnAnyShapeClicked;
                ShapeClickManager.OnShapeClicked += OnAnyShapeClicked;
            }
            else
            {
                Debug.LogError("HOGameBeh: Found ShapeClickManager GameObject, but the ShapeClickManager component is missing!");
            }
        }
        else
        {
            Debug.LogError("HOGameBeh: ShapeClickManager GameObject not found! Cannot subscribe to OnShapeClicked.");
        }

        // Set the problem based on HO level
        SetManualProblem(GlobalVariables.HOProblem());

        //Instance the HO Game Spell Object
        script.InstanceSpellObject();
    }

    /**Call to set the shapes to be spawned*/
    public void SetManualProblem(List<ShapeObject> list)
    {
        this.spellCastEvent = new SpellCastEvent(this, list);
        // Add this single line to make all shapes clickable
        if (this.shapeClickManager != null)
        {
            this.shapeClickManager.MakeShapesClickable();
        }
        else
        {
            Debug.LogWarning("HOGameBeh: SetManualProblem called, but shapeClickManager is null. Cannot make shapes clickable yet.");
        }
    }

    public bool isAllAttemptedSolve()
    {
        foreach (ShapeObject shape in this.spellCastEvent.shapes)
        {
            if (!shape.actualShapeObj.transform.Find("FillShape"))
                return false;
        }
        return true;
    }


    public class ShapeObject
    {
        public float x = UNUSED;
        public float y = UNUSED;
        public GameBehaviour.SHAPES shape;
        public GameObject actualShapeObj;
        public Vector3 offset = Vector3.zero;
        public bool isToBeFilled = false;
        public float angle = 0;
        public bool isExcess = false;
        public bool isIntersect = false;
        public float zOffset = 0;

        public ShapeObject(float x, float y, GameBehaviour.SHAPES shape)
        {
            this.x = x;
            this.y = y;
            this.shape = shape;
        }

        public ShapeObject withOffset(Vector3 offset)
        {
            this.offset = offset;
            return this;
        }

        public ShapeObject setIsToBeFilled()
        {
            this.isToBeFilled = true;
            return this;
        }

        public ShapeObject tilt(float angle)
        {
            this.angle = angle;
            return this;
        }
    }

    public class SpellCastEvent
    {
        public HOGameBeh main;
        public List<ShapeObject> shapes;

        public SpellCastEvent(HOGameBeh behavior, List<ShapeObject> list)
        {
            this.main = behavior;
            this.shapes = list;
            this.initialize();
        }

        public void initialize()
        {
            if (this.shapes == null) return;

            foreach (ShapeObject shapeObject in this.shapes)
            {
                if (shapeObject != null)
                {
                    shapeObject.actualShapeObj = generate(shapeObject);
                    if (shapeObject.actualShapeObj != null)
                    {
                        shapeObject.actualShapeObj.transform.Rotate(0, 0, -shapeObject.angle);
                    }
                }
            }
        }

        public GameObject generate(ShapeObject obj)
        {
            if (obj == null || this.main == null || this.main.shapeGenerator == null)
            {
                UnityEngine.Debug.LogError("Cannot generate shape: null object, main behavior, or shapeGenerator.");
                return null;
            }

            GameObject ret = null; //Return GameObject
            switch (obj.shape)
            {
                case GameBehaviour.SHAPES.SQUARE:
                    ret = this.main.shapeGenerator.CreateSquare(obj.offset, obj.x);
                    break;
                case GameBehaviour.SHAPES.TRIANGLE:
                    ret = this.main.shapeGenerator.CreateTriangle(obj.offset, obj.x, obj.y);
                    break;
                case GameBehaviour.SHAPES.CIRCLE:
                    ret = this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, false);
                    break;
                case GameBehaviour.SHAPES.RECTANGLE:
                    ret = this.main.shapeGenerator.CreateRectangle(obj.offset, obj.x, obj.y);
                    break;
                case GameBehaviour.SHAPES.SEMI_CIRCLE:
                    ret = this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, true);
                    break;
                default:
                    UnityEngine.Debug.LogWarning($"Unsupported shape type for generation: {obj.shape}");
                    return null;
            }

            //Shape that is intersect is always excess
            if(obj.isIntersect)
                obj.isExcess = true;

            //Change color of excess
            if (obj.isExcess)
            {
                ret.GetComponent<Renderer>().material.color = Color.grey;
            }

            //Apply z-axis Offset to determine which shape is above which
            Vector3 curPos = ret.transform.localPosition;
            ret.transform.localPosition = new Vector3(curPos.x, curPos.y, obj.zOffset);

            return ret;
        }

        public void destroyAllShapes()
        {
            if (shapes == null) return;
            foreach (ShapeObject obj in shapes)
            {
                if (obj != null && obj.actualShapeObj != null)
                {
                    Destroy(obj.actualShapeObj);
                }
            }
            this.shapes.Clear();
        }

        public void setHiddenStateAllShapes(bool value)
        {
            if (shapes == null) return;

            foreach (ShapeObject obj in shapes)
            {
                
                if (obj != null && obj.actualShapeObj != null)
                {
                    //obj.actualShapeObj.SetActive(value);
                    if (obj.actualShapeObj.transform.Find("FillShape") != null)
                        SetHiddenStateAllShapes(obj.actualShapeObj.GetComponent<MeshRenderer>(), value,
                            obj.actualShapeObj.transform.Find("FillShape").gameObject);
                    else
                        SetHiddenStateAllShapes(obj.actualShapeObj.GetComponent<MeshRenderer>(), value);
                }
            }
        }

        private void SetHiddenStateAllShapes(MeshRenderer renderer, bool value, GameObject fillShape = null)
        {
            Vector3 hidePos = new(30,30,30);
            
            if (value)
            {
                float r = renderer.material.color.r, g = renderer.material.color.g, b = renderer.material.color.b;
                renderer.material.color = new Color(r,g,b,0);
                if (fillShape != null)
                    fillShape.transform.localPosition = hidePos; //Hide fill shape
            }

            else
            {
                float r = renderer.material.color.r, g = renderer.material.color.g, b = renderer.material.color.b;
                renderer.material.color = new Color(r, g, b, 1.0f);
                if (fillShape != null)
                    fillShape.transform.localPosition = Vector3.zero; //Show fill shape
            }
        }
    }

    private void OnAnyShapeClicked(ShapeClickManager.ShapeClickData clickData)
    {
        script.UIAfterShapeSelect(clickData);
        // Your custom logic here - you get ALL the shape information!
        // UnityEngine.Debug.Log($"HOGameBeh.OnAnyShapeClicked: Test that please... Clicked {clickData.shapeType} at {clickData.worldPosition}");

        //GlobalVariables.clickedShapeData = clickData;

        //UnityEngine.Debug.Log(clickData.originalShapeObject.x);
        //UnityEngine.Debug.Log(clickData.originalShapeObject.y);
        //UnityEngine.Debug.Log(clickData.originalShapeObject.shape);

        // Example actions:
        // - Play sound
        // - Update UI
        // - Change game state
        // - Move other shapes
        // etc.

        //Get the text displays for the selected shape
        script.GetVarDisp(clickData.originalShapeObject.shape);

        //Change Character dialogue on click
        script.characterSay.text = HOGameScript.charDialogue3;
    }

    void OnDestroy()
    {
        UnityEngine.Debug.Log("HOGameBeh: OnDestroy called. Unsubscribing OnAnyShapeClicked.");
        // Important: Check if ShapeClickManager might have been destroyed before this.
        // Although static events don't strictly need the instance, it's good practice.
        ShapeClickManager.OnShapeClicked -= OnAnyShapeClicked;
    }
}