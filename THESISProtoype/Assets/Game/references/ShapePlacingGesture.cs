using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ShapePlacementManager : MonoBehaviour
{
    [Header("References")]
    public ShapeGenerator shapeGenerator;
    public GridSystem gridSystem;

    [Header("Preview Settings")]
    public UnityEngine.Color previewColor = new UnityEngine.Color(0.5f, 0.5f, 1f, 0.5f);
    public float previewZOffset = -0.1f;
    public float touchDelayTime = 0.3f; // Added delay time setting

   // public UnityEngine.Color settedColor = new UnityEngine.Color(0.1f, 0.32f, 1f, 1f);

    // Shape types
    public enum ShapeType
    {
        None,
        Square,
        Rectangle,
        Triangle,
        Circle,
        HalfCircle
    }

    // Current selection
    public ShapeType currentShapeType = ShapeType.None;
    private float currentWidth = 1f;
    private float currentHeight = 1f;

    // Current shape size for this drag operation
    private Vector3 currentDragShapeSize;
    private bool hasShapeSizeForDrag = false;

    // Touch tracking
    private Vector3 touchStartPos;
    private Vector3 currentTouchPos;
    private bool isDragging = false;
    private GameObject previewShape;

    // Touch delay variables
    private bool isWaitingForDelay = false;
    private float touchStartTime = 0f;
    private Vector3 pendingTouchPosition;

    // Replace GameObject list with ShapeObject list
    public List<HOGameBeh.ShapeObject> placedShapes = new List<HOGameBeh.ShapeObject>();

    // Keep track of shapes we've placed and their sizes for undo
    //private Stack<KeyValuePair<HOGameBeh.SHAPES, Vector3>> placedShapeHistory = new Stack<KeyValuePair<HOGameBeh.SHAPES, Vector3>>();

    // Keep a reference to HOGameBeh
    private HOGameBeh gameController;

    // Reference to main camera
    private Camera mainCamera;

    // System.Random with better seed
    private System.Random random;

    private bool isSizeRandomlyGenerated = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
            return;
        }

        if (shapeGenerator == null)
        {
            shapeGenerator = FindObjectOfType<ShapeGenerator>();
        }

        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
        }

        // Get reference to HOGameBeh
        gameController = FindObjectOfType<HOGameBeh>();

        if (shapeGenerator == null || gridSystem == null || gameController == null)
        {
            Debug.LogError("Required references missing in ShapePlacementManager!");
            enabled = false;
        }

        if (currentWidth == 0)
        {
            currentWidth = 1f;
            currentHeight = 1f;
        }

        // Initialize random with a better seed
        random = new System.Random(System.DateTime.Now.Millisecond + System.Environment.TickCount);
    }

    void Update()
    {
        // Process touch delay if needed
        if (isWaitingForDelay)
        {
            if (Time.time - touchStartTime >= touchDelayTime)
            {
                isWaitingForDelay = false;
                BeginDrag(pendingTouchPosition);
            }
        }

        HandleTouchInput();
    }

    void HandleTouchInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            // Start the delay timer
            StartTouchDelay(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                EndDrag();
            }
            else
            {
                // Cancel the delay if touch is released before delay completes
                isWaitingForDelay = false;
            }
        }
#else
       if (Input.touchCount > 0)
       {
           Touch touch = Input.GetTouch(0);
           
           if (touch.phase == TouchPhase.Began)
           {
               // Start the delay timer
               StartTouchDelay(touch.position);
           }
           else if (touch.phase == TouchPhase.Moved && isDragging)
           {
               ContinueDrag(touch.position);
           }
           else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
           {
               if (isDragging)
               {
                   EndDrag();
               }
               else
               {
                   // Cancel the delay if touch is released before delay completes
                   isWaitingForDelay = false;
               }
           }
       }
#endif
    }

    void StartTouchDelay(Vector3 screenPosition)
    {
        touchStartTime = Time.time;
        pendingTouchPosition = screenPosition;
        isWaitingForDelay = true;
    }

    void BeginDrag(Vector3 screenPosition)
    {
        touchStartPos = GetWorldPositionFromScreen(screenPosition);
        currentTouchPos = touchStartPos;
        isDragging = true;

        // Convert ShapeType to HOGameBeh.SHAPES enum for size lookup
        HOGameBeh.SHAPES shapeType = ConvertToHOGameShape(currentShapeType);

        // Get available size from HOGameBeh, but don't remove it yet
        currentDragShapeSize = GetNextShapeSizeFromHOGameBeh(shapeType, false);


        if (currentDragShapeSize != Vector3.zero)
        {
            // Update dimensions with the obtained size
            currentWidth = currentDragShapeSize.x;
            currentHeight = currentDragShapeSize.y;
            hasShapeSizeForDrag = true;
        }
        else
        {
            hasShapeSizeForDrag = false;
        }

        CreatePreviewShape();
    }

    void ContinueDrag(Vector3 screenPosition)
    {
        currentTouchPos = GetWorldPositionFromScreen(screenPosition);

        if (Vector3.Distance(currentTouchPos, touchStartPos) > 0.05f)
        {
            UpdatePreviewShapePosition();
        }
    }

    void EndDrag()
    {
        if (previewShape != null)
        {
            Destroy(previewShape);
            previewShape = null;
        }

        // Calculate grid position
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector2 offset = new Vector2(
            Mathf.RoundToInt((currentTouchPos.x - cameraPosition.x) / gridSystem.minorGridSize),
            Mathf.RoundToInt((currentTouchPos.y - cameraPosition.y) / gridSystem.minorGridSize)
        );

        // Convert ShapeType to HOGameBeh.SHAPES enum
        HOGameBeh.SHAPES shapeType = ConvertToHOGameShape(currentShapeType);

        Vector3 usedShapeSize;

        // If we have a shape size for this drag, use it
        // Otherwise get a new one and remove it from the list
        if (!hasShapeSizeForDrag)
        {
            Vector3 shapeSize = GetNextShapeSizeFromHOGameBeh(shapeType, true);
            if (shapeSize != Vector3.zero)
            {
                currentWidth = shapeSize.x;
                currentHeight = shapeSize.y;
                usedShapeSize = shapeSize;
            }
            else
            {
                usedShapeSize = new Vector3(currentWidth, currentHeight, 0);
            }
        }
        else
        {
            // We already have the size, but we need to remove it from the list
            RemoveShapeSizeFromHOGameBeh(shapeType, currentDragShapeSize);
            usedShapeSize = currentDragShapeSize;
        }

        // Create the shape and get the GameObject with updated dimensions
        GameObject finalShapeObject = CreateShapeWithOffset(offset, false);

        if (finalShapeObject != null)
        {
            // Create a new ShapeObject
            HOGameBeh.ShapeObject shapeObj = new HOGameBeh.ShapeObject(
                (int)currentWidth,
                (int)currentHeight,
                shapeType
            );
            shapeObj.isExcess = this.isSizeRandomlyGenerated;

            // Set the actual GameObject reference
            shapeObj.actualShapeObj = finalShapeObject;
            shapeObj.offset = new Vector3(offset.x, offset.y, 0);

            // Add to the list
            placedShapes.Add(shapeObj);

            // Add to our history for undo
            //placedShapeHistory.Push(new KeyValuePair<HOGameBeh.SHAPES, Vector3>(shapeType, usedShapeSize));

            // Reduce the remaining count for this shape type in HOGameBeh
            if (gameController != null && gameController.spellCastEvent != null)
            {
                gameController.spellCastEvent.addPlacementShapeRemainingUse(shapeType, -1);
            }

            Debug.Log($"Placed {shapeType} at {finalShapeObject.transform.position} with dimensions: {currentWidth}x{currentHeight}");
        }

        isDragging = false;
        isWaitingForDelay = false;
        hasShapeSizeForDrag = false;
    }

    private Vector3 GetNextShapeSizeFromHOGameBeh(HOGameBeh.SHAPES shapeType, bool removeFromList)
    {
        if (gameController == null || gameController.spellCastEvent == null)
            return Vector3.zero;

        // Get available sizes for this shape type
        List<Vector3> availableSizes = gameController.spellCastEvent.getPlacementShapeSizes(shapeType);

        // If there are no predefined sizes, generate a random one based on the problem
        if (availableSizes == null || availableSizes.Count == 0)
        {
            isSizeRandomlyGenerated = true;
            // Generate random sizes based on the problem's existing shapes
            return GenerateRandomSizeBasedOnProblem(shapeType);
        }

        else
        {
            isSizeRandomlyGenerated = false;
        }

        // Otherwise, take the first available size and remove it from the list if needed
        if (availableSizes.Count > 0)
        {
            // Get a random size from the available ones instead of always the first
            int randomIndex = random.Next(availableSizes.Count);
            Vector3 size = availableSizes[randomIndex];

            if (removeFromList)
            {
                availableSizes.RemoveAt(randomIndex);
            }
            return size;
        }

        return Vector3.zero;
    }

    private void RemoveShapeSizeFromHOGameBeh(HOGameBeh.SHAPES shapeType, Vector3 sizeToRemove)
    {
        if (gameController == null || gameController.spellCastEvent == null)
            return;

        List<Vector3> availableSizes = gameController.spellCastEvent.getPlacementShapeSizes(shapeType);
        if (availableSizes != null && availableSizes.Count > 0)
        {
            // Look for the size to remove
            for (int i = 0; i < availableSizes.Count; i++)
            {
                Vector3 size = availableSizes[i];
                if (Mathf.Approximately(size.x, sizeToRemove.x) &&
                    Mathf.Approximately(size.y, sizeToRemove.y) &&
                    Mathf.Approximately(size.z, sizeToRemove.z))
                {
                    availableSizes.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void RestoreShapeSizeToHOGameBeh(HOGameBeh.SHAPES shapeType, Vector3 sizeToRestore)
    {
        if (gameController == null || gameController.spellCastEvent == null)
            return;

        gameController.spellCastEvent.addPlacementShapeSizes(shapeType, sizeToRestore);
    }

    int maxX = 1;
    int maxY = 1;

    private Vector3 GenerateRandomSizeBasedOnProblem(HOGameBeh.SHAPES shapeType)
    {
        if (gameController == null || gameController.spellCastEvent == null)
            return new Vector3(1, 1, 0);

        // Find the max dimensions from the problem's shapes
  

        foreach (HOGameBeh.ShapeObject shape in gameController.spellCastEvent.shapes)
        {
            if (shape.x > maxX) 
                maxX = shape.x;
            if (shape.y > maxY && shape.y != HOGameBeh.UNUSED) 
                maxY = shape.y;
        }

        // Generate random size between 1,1 and maxX,maxY
      
        int randomX = random.Next(1, maxX + 1);
        int randomY = randomX;

        // For rectangle, generate different Y dimension
        if (shapeType == HOGameBeh.SHAPES.RECTANGLE)
        {
            while(randomX == randomY)
                randomY = random.Next(1, maxY + 1);
        }

        // Circle and semi-circle only need X dimension
        if (shapeType == HOGameBeh.SHAPES.CIRCLE || shapeType == HOGameBeh.SHAPES.SEMI_CIRCLE)
        {
            return new Vector3(randomX, 0, 0);
        }

        return new Vector3(randomX, randomY, 0);
    }

    private Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        float distance = -ray.origin.z / ray.direction.z;
        Vector3 worldPos = ray.origin + ray.direction * distance;
        worldPos.z = 0;
        return worldPos;
    }

    private void UpdatePreviewShapePosition()
    {
        if (previewShape != null)
        {
            Destroy(previewShape);
        }

        // Calculate offset from camera position
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector2 offset = new Vector2(
            Mathf.RoundToInt((currentTouchPos.x - cameraPosition.x) / gridSystem.minorGridSize),
            Mathf.RoundToInt((currentTouchPos.y - cameraPosition.y) / gridSystem.minorGridSize)
        );

        // Create preview using offset approach
        previewShape = CreateShapeWithOffset(offset, true);
    }

    private void CreatePreviewShape()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector2 offset = new Vector2(
            Mathf.RoundToInt((touchStartPos.x - cameraPosition.x) / gridSystem.minorGridSize),
            Mathf.RoundToInt((touchStartPos.y - cameraPosition.y) / gridSystem.minorGridSize)
        );

        previewShape = CreateShapeWithOffset(offset, true);
    }

    private GameObject CreateShapeWithOffset(Vector2 offset, bool isPreview)
    {
        GameObject shape = null;

        try
        {
            // Use the ShapeGenerator's methods with the calculated offset
            switch (currentShapeType)
            {
                case ShapeType.Square:
                    shape = shapeGenerator.CreateSquare(offset, currentWidth);
                    break;

                case ShapeType.Rectangle:
                    shape = shapeGenerator.CreateRectangle(offset, currentWidth, currentHeight);
                    break;

                case ShapeType.Triangle:
                    shape = shapeGenerator.CreateTriangle(offset, currentWidth, currentHeight);
                    break;

                case ShapeType.Circle:
                    shape = shapeGenerator.CreateCircle(offset, currentWidth, false);
                    break;

                case ShapeType.HalfCircle:
                    shape = shapeGenerator.CreateCircle(offset, currentWidth, true);
                    break;
            }

            if (shape != null && isPreview)
            {
                shape.name = "Preview Shape";

                // Adjust Z position to avoid z-fighting
                Vector3 pos = shape.transform.position;
                pos.z = previewZOffset;
                shape.transform.position = pos;

                // Make semi-transparent and different color
                Renderer renderer = shape.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Material previewMaterial = new Material(renderer.material);
                    previewMaterial.color = previewColor;
                    renderer.material = previewMaterial;
                }
            }
            else if (shape != null)
            {
                shape.name = $"{currentShapeType} Shape";

                Renderer renderer = shape.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    // Create a new material instance if needed
                    Material mat = renderer.material;
                    mat.color = new UnityEngine.Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f); ;
                    // Set the rendering mode to opaque or cutout instead of transparent
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1); // Enable depth writing
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = isPreview ? 3000 : 2000; // Lower number renders first
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating shape: {e.Message}");
        }

        return shape;
    }

    // Helper method to convert ShapeType to HOGameBeh.SHAPES
    private HOGameBeh.SHAPES ConvertToHOGameShape(ShapeType type)
    {
        switch (type)
        {
            case ShapeType.Square:
                return HOGameBeh.SHAPES.SQUARE;
            case ShapeType.Rectangle:
                return HOGameBeh.SHAPES.RECTANGLE;
            case ShapeType.Triangle:
                return HOGameBeh.SHAPES.TRIANGLE;
            case ShapeType.Circle:
                return HOGameBeh.SHAPES.CIRCLE;
            case ShapeType.HalfCircle:
                return HOGameBeh.SHAPES.SEMI_CIRCLE;
            default:
                return HOGameBeh.SHAPES.NONE;
        }
    }

    // Convert HOGameBeh.SHAPES to ShapeType
    private ShapeType ConvertToShapeType(HOGameBeh.SHAPES shape)
    {
        switch (shape)
        {
            case HOGameBeh.SHAPES.SQUARE:
                return ShapeType.Square;
            case HOGameBeh.SHAPES.RECTANGLE:
                return ShapeType.Rectangle;
            case HOGameBeh.SHAPES.TRIANGLE:
                return ShapeType.Triangle;
            case HOGameBeh.SHAPES.CIRCLE:
                return ShapeType.Circle;
            case HOGameBeh.SHAPES.SEMI_CIRCLE:
                return ShapeType.HalfCircle;
            default:
                return ShapeType.None;
        }
    }

    // Public methods to be called from other scripts
    public void SetCurrentShape(ShapeType shapeType)
    {
        currentShapeType = shapeType;
    }

    public void SetShapeDimensions(float width, float height)
    {
        currentWidth = width;
        currentHeight = height;
    }

    public void ClearAllShapes()
    {
        foreach (HOGameBeh.ShapeObject shape in placedShapes)
        {
            if (shape.actualShapeObj != null)
            {
                Destroy(shape.actualShapeObj);
            }
        }

        placedShapes.Clear();
     
    }

    public ShapeType Undo()
    {
        // Check if there are any shapes to undo
        if (placedShapes.Count > 0)
        {
            // Get the last placed shape
            HOGameBeh.ShapeObject lastShape = placedShapes[placedShapes.Count - 1];

            // Store the shape type before removing
            ShapeType removedShapeType = ConvertToShapeType(lastShape.shape);
            HOGameBeh.SHAPES hoShapeType = lastShape.shape;

            // Remove it from the list
            placedShapes.RemoveAt(placedShapes.Count - 1);

            // Destroy the GameObject
            if (lastShape.actualShapeObj != null)
            {
                Destroy(lastShape.actualShapeObj);
                print("Is Really Excess? " + lastShape.isExcess);

                // Don't add specific size back to available sizes list
                // Just increment the remaining count so it can generate a new intended size
                if (gameController != null && gameController.spellCastEvent != null && !lastShape.isExcess)
                {
                    gameController.spellCastEvent.addPlacementShapeSizes(hoShapeType, new Vector3(lastShape.x, lastShape.y, 0));    
                }

                Debug.Log($"Undid last shape placement: {lastShape.shape}");
            }


            // Return the removed shape type
            return removedShapeType;
        }
        else
        {
            Debug.Log("Nothing to undo");
            return ShapeType.None;
        }
    }
    // This method updates the angle of the last placed shape
    public void RotateLastShape(float angle)
    {
        if (placedShapes.Count > 0)
        {
            HOGameBeh.ShapeObject lastShape = placedShapes[placedShapes.Count - 1];
            if (lastShape.actualShapeObj != null)
            {
                lastShape.angle = angle;
                lastShape.actualShapeObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    // Get the current placed shapes count
    public int GetPlacedShapesCount()
    {
        return placedShapes.Count;
    }

    // Get a specific placed shape by index
    public HOGameBeh.ShapeObject GetPlacedShape(int index)
    {
        if (index >= 0 && index < placedShapes.Count)
        {
            return placedShapes[index];
        }
        return null;
    }
}