using System;
using System.Collections.Generic;
using UnityEngine;

public class ShapeClickManager : MonoBehaviour
{
    [Header("References")]
    public HOGameBeh hoGameBeh;

    [Header("Click Settings")]
    public Color clickHighlightColor = Color.yellow;
    public bool enableClickFeedback = true;
    public bool enableDebugMode = true;

    public static event Action<ShapeClickData> OnShapeClicked;

    [System.Serializable]
    public class ShapeClickData
    {
        public HOGameBeh.SHAPES shapeType;
        public Vector3 worldPosition;
        public Vector3 gridPosition;
        public Vector2 size;
        public Vector3 offset;
        public float angle;
        public bool isToBeFilled;
        public bool isExcess;
        public GameObject shapeGameObject;
        public HOGameBeh.ShapeObject originalShapeObject;

        public ShapeClickData(HOGameBeh.ShapeObject shapeObj)
        {
            if (shapeObj?.actualShapeObj != null)
            {
                originalShapeObject = shapeObj;
                shapeType = shapeObj.shape;
                worldPosition = shapeObj.actualShapeObj.transform.position;
                gridPosition = new Vector3(shapeObj.x, shapeObj.y, 0);
                size = new Vector2(shapeObj.x, shapeObj.y);
                offset = shapeObj.offset;
                angle = shapeObj.angle;
                isToBeFilled = shapeObj.isToBeFilled;
                isExcess = shapeObj.isExcess;
                shapeGameObject = shapeObj.actualShapeObj;
            }
        }
    }

    private Dictionary<GameObject, HOGameBeh.ShapeObject> shapeRegistry = new Dictionary<GameObject, HOGameBeh.ShapeObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    void Start()
    {
        if (enableDebugMode) Debug.Log("ShapeClickManager: Starting...");

        if (hoGameBeh == null)
        {
            hoGameBeh = FindObjectOfType<HOGameBeh>();
            if (enableDebugMode) Debug.Log($"ShapeClickManager: Found HOGameBeh: {hoGameBeh != null}");
        }

        if (hoGameBeh == null)
        {
            Debug.LogError("ShapeClickManager: HOGameBeh not found! Some functionality might be impaired.");
            // Note: The manager can still function for click detection,
            // but MakeShapesClickable might need to be called manually if hoGameBeh remains null.
        }

        // Subscribe to our own events for built-in feedback
        OnShapeClicked -= HandleShapeClickFeedback; // Defensive unsubscribe
        OnShapeClicked += HandleShapeClickFeedback;

        StartCoroutine(AutoSetupShapes());
    }

    private System.Collections.IEnumerator AutoSetupShapes()
    {
        yield return new UnityEngine.WaitForSeconds(0.5f); // Ensure HOGameBeh has had time to create shapes
        MakeShapesClickable();
    }

    void OnDestroy()
    {
        if (enableDebugMode) Debug.Log("ShapeClickManager: OnDestroy called.");
        OnShapeClicked -= HandleShapeClickFeedback;
        // Clear static event subscribers if this manager is destroyed to prevent memory leaks
        // if other objects don't unsubscribe correctly.
        // However, be careful if you have multiple ShapeClickManagers (which you shouldn't for a static event like this)
        if (OnShapeClicked != null)
        {
            foreach (Delegate d in OnShapeClicked.GetInvocationList())
            {
                OnShapeClicked -= (Action<ShapeClickData>)d;
            }
            if (enableDebugMode) Debug.Log("ShapeClickManager: Cleared all subscribers from OnShapeClicked upon destruction.");
        }
    }

    public void MakeShapesClickable()
    {
        if (enableDebugMode) Debug.Log("ShapeClickManager: MakeShapesClickable called");

        if (hoGameBeh?.spellCastEvent?.shapes == null)
        {
            if (enableDebugMode)
            {
                if (hoGameBeh == null) Debug.Log("ShapeClickManager: HOGameBeh reference is null.");
                else if (hoGameBeh.spellCastEvent == null) Debug.Log("ShapeClickManager: HOGameBeh.spellCastEvent is null.");
                else if (hoGameBeh.spellCastEvent.shapes == null) Debug.Log("ShapeClickManager: HOGameBeh.spellCastEvent.shapes is null.");
                Debug.Log("ShapeClickManager: No shapes found to make clickable at this moment.");
            }
            return;
        }

        int shapeCount = 0;
        foreach (var shapeObj in hoGameBeh.spellCastEvent.shapes)
        {
            if (shapeObj?.actualShapeObj != null)
            {
                MakeShapeClickable(shapeObj);
                shapeCount++;
            }
        }

        if (enableDebugMode) Debug.Log($"ShapeClickManager: Attempted to make {shapeCount} shapes clickable.");
    }

    public void MakeShapeClickable(HOGameBeh.ShapeObject shapeObj)
    {
        if (shapeObj?.actualShapeObj == null)
        {
            if (enableDebugMode) Debug.Log("ShapeClickManager: Cannot make null shape or shape with null actualShapeObj clickable");
            return;
        }

        GameObject shapeGameObj = shapeObj.actualShapeObj;

        if (enableDebugMode) Debug.Log($"ShapeClickManager: Making shape clickable: {shapeGameObj.name} ({shapeObj.shape})");

        if (shapeGameObj.GetComponent<Collider>() == null && shapeGameObj.GetComponent<Collider2D>() == null)
        {
            AddColliderToShape(shapeGameObj);
        }
        else
        {
            if (enableDebugMode) Debug.Log($"ShapeClickManager: Collider already exists on {shapeGameObj.name}");
        }

        ShapeClickHandler clickHandler = shapeGameObj.GetComponent<ShapeClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = shapeGameObj.AddComponent<ShapeClickHandler>();
            if (enableDebugMode) Debug.Log($"ShapeClickManager: Added ShapeClickHandler to {shapeGameObj.name}");
        }
        clickHandler.Initialize(this, shapeObj);

        shapeRegistry[shapeGameObj] = shapeObj;

        MeshRenderer renderer = shapeGameObj.GetComponent<MeshRenderer>();
        if (renderer != null && !originalColors.ContainsKey(shapeGameObj))
        {
            originalColors[shapeGameObj] = renderer.material.color;
        }

        if (enableDebugMode)
        {
            Collider col = shapeGameObj.GetComponent<Collider>();
            Collider2D col2D = shapeGameObj.GetComponent<Collider2D>();
            Debug.Log($"ShapeClickManager: Shape {shapeGameObj.name} setup complete. Has Collider: {col != null}, Has Collider2D: {col2D != null}, Has Handler: {clickHandler != null}");
            if (col != null) Debug.Log($"ShapeClickManager: Collider bounds: {col.bounds}");
            if (col2D != null) Debug.Log($"ShapeClickManager: Collider2D bounds: {col2D.bounds}");
        }
    }

    public void NotifyShapeClicked(HOGameBeh.ShapeObject shapeObj)
    {
        if (enableDebugMode) Debug.Log($"ShapeClickManager: NotifyShapeClicked called for shape: {shapeObj?.shape}");

        if (shapeObj != null)
        {
            ShapeClickData clickData = new ShapeClickData(shapeObj);

            if (OnShapeClicked == null)
            {
                Debug.LogWarning("ShapeClickManager: OnShapeClicked event is NULL before invoking! No subscribers.");
            }
            else
            {
                Debug.Log($"ShapeClickManager: Invoking OnShapeClicked. Number of subscribers: {OnShapeClicked.GetInvocationList().Length}");
                foreach (Delegate handler in OnShapeClicked.GetInvocationList())
                {
                    // Construct a more informative string for the target
                    string targetInfo = "Static or Unknown";
                    if (handler.Target != null)
                    {
                        targetInfo = handler.Target.GetType().FullName;
                        // If it's a MonoBehaviour, include GameObject name for easier debugging
                        if (handler.Target is MonoBehaviour monoBehaviourTarget)
                        {
                            targetInfo += $" (GameObject: {monoBehaviourTarget.gameObject.name})";
                        }
                    }
                    Debug.Log($"ShapeClickManager: Subscriber -> Method: {handler.Method.Name}, Target: {targetInfo}");
                }
            }
            OnShapeClicked?.Invoke(clickData);
        }
    }

    private void HandleShapeClickFeedback(ShapeClickData clickData)
    {
        Debug.Log($"ShapeClickManager.HandleShapeClickFeedback: SHAPE CLICKED! Type: {clickData.shapeType}, Position: {clickData.worldPosition}, Size: {clickData.size}");

        if (!enableClickFeedback || clickData.shapeGameObject == null) return;

        MeshRenderer renderer = clickData.shapeGameObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            StartCoroutine(FlashColor(renderer, clickHighlightColor, 0.2f));
        }
    }

    private System.Collections.IEnumerator FlashColor(MeshRenderer renderer, Color flashColor, float duration)
    {
        if (!originalColors.TryGetValue(renderer.gameObject, out Color originalColor))
        {
            originalColor = renderer.material.color; // Fallback if not stored
        }
        renderer.material.color = flashColor;
        yield return new UnityEngine.WaitForSeconds(duration);
        if (renderer != null) // Check if object still exists
        {
            renderer.material.color = originalColor;
        }
    }

    private void AddColliderToShape(GameObject shapeGameObj)
    {
        if (enableDebugMode) Debug.Log($"ShapeClickManager: Adding collider to {shapeGameObj.name}");

        // Prefer MeshCollider if a MeshFilter exists
        MeshFilter meshFilter = shapeGameObj.GetComponent<MeshFilter>();
        if (meshFilter?.sharedMesh != null)
        {
            if (enableDebugMode) Debug.Log($"ShapeClickManager: Adding MeshCollider to {shapeGameObj.name} as it has a MeshFilter.");
            MeshCollider meshCollider = shapeGameObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            // Convex can be true if your shapes are simple and you don't need concave detection.
            // For complex 2D shapes made of meshes, convex might need to be true or you might need to decompose them.
            // For simple generated meshes (like Unity primitives or your custom ones), non-convex should work.
            meshCollider.convex = true; // Often better for dynamic objects or simpler meshes
            meshCollider.enabled = true;

            if (enableDebugMode)
            {
                Debug.Log($"ShapeClickManager: Added MeshCollider to {shapeGameObj.name}. Bounds: {meshCollider.bounds}");
            }
        }
        else
        {
            // Fallback to BoxCollider if no mesh or if you prefer simpler colliders
            if (enableDebugMode) Debug.Log($"ShapeClickManager: Adding BoxCollider to {shapeGameObj.name} (no MeshFilter or preferred).");
            BoxCollider boxCollider = shapeGameObj.AddComponent<BoxCollider>();
            // Ensure some thickness for raycasting, especially if camera is perfectly aligned.
            // Size will be based on renderer bounds if available, else default.
            Renderer rend = shapeGameObj.GetComponent<Renderer>();
            if (rend != null)
            {
                boxCollider.size = new Vector3(rend.bounds.size.x, rend.bounds.size.y, Mathf.Max(0.1f, rend.bounds.size.z));
            }
            else
            {
                boxCollider.size = new Vector3(1f, 1f, 0.1f); // Default size
            }
            boxCollider.enabled = true;
            if (enableDebugMode)
            {
                Debug.Log($"ShapeClickManager: Added BoxCollider to {shapeGameObj.name}. Size: {boxCollider.size}, Bounds: {boxCollider.bounds}");
            }
        }
    }

    void Update()
    {
        if (enableDebugMode && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            // Debug.Log($"ShapeClickManager: Mouse clicked at screen position: {mousePos}"); // Can be noisy

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("ShapeClickManager: No main camera found for raycast debug!");
                return;
            }

            Ray ray = cam.ScreenPointToRay(mousePos);
            // Debug.Log($"ShapeClickManager: Ray origin: {ray.origin}, direction: {ray.direction}"); // Can be noisy

            RaycastHit hit;
            // LayerMask could be useful here if you have many colliders
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log($"ShapeClickManager (DEBUG RAYCAST): Raycast hit: {hit.collider.gameObject.name} at distance {hit.distance} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            }
            else
            {
                // Debug.Log("ShapeClickManager (DEBUG RAYCAST): Raycast hit nothing."); // Can be noisy
            }
        }
    }

    public class ShapeClickHandler : MonoBehaviour
    {
        private ShapeClickManager manager;
        private HOGameBeh.ShapeObject linkedShapeObject;

        public void Initialize(ShapeClickManager clickManager, HOGameBeh.ShapeObject shapeObj)
        {
            manager = clickManager;
            linkedShapeObject = shapeObj;

            if (manager != null && manager.enableDebugMode)
            {
                //Debug.Log($"ShapeClickHandler: Initialized on {gameObject.name}");
            }
        }

        private void OnMouseDown()
        {
            if (manager != null && manager.enableDebugMode)
            {
                Debug.Log($"ShapeClickHandler: OnMouseDown called on {gameObject.name} (Shape: {linkedShapeObject?.shape})");
            }

            if (manager != null && linkedShapeObject != null)
            {
                manager.NotifyShapeClicked(linkedShapeObject);
            }
            else
            {
                if (manager != null && manager.enableDebugMode)
                {
                    Debug.LogWarning($"ShapeClickHandler: OnMouseDown on {gameObject.name} - manager or linkedShapeObject is null. Manager: {manager != null}, LinkedShape: {linkedShapeObject != null}");
                }
            }
        }

        private void OnMouseEnter()
        {
            // Optional: Add hover feedback if desired
            // if (manager != null && manager.enableDebugMode)
            // {
            //     Debug.Log($"ShapeClickHandler: Mouse entered {gameObject.name}");
            // }
        }

        private void OnMouseExit()
        {
            // Optional: Remove hover feedback if desired
            // if (manager != null && manager.enableDebugMode)
            // {
            //     Debug.Log($"ShapeClickHandler: Mouse exited {gameObject.name}");
            // }
        }
    }
}