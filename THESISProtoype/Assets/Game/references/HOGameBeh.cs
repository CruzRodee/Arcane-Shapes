using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor;


public class HOGameBeh : MonoBehaviour
{
    public const int UNUSED = -1;


    SHAPES currentShape;
    public SpellCastEvent spellCastEvent;
    TMP_Dropdown dropdown;
    public TMP_Text text;
    TMP_Text manaMeasure;
    TMP_Text correctionPerc;
    FormulaDropdownManager dropdownManager;



    UnityEngine.UI.Button confirmMeasurement;
    UnityEngine.UI.Button restart;
    UnityEngine.UI.Button undo;
    UnityEngine.UI.Button rotate;
    int currentOptionSelected;

    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;
    public int seed;

    public HOLineSnapper lineSnapper;
    public ShapePlacementManager shapeManager;


    public const String SQUARE_FORMULA = "S*S";
    public const String CIRCLE_FORMULA = "ƒÎ*R*R";
    public const String RECTANGLE_FORMULA = "W*H";
    public const String TRIANGLE_FORMULA = "B*H*1/2";
    public const String SEMI_CIRCLE = CIRCLE_FORMULA + "*1/2";

    public const String ADD_SYMBOL = "+";
    public const String SUB_SYMBOL = "-";


    private string previousValue = "";

    [SerializeField] public TMP_Text inputField;
    [SerializeField] public UnityEngine.UI.Button gestureType;
    [SerializeField] public UnityEngine.UI.Button hideButton;
    [SerializeField] public GameObject panel;


    int current_mode = 0; //0 = shape, 1 = measure, 2 = none

    void Awake()
    {
        currentShape = SHAPES.NONE;
        System.Random rand = new System.Random();
        this.seed = rand.Next();
    }

    /*
     * 
     * 
     * PivotToCenter = Pivot + SizeX/2 + SizeY/2
     * 
     * 
     */

    // _init() _ready()

    void Reset()
    {
        //UnityEngine.Debug.Log("Is called?");
        dropdown.gameObject.SetActive(true);
        dropdown.value = 0;
     
      
        dropdown.RefreshShownValue();
        currentShape = SHAPES.NONE;

   
    
    
        confirmMeasurement.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        this.spellCastEvent.destroyAllShapes();
        lineSnapper.gameObject.SetActive(false);
        undo.gameObject.SetActive(false);
        rotate.gameObject.SetActive(false);

        while (lineSnapper.OnUndoPressed()) ;
        while (shapeManager.Undo() != ShapePlacementManager.ShapeType.None);
      

        this.resetProblem();

    }

    private bool isPanelActive = true;

    void Start()
    {
        //UnityEngine.Debug.Log(dropdown);
        dropdownManager = GameObject.Find("FormulaManager").GetComponent<FormulaDropdownManager>();
        restart = GameObject.Find("Restart").GetComponent<UnityEngine.UI.Button>();
        restart.onClick.AddListener(() =>
        {
            Reset();
        });
        dropdown = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();

        dropdown.onValueChanged.AddListener(OnOptionSelect);

        dropdown.value = 0;
        dropdown.RefreshShownValue();

        //inputField.onValueChanged.AddListener(ValidateInput);
      

        

        confirmMeasurement = GameObject.Find("ConfirmMeasurement").GetComponent<UnityEngine.UI.Button>();
        correctionPerc = GameObject.Find("ManaFillCorrectPerc").GetComponent<TMP_Text>();
        lineSnapper = GameObject.Find("Gesture").GetComponent<HOLineSnapper>();
        undo = GameObject.Find("Undo").GetComponent<UnityEngine.UI.Button>();
        rotate = GameObject.Find("Rotate").GetComponent<UnityEngine.UI.Button>();


        StartCoroutine(WaitForComponent());


        /* UnityEngine.Debug.Log(yesButton);
         UnityEngine.Debug.Log(noButton);*/

        shapeManager.SetCurrentShape(ShapePlacementManager.ShapeType.Triangle);
      
    
        confirmMeasurement.gameObject.SetActive(true);
        correctionPerc.gameObject.SetActive(true);
        lineSnapper.gameObject.SetActive(false);
        undo.gameObject.SetActive(false);
        rotate.gameObject.SetActive(false);


        hideButton.onClick.AddListener(() =>
        {
            TMP_Text textComponent = hideButton.GetComponentInChildren<TMP_Text>();
            isPanelActive = !isPanelActive;
            if (isPanelActive)
            {
                textComponent.text = "Hide Panel";
            }
            else
            {
                textComponent.text = "Show Panel";
            }
            panel.SetActive(isPanelActive);
        });

            gestureType.onClick.AddListener(() =>
        {
            current_mode = current_mode + 1;
            if (current_mode >= 3)
                current_mode = 0;

            TMP_Text textComponent = gestureType.GetComponentInChildren<TMP_Text>();
            if (current_mode == 0)
            {
                textComponent.text = "SHAPES";
                lineSnapper.gameObject.SetActive(false);
                shapeManager.gameObject.SetActive(true);
            }
            else if (current_mode == 1)
            {
                textComponent.text = "MEASURE";
                lineSnapper.gameObject.SetActive(true);
                shapeManager.gameObject.SetActive(false);
            }
            else
            {
                textComponent.text = "DISABLED";
                lineSnapper.gameObject.SetActive(false);
                shapeManager.gameObject.SetActive(false);
            }
                
        });

        rotate.onClick.AddListener(() =>
        {
            if (shapeManager.placedShapes.Count > 0)
            {
                ShapeObject shapeObj = shapeManager.placedShapes[shapeManager.placedShapes.Count - 1];

                if (shapeObj.shape == SHAPES.SQUARE || shapeObj.shape == SHAPES.CIRCLE)
                    return;

                shapeObj.actualShapeObj.transform.Rotate(new Vector3(0, 0, 90));

                if (!(shapeObj.shape == SHAPES.TRIANGLE || shapeObj.shape == SHAPES.RECTANGLE))
                    return;

                
                float zAngle = shapeObj.actualShapeObj.transform.eulerAngles.z;
                int orientation = Mathf.RoundToInt(zAngle / 90) % 4;
                switch (orientation)
                {
                    case 0:
                        shapeGenerator.OffsetPositionTo(shapeObj.actualShapeObj, new Vector3(0, -shapeObj.y, 0), shapeObj.x, shapeObj.y);
                        break;
                    case 1:
                        //move to the right based on height
                        shapeGenerator.OffsetPositionTo(shapeObj.actualShapeObj, new Vector3(shapeObj.y, 0, 0), shapeObj.x, shapeObj.y);
                        break;
                    case 2:
                        shapeGenerator.OffsetPositionTo(shapeObj.actualShapeObj, new Vector3(0, shapeObj.y, 0), shapeObj.x, shapeObj.y);
                        break;
                    case 3:
                        shapeGenerator.OffsetPositionTo(shapeObj.actualShapeObj, new Vector3(-shapeObj.y, 0, 0), shapeObj.x, shapeObj.y);
                        break;

                }
            }
        });
        // In HOGameBeh.Start() method, update the undo button listener
        undo.onClick.AddListener(() =>
       {
           if (current_mode == 1)
           {
               lineSnapper.OnUndoPressed();
               return;
           }

           else if (current_mode == 2)
           {
               return;
           }

           // Call the shape manager's Undo function and get the removed shape type
           ShapePlacementManager.ShapeType removedShapeType = shapeManager.Undo();

           // Convert ShapePlacementManager.ShapeType to HOGameBeh.SHAPES
           if (removedShapeType != ShapePlacementManager.ShapeType.None)
           {
               HOGameBeh.SHAPES shapeType = HOGameBeh.SHAPES.NONE;

               switch (removedShapeType)
               {
                   case ShapePlacementManager.ShapeType.Triangle:
                       shapeType = HOGameBeh.SHAPES.TRIANGLE;
                       break;
                   case ShapePlacementManager.ShapeType.Square:
                       shapeType = HOGameBeh.SHAPES.SQUARE;
                       break;
                   case ShapePlacementManager.ShapeType.Rectangle:
                       shapeType = HOGameBeh.SHAPES.RECTANGLE;
                       break;
                   case ShapePlacementManager.ShapeType.Circle:
                       shapeType = HOGameBeh.SHAPES.CIRCLE;
                       break;
                   case ShapePlacementManager.ShapeType.HalfCircle:
                       shapeType = HOGameBeh.SHAPES.SEMI_CIRCLE;
                       break;
               }

               // Restore the shape count if a valid shape was undone
               if (shapeType != HOGameBeh.SHAPES.NONE)
               {
       
                   if (shapeManager.placedShapes.Count <= 0)
                   {
                       undo.gameObject.SetActive(false);
                       rotate.gameObject.SetActive(false);
                   }
               }
           }
       });

     


        confirmMeasurement.onClick.AddListener(() =>
        {
 
            FillMarkedShapes();
            
        });

 




    }

    
    


    // no need for a variable select to hold

    public enum SHAPES
    {

        NONE,
        TRIANGLE,
        SQUARE,
        RECTANGLE,
        CIRCLE,
        SEMI_CIRCLE,
    }




    // Update is called once per frame
    void Update()
    {
        if (shapeManager != null && shapeManager.placedShapes.Count > 0)
        {
            rotate.gameObject.SetActive(true);
            undo.gameObject.SetActive(true);

        }
        //UnityEngine.Debug.Log("Line Snapper Value: " + lineSnapper.getMeasuredValue());


    }

    public void OnOptionSelect(int option)
    {
        currentOptionSelected = option;

        shapeManager.SetCurrentShape(ShapePlacementManager.ShapeType.None);

        SHAPES selectedShape = SHAPES.NONE;
        ShapePlacementManager.ShapeType managerShapeType = ShapePlacementManager.ShapeType.None;

        switch (option)
        {
            case 0:
                selectedShape = SHAPES.TRIANGLE;
                managerShapeType = ShapePlacementManager.ShapeType.Triangle;
                break;
            case 1:
                selectedShape = SHAPES.SQUARE;
                managerShapeType = ShapePlacementManager.ShapeType.Square;
                break;
            case 2:
                selectedShape = SHAPES.RECTANGLE;
                managerShapeType = ShapePlacementManager.ShapeType.Rectangle;
                break;
            case 3:
                selectedShape = SHAPES.CIRCLE;
                managerShapeType = ShapePlacementManager.ShapeType.Circle;
                break;
            case 4:
                selectedShape = SHAPES.SEMI_CIRCLE;
                managerShapeType = ShapePlacementManager.ShapeType.HalfCircle;
                break;
        }

        if (selectedShape != SHAPES.NONE)
        {
            shapeManager.SetCurrentShape(managerShapeType);
            shapeManager.gameObject.SetActive(true);  
        }
    }
    public class ShapeObject
    {
        public int x = UNUSED;
        public int y = UNUSED;
        public SHAPES shape;
        public GameObject actualShapeObj;
        public Vector3 offset = new Vector3(0, 0, 0);
        public bool isToBeFilled = false;
        public float angle = 0;
        public bool isExcess = false;

        public ShapeObject(int x, int y, SHAPES shape)
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

        public int maxGestures = UNUSED;

        public double area = UNUSED;

        Dictionary<SHAPES, int> shapesRemaining = new Dictionary<SHAPES, int>();
        Dictionary<SHAPES, List<Vector3>> shapeSizes = new Dictionary<SHAPES, List<Vector3>>();

        public int getPlacementShapeRemainingUse(SHAPES shape)
        {
            // Always return a positive number so shapes are always available
            return 999; // Effectively unlimited
        }

        public List<Vector3> getPlacementShapeSizes(SHAPES shape)
        {
            // Make sure the dictionary has an entry for this shape
            if (!shapeSizes.ContainsKey(shape))
            {
                shapeSizes[shape] = new List<Vector3>();
            }

            if (shapeSizes[shape].Count == 0)
            {
                return null;
                
            }

            return shapeSizes[shape];
        }

        public void addPlacementShapeRemainingUse(SHAPES shape, int value)
        {
            // No need to track remaining uses anymore
            // This function is kept for compatibility with existing code
        }

        public void addPlacementShapeSizes(SHAPES shape, Vector3 value)
        {
            if (!shapeSizes.ContainsKey(shape))
            {
                shapeSizes[shape] = new List<Vector3>();
            }
            shapeSizes[shape].Add(value);
        }

        //Area is manually calculated by level designer
        public SpellCastEvent(HOGameBeh behavior, List<ShapeObject> list, double area, int maxGestures)
        {
            this.main = behavior;
            this.shapes = list;
            this.area = area;

            this.maxGestures = maxGestures;
            this.initialize();
            int formulas = 0;
            foreach (ShapeObject o in shapes)
            {
                if (o.isToBeFilled)
                    formulas++; 
                // Initialize the list for this shape type if it doesn't exist
                if (!shapeSizes.ContainsKey(o.shape))
                {
                    shapeSizes[o.shape] = new List<Vector3>();
                }

                // Add the shape's size information
                Vector3 sizeVector;
                switch (o.shape)
                {
                    case SHAPES.TRIANGLE:
                        sizeVector = new Vector3(o.x, o.y, 0);
                        break;
                    case SHAPES.RECTANGLE:
                        sizeVector = new Vector3(o.x, o.y, 0);
                        break;
                    case SHAPES.SQUARE:
                        sizeVector = new Vector3(o.x, o.x, 0);
                        break;
                    case SHAPES.CIRCLE:
                        sizeVector = new Vector3(o.x, 0, 0);
                        break;
                    case SHAPES.SEMI_CIRCLE:
                        sizeVector = new Vector3(o.x, 0, 0);
                        break;
                    default:
                        sizeVector = new Vector3(0, 0, 0);
                        break;
                }

                shapeSizes[o.shape].Add(sizeVector);
            }
             this.main.dropdownManager.SetUpDropdownEquation(formulas);
        }

        public void initialize()
        {
            /*ShapeObject shape = this.shapes[0];
            this.shapes.RemoveAt(0);
            shape.actualShapeObj = generate(shape);*/
            foreach (ShapeObject shapeObject in this.shapes)
            {
                shapeObject.actualShapeObj = generate(shapeObject);
                shapeObject.actualShapeObj.transform.Rotate(0, 0, -shapeObject.angle);
            }
        }

        public GameObject generate(ShapeObject obj)
        {
            switch (obj.shape)
            {
                case SHAPES.SQUARE:
                    return this.main.shapeGenerator.CreateSquare(obj.offset, obj.x);

                case SHAPES.TRIANGLE:
                    return this.main.shapeGenerator.CreateTriangle(obj.offset, obj.x, obj.y);

                case SHAPES.CIRCLE:
                    return this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, false);

                case SHAPES.RECTANGLE:
                    return this.main.shapeGenerator.CreateRectangle(obj.offset, obj.x, obj.y);

                case SHAPES.SEMI_CIRCLE:
                    return this.main.shapeGenerator.CreateCircle(obj.offset, obj.x, true);

                default:
                    return null;
            }
        }

        public void destroyAllShapes()
        {
            foreach (ShapeObject obj in shapes)
            {
                Destroy(obj.actualShapeObj);
            }
            this.shapes.Clear();
        }

        public float GetFillPercentage(float input)
        {
            float targetArea = (float)area;
            return input / targetArea;
        }

    }

    IEnumerator WaitForComponent()
    {
        while (shapeGenerator == null)
        {
            shapeGenerator = GameObject.Find("ShapeGenerator").GetComponent<ShapeGenerator>();
            yield return new WaitForEndOfFrame();
        }

        shapeFiller = GameObject.Find("ShapeGenerator").GetComponent<ShapeFiller>();
        shapeManager = GameObject.Find("ShapeManager").GetComponent<ShapePlacementManager>();

        shapeManager.gameObject.SetActive(false);


        //generateProblem();
        List<ShapeObject> house = new List<ShapeObject>();

  

        house.Add(new ShapeObject(2, UNUSED, SHAPES.SQUARE).setIsToBeFilled());
        house.Add(new ShapeObject(2, 2, SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(1, 0, 0)).tilt(90));
        house.Add(new ShapeObject(2, 2, SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(-1, 0, 0)).tilt(-90));
        house.Add(new ShapeObject(2, 2, SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(0, 2, 0)));
        //house.Add(new ShapeObject(2, 2, SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(2, 0, 0)).tilt(180));

        UnityEngine.Debug.Log("CURRENT ANSWER - TOTAL AREA: " + ((0.5* 1 * Math.PI) + (0.5 * 1 * Math.PI) + 2 + 4));
        SetManualProblem(house, (0.5 * 1 * Math.PI) + (0.5 * 1 * Math.PI) + 2 + 4, 2);
    }

    /**Call to reset/set the level problem*/
    public void SetManualProblem(List<ShapeObject> list, double area, int gestures)
    {

        //slider.maxValue = (int)Math.Round(area * (1.5 + 0.5 * new System.Random().NextDouble()));

        this.spellCastEvent = new SpellCastEvent(this, list, area, gestures);

    }

    public void resetProblem()
    {
        List<ShapeObject> house = new List<ShapeObject>();
        house.Add(new ShapeObject(2, UNUSED, SHAPES.SQUARE).setIsToBeFilled());
        house.Add(new ShapeObject(2, 2, SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(1, 0, 0)).tilt(90));
        house.Add(new ShapeObject(2, 2, SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new Vector3(-1, 0, 0)).tilt(-90));
        house.Add(new ShapeObject(2, 2, SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(0, 2, 0)));
        house.Add(new ShapeObject(2, 2, SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(2, 0, 0)).tilt(180));



        SetManualProblem(house, 100, 2);
    }

    public void FillMarkedShapes()
    {
        if (spellCastEvent == null || spellCastEvent.shapes == null)
            return;

        StartCoroutine(FillShapesSequentially());
    }

    private IEnumerator FillShapesSequentially()
    {
        float fillSpeed = 0.75f; // return to 0.5 if some bug happens idk
        Color fillColor = Color.blue; 

    
        float answer = dropdownManager.CalculateAnswer();
        inputField.text = "Answer: " + answer.ToString();

        float total_area_from_answer = answer;

        // Calculate the fill ratio based on user input vs. expected area
        float fillRatio = total_area_from_answer / (float)spellCastEvent.area;
        if (fillRatio > 2.0f)
            fillRatio = 2.0f;

        correctionPerc.text = "Incorrectness: " + Math.Abs((int)((1 - fillRatio) * 100)) + "%";

        // Get all shapes that need to be filled
        List<ShapeObject> shapesToFill = spellCastEvent.shapes.FindAll(s => s.isToBeFilled && s.actualShapeObj != null);

        // Calculate the proportions of each shape relative to the total area
        Dictionary<ShapeObject, float> shapeProportions = new Dictionary<ShapeObject, float>();
        Dictionary<ShapeObject, float> shapeRelativeAreas = new Dictionary<ShapeObject, float>();

        float totalRelativeArea = 0;

        // First pass: calculate relative areas based on shape dimensions
        foreach (ShapeObject shape in shapesToFill)
        {
            float relativeArea = CalculateShapeArea(shape);
            shapeRelativeAreas[shape] = relativeArea;
            totalRelativeArea += relativeArea;
        }

        // Second pass: normalize to get proportions of the total area
        foreach (ShapeObject shape in shapesToFill)
        {
            shapeProportions[shape] = shapeRelativeAreas[shape] / totalRelativeArea;
        }

        // Calculate how much area each shape should get from the user's answer
        float remainingAreaToFill = total_area_from_answer;

        // Fill shapes sequentially
        for (int i = 0; i < shapesToFill.Count; i++)
        {
            ShapeObject shape = shapesToFill[i];

            // Calculate this shape's share of the total area
            float shapeTargetArea = (float)spellCastEvent.area * shapeProportions[shape];

            // Determine fill amount (0.0 to 1.0)
            float fillAmount;

            if (remainingAreaToFill >= shapeTargetArea)
            {
                // Fill the entire shape
                fillAmount = 1.0f;
                remainingAreaToFill -= shapeTargetArea;
            }
            else if (remainingAreaToFill > 0)
            {
                // Partial fill for this shape
                fillAmount = remainingAreaToFill / shapeTargetArea;
                remainingAreaToFill = 0;
            }
            else
            {
                // No more area to fill
                fillAmount = 0;
            }

            // Fill the shape
            shapeFiller.InitializeFill(shape.actualShapeObj, fillColor, fillSpeed, fillAmount);

            // Wait for fill animation to complete
            yield return new WaitForSeconds(fillSpeed * 4); // Approximate time to fill
        }
    }

    /*private IEnumerator FillShapesSequentially()
    {
        float fillSpeed = 0.5f; // Adjust fill speed as needed
        Color fillColor = Color.blue; // Choose appropriate fill color
        float total_area_from_answer = float.Parse(inputField.text);

        // Calculate what percentage of the total area we should fill based on user input
        float fillRatio = total_area_from_answer / (float)spellCastEvent.area;
        if (fillRatio > 2.0f)
            fillRatio = 2.0f;

        correctionPerc.text = "Incorrectness: " + Math.Abs((int)((1 - fillRatio) * 100)) + "%";

        float remainingAreaToFill = total_area_from_answer;
        List<ShapeObject> shapesToFill = spellCastEvent.shapes.FindAll(s => s.isToBeFilled && s.actualShapeObj != null);

        // Calculate total area of all shapes
        float totalShapeArea = 0;
        Dictionary<ShapeObject, float> shapeAreas = new Dictionary<ShapeObject, float>();

        foreach (ShapeObject shape in shapesToFill)
        {
            float area = CalculateShapeArea(shape);
            shapeAreas[shape] = area;


            totalShapeArea += area;
        }

        // Fill shapes sequentially
        for (int i = 0; i < shapesToFill.Count; i++)
        {
            ShapeObject shape = shapesToFill[i];
            float shapeArea = shapeAreas[shape];

            // Determine how much of this shape to fill
            float fillAmount = 1.0f; // Default to full fill

            if (remainingAreaToFill >= shapeArea)
            {
                // Fill the entire shape
                remainingAreaToFill -= shapeArea;
                // Use 1.0f for full fill
                shapeFiller.InitializeFill(shape.actualShapeObj, fillColor, fillSpeed, 1.0f);
            }
            else if (remainingAreaToFill > 0)
            {
                // Partial fill for the last shape
                fillAmount = remainingAreaToFill / shapeArea;
                remainingAreaToFill = 0;
                shapeFiller.InitializeFill(shape.actualShapeObj, fillColor, fillSpeed, fillAmount);
            }
            else
            {
                // No more area to fill
                //shapeFiller.InitializeFill(shape.actualShapeObj, fillColor, fillSpeed, 0f);
                break;
            }

            // Wait for fill animation to complete
            yield return new WaitForSeconds(fillSpeed * 4); // Approximate time to fill
        }
    }
    */
    private float CalculateShapeArea(ShapeObject shape)
    {
        switch (shape.shape)
        {
            case SHAPES.TRIANGLE:
                return 0.5f * shape.x * shape.y;
            case SHAPES.SQUARE:
                return shape.x * shape.x;
            case SHAPES.RECTANGLE:
                return shape.x * shape.y;
            case SHAPES.CIRCLE:
                return Mathf.PI * shape.x * shape.x;  // PI * r^2 (where r = x)
            case SHAPES.SEMI_CIRCLE:
                return 0.5f * Mathf.PI * shape.x * shape.x;  // 0.5 * PI * r^2
            default:
                return 0f;
        }
    }

    private void UpdateCorrectionPercentage()
    {
        float clamped = spellCastEvent.GetFillPercentage(UNUSED);
        if (clamped > 2.0f)
            clamped = 2.0f;

        correctionPerc.text = "Incorrectness: " + Mathf.Abs((int)((1 - clamped) * 100)) + "%";
    }

    /*    public void generateProblem()
        {
            System.Random random = new System.Random(seed);
            SHAPES randomShape = (SHAPES)(random.Next(1, Enum.GetValues(typeof(SHAPES)).Length));

            //UnityEngine.Debug.Log(randomShape);

            Problem problem = new Problem(randomShape, this);

            double result;

            switch (problem.problemShape)
            {
                case SHAPES.TRIANGLE:
                    result = (0.5 * problem.p_measure * problem.s_measure);
                    break;
                case SHAPES.CIRCLE:
                    result = (Math.PI * Math.Pow(problem.p_measure / 2, 2));
                    break;
                case SHAPES.RECTANGLE:
                    result = (problem.p_measure * problem.s_measure);
                    break;
                case SHAPES.SQUARE:
                    result = Math.Pow(problem.p_measure, 2);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    result = (0.5 * Math.PI * Math.Pow(problem.p_measure / 2, 2));
                    break;
                default:
                    throw new Exception("Invalid shape");
                    //throw this shit 
            }

            slider.maxValue = (int)Math.Round(result * (1.5 + 0.5 * random.NextDouble()));

            this.spellCastEvent = new SpellCastEvent(this);
        }*/

    
}