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

        // Define the "house" shape configuration
        List<ShapeObject> house = new List<ShapeObject>();
        house.Add(new ShapeObject(2, UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled());
        house.Add(new ShapeObject(2, 2, GameBehaviour.SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(1, 0, 0)).tilt(90));
        house.Add(new ShapeObject(2, 2, GameBehaviour.SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(-1, 0, 0)).tilt(-90));
        house.Add(new ShapeObject(2, 2, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(0, 2, 0)));

        SetManualProblem(house);
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
        public int x = UNUSED;
        public int y = UNUSED;
        public GameBehaviour.SHAPES shape;
        public GameObject actualShapeObj;
        public Vector3 offset = Vector3.zero;
        public bool isToBeFilled = false;
        public float angle = 0;
        public bool isExcess = false;

        public ShapeObject(int x, int y, GameBehaviour.SHAPES shape)
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

            switch (obj.shape)
            {
                case GameBehaviour.SHAPES.SQUARE:
                    return this.main.shapeGenerator.CreateSquare(obj.offset, obj.x);
                case GameBehaviour.SHAPES.TRIANGLE:
                    return this.main.shapeGenerator.CreateTriangle(obj.offset, obj.x, obj.y);
                case GameBehaviour.SHAPES.CIRCLE:
                    return this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, false);
                case GameBehaviour.SHAPES.RECTANGLE:
                    return this.main.shapeGenerator.CreateRectangle(obj.offset, obj.x, obj.y);
                case GameBehaviour.SHAPES.SEMI_CIRCLE:
                    return this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, true);
                default:
                    UnityEngine.Debug.LogWarning($"Unsupported shape type for generation: {obj.shape}");
                    return null;
            }
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
                    SetHiddenStateAllShapes(obj.actualShapeObj.GetComponent<MeshRenderer>(), value);
                }
            }
        }

        private void SetHiddenStateAllShapes(MeshRenderer renderer, bool value)
        {
            if (value)
            {
                float r = renderer.material.color.r, g = renderer.material.color.g, b = renderer.material.color.b;
                renderer.material.color = new Color(r,g,b,0);
            }

            else
            {
                float r = renderer.material.color.r, g = renderer.material.color.g, b = renderer.material.color.b;
                renderer.material.color = new Color(r, g, b, 1.0f);
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
    }

    void OnDestroy()
    {
        UnityEngine.Debug.Log("HOGameBeh: OnDestroy called. Unsubscribing OnAnyShapeClicked.");
        // Important: Check if ShapeClickManager might have been destroyed before this.
        // Although static events don't strictly need the instance, it's good practice.
        ShapeClickManager.OnShapeClicked -= OnAnyShapeClicked;
    }
}