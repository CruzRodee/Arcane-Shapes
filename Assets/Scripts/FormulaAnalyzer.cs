using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class FormulaAnalyzer : MonoBehaviour
{
    private float evalAnswer = -1, tempAns = -1; //Contains the answer to the evaluated expression, default: -1
    private string inputAnswer = ""; //Contains the answer from player input, default: "", to be parsed into a float if needed
    private string evalString = "", displayString = ""; //Strings for storing the input, first for eval second for display to UI
    private string calcDispString = "", tempDisp = "", tempEval = "", tempInput = ""; //Strings for calcMode
    private bool isValidFormula = false, tempValid = false; //Contains result of eval
    private bool equMode = false, tempEqu = false; //If true, user has inputed "=" and formula eval + checking correct answer begins
    private GameBehaviour.SHAPES formulaShape = GameBehaviour.SHAPES.NONE, tempShape = GameBehaviour.SHAPES.NONE;

    private float sideA = 0, sideB = 0, tempA = 0, tempB = 0; //Floats for side legths of shape

    //String arrays containing values and operators. NOTE: NEEDS TO BE MATCHED TO CLASSES ARRAY IN OCROBJECT
    private readonly string[] VALUES = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "." }; //"." for decimals
    private readonly string[] OPERATORS = { "\u00f7", "=", "-", "+", "\u00d7" };

    private bool DEBUGCALCFLAG = false;
    public bool DEBUG = false;
    public bool DEBUG_RESET = false; //Used for restarting everything for testing
    public bool calcMode = false; //Flag for calculator mode so that it spits out the answer
    private bool isLOGame = true; //Boolean that determins if game is LO or HO
    public bool isCompoundArea = false; //Flag for determining if inputting an HO final answer and disables shape verification

    public GameObject gbHolder; //Object that holds GB
    private GameBehaviour gb; //Reference to GB script, assign this during Start()
    public GameObject formulaDispObj; //Insert tmp object here
    private TextMeshProUGUI formulaDispGUI; //Set this during Start() with get component

    private const string formulaDefaultText = "Isulat ang formula sa Board";
    private const string calculatorDefaultText = "Calculate";

    //Magic regex runes, the rest in AreaFormulaParser

    private readonly Regex validFormulaRegex = new Regex(@"^\(*((\d+(?:\.\d+)?|pi|π|3\.1416)\)*([\+\-\*/\^]\(*(\d+(?:\.\d+)?|pi|π|3\.1416)\)*)+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //---

    public HOGameScript hgb = null;

    // Start is called before the first frame update
    void Start()
    {
        //DEBUG
        if (DEBUG)
            Debug.Log("FA: HELLO WORLD");

        //Grab GUI of textmeshpro object
        formulaDispGUI = formulaDispObj.GetComponent<TextMeshProUGUI>();
        //Init text for GUI
        formulaDispGUI.text = formulaDefaultText;

        //Grab Gamebehavior script if LO
        //hgb = gbHolder.GetComponent<HOGameScript>();

        if (hgb != null)
        {
            print("HIGHER ORDER GAME MODE");
            isLOGame = false;
        }
        else
        {
            gb = gbHolder.GetComponent<GameBehaviour>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG)
        {
            if (DEBUG_RESET)
            {
                Debug.ClearDeveloperConsole();
                ResetAnalyzer();
                DEBUG_RESET = false;
                Debug.Log("FA: ANALYZER RESET!");
                DebugDisplay();
            }

            if (calcMode && !DEBUGCALCFLAG)
            {
                //run EnterCalc
                EnterCalc();
            }

            if (!calcMode && DEBUGCALCFLAG) //If not calcMode and tempDisp != "" (temps contain something and still need exit)
            {
                //Run ExitCalc(), clear tempDisp and tempEval
                ExitCalc();
            }
        }
    }

    private void UpdateCalcDisp(string updateString)
    {
        if (calcMode)
        {
            calcDispString = updateString;
        }
    }

    public void EnterCalc()
    {
        if (DEBUG) DEBUGCALCFLAG = true; //Set flag for debug

        calcMode = true; //Trigger flag

        //Clear calc display
        calcDispString = "";

        //Update GUI
        formulaDispGUI.text = calcDispString;

        //Store input data in temp
        tempDisp = displayString;
        tempEval = evalString;
        tempInput = inputAnswer;
        tempAns = evalAnswer;
        tempValid = isValidFormula;
        tempEqu = equMode;
        tempShape = formulaShape;
        tempA = sideA;
        tempB = sideB;

        //Clear data
        ResetAnalyzer();

        //Update data collection on numCalcUsed
        if(gb != null)
        {
            gb.levelData.numCalcUsed++;
        }
        else if (hgb != null)
        {
            hgb.levelData.numCalcUsed++;
        }

        if (DEBUG) DebugDisplay();
    }

    public void ExitCalc()
    {
        calcMode = false; //Resey flag

        //Restore data from temp
        displayString = tempDisp;
        evalString = tempEval;
        inputAnswer = tempInput;
        evalAnswer = tempAns;
        isValidFormula = tempValid;
        equMode = tempEqu;
        formulaShape = tempShape;
        sideA = tempA;
        sideB = tempB;

        //Update GUI
        if (displayString.Length > 0) //If display contains anything, show that
            formulaDispGUI.text = displayString;
        else //Else show default text
            formulaDispGUI.text = formulaDefaultText;

        //Reset flag for calc mode debug
        if (DEBUG) DEBUGCALCFLAG = false; //Set flag for debug

        if (DEBUG) DebugDisplay();
    }

    //Method for appending to the input string variables
    public void InputString(string input)
    {
        //Update data collection on numOCRInput
        if (gb != null)
        {
            gb.levelData.numOCRInput++;
        }
        else if (hgb != null)
        {
            hgb.levelData.numOCRInput++;
        }

        //Check for the non-number inputs that may be different between evalString and displayString
        //Else, number strings are same in both
        //Special function for switching to comparing if equ is input
        switch (input)
        {
            //Identical between eval and display
            case "Lpar":
                if (equMode) break;
                evalString += "(";
                displayString += "(";
                UpdateCalcDisp(displayString);
                break;
            case "Rpar":
                if (equMode) break;
                evalString += ")";
                displayString += ")";
                UpdateCalcDisp(displayString);
                break;
            case "dec":
                if (!equMode)
                    evalString += ".";
                displayString += ".";
                if (equMode)
                    inputAnswer += ".";
                UpdateCalcDisp(displayString);
                break;
            case "min":
                if (equMode) break;
                evalString += "-";
                displayString += "-";
                UpdateCalcDisp(displayString);
                break;
            case "plu":
                if (equMode) break;
                evalString += "+";
                displayString += "+";
                UpdateCalcDisp(displayString);
                break;
            //Different between eval and display
            case "div":
                if (equMode) break;
                evalString += "/";
                displayString += "\u00f7";
                UpdateCalcDisp(displayString);
                break;
            case "tim":
                if (equMode) break;
                evalString += "*";
                displayString += "\u00d7";
                UpdateCalcDisp(displayString);
                break;
            case "pi":
                if (equMode) break; //Not 100% sure if pi not needed in answering
                evalString += input;
                displayString += "\u03C0";
                UpdateCalcDisp(displayString);
                break;
            //Special case: equ start evaluating the evalString and compares evalAnswer with inputAnswer after a trigger
            case "equ":
                if (calcMode)
                {
                    //Required to not break things
                    displayString += "=";

                    //Evaluate Formula, show answer, then clear inputs
                    EvaluateFormula();
                    UpdateCalcDisp(evalAnswer.ToString());
                    Debug.Log("FA: calcDispString -> " + calcDispString); //Just for debug, add actual method to release answer
                    ResetAnalyzer();
                    break;
                }

                if (!equMode)
                {
                    //Add symbol to dispLay but not eval
                    displayString += "=";

                    //Call EvaluateFormula
                    EvaluateFormula();
                    if (DEBUG && isValidFormula) //Also need to check if valid formula
                        Debug.Log("FA: Formula Shape is " + formulaShape);
                    //Flag equMode to start getting input answer if valid formula
                    if (isValidFormula)
                        equMode = true;
                    else if (!isValidFormula) //Reset analyzer if invalid
                    {
                        if (DEBUG)
                        {
                            Debug.Log("FA: Invalid input, try again");
                        }
                        ResetAnalyzer();

                        //Notify gb of invalid formula
                        if (gb != null)
                        {
                            gb.NotifyInvalidFormula();
                        }
                        //Notify hgb of invalid formula
                        if (hgb != null)
                        {
                            hgb.NotifyInvalidFormula();
                        }
                    }
                }
                else if (equMode)
                {
                    //Compare evalAnswer and inputAnswer, send to GB script if equal
                    if (CheckInputAnswer())
                    {
                        //Reset Analyzer to base state if equal
                        ResetAnalyzer();
                        break; //Early break to stop from running code below
                    }

                    //Else reset inputAnswer and remove all nums after = in displayAnswer to try again
                    ResetInputAns();
                    int equIndex = displayString.IndexOf("=");
                    if ((equIndex + 1) < displayString.Length) //Check for String length first
                        displayString = displayString.Remove(equIndex + 1);
                    if (DEBUG)
                    {
                        DebugDisplay();
                        Debug.Log("displayString length: " + displayString.Length);
                    }

                    //Notify gb of invalid answer
                    if (gb != null)
                    {
                        gb.NotifyMismatchedAnswer();
                    }
                    //Notify hgb of invalid answer
                    if (hgb != null)
                    {
                        hgb.NotifyMismatchedAnswer();
                    }
                }
                break;
            //Numbers
            default:
                if (!equMode) //Dont add if in equMode
                    evalString += input;
                displayString += input;
                if (equMode) //In equMode,
                    inputAnswer += input;
                UpdateCalcDisp(displayString);
                break;
        }

        //Debug info
        if (!input.Equals("equ"))
            DebugDisplay();

        //Update TMP display
        if (!calcMode)
            formulaDispGUI.text = displayString;
        else if (calcMode)
            formulaDispGUI.text = calcDispString;

        PrintDefaultText();
    }

    private bool CheckInputAnswer()
    {
        //TODO: USE THIS METHOD TO CHECK IF USER SOLVED MATH INPUT CORRECTLY
        if (evalAnswer.ToString() == inputAnswer)
        {
            //Send a message or activate LO GB the input answer, end early
            //print("Test");
            //print(isLOGame);
            //print(gb != null);
            if (isLOGame)
            {
                UnityEngine.Debug.Log("LO Game Check");

                if (gb != null)
                {
                    UnityEngine.Debug.Log("Parse Attempt");
                    gb.InputAnswer(float.Parse(inputAnswer));
                    return true;
                }
                return false; //Error if gb is not defined for some reason
            }
            else
            {
                if (hgb != null)
                {
                    if (isCompoundArea) //Flag as final answer if sending valid compound area
                        hgb.isFinalAnswer = true;

                    hgb.InputAnswer(float.Parse(inputAnswer));
                    return true;
                }
            }

            //DEBUG
            if (DEBUG)
            {
                DebugDisplay();
                Debug.Log("FA: evalAnswer is same as inputAnswer, sending answer to gb for final check");
            }

            //return true
            return true;
        }

        //DEBUG
        if (DEBUG)
        {
            DebugDisplay();
            Debug.Log("FA: evalAnswer not same as inputAnswer, user calcs incorrect or error, try again...");
        }
        return false; //Default response
    }

    //Used for evaluating calculator input on click
    private string CalculatorEvaluate()
    {
        string evalResult = "Invalid Input"; //Default to invalid

        try
        {          
            bool isValidExpression = ExpressionEvaluator.Evaluate(tempEval, out float evaluatorReturn);

            //Round evalAnswer to 2 decimal places, do same for all answers
            if (isValidExpression)
                evalResult = ((float)System.Math.Round(evaluatorReturn, 2)).ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e); //Logs exception
        }

        return evalResult;
    }

    //Evualutaes the string formula with NCalc Expression.Evaluate(), stores answer in evalAnswer formula validity in isValidFormula
    private void EvaluateFormula()
    {
        evalString = evalString.Trim(); //Input cleaning

        if (!validFormulaRegex.IsMatch(evalString)) //Block invalid inputs
        {
            isValidFormula = false;
            if (DEBUG) Debug.Log("Invalid Formula: " + evalString);
            return;
        }

        try
        {
            isValidFormula = ExpressionEvaluator.Evaluate(evalString, out evalAnswer);
            //Round evalAnswer to 2 decimal places, do same for all answers
            evalAnswer = (float)System.Math.Round(evalAnswer, 2);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e); //Logs exception
            isValidFormula = false; //Formula not valid if exception
            if (!equMode) //Reset the evalAnswer if formula not valid and user has not inputed "=" successfully yet
                ResetEvalAns();
        }
        finally
        {
            //Final eval, check shape, ignore if inputting Compound Area
            if (!isCompoundArea)
            {
                formulaShape = EvalFormulaShape();
                if (formulaShape == GameBehaviour.SHAPES.NONE && !calcMode) //If no shape, formula invalid if not calcMode
                    isValidFormula = false;
            }
            DebugDisplay();
        }
    }

    private void PrintDefaultText()
    {
        //Display default text based on mode
        if (!calcMode && displayString.Length < 1)
            formulaDispGUI.text = formulaDefaultText;
        else if (calcMode && calcDispString.Length < 1 && tempEval.Length > 1)
            formulaDispGUI.text = $"Answer: {CalculatorEvaluate()}";
        else if (calcMode && calcDispString.Length < 1)
            formulaDispGUI.text = calculatorDefaultText;
    }

    //Resetters
    public void ResetAnalyzer()
    {
        ResetEvalAns();
        ResetInputAns();
        ResetString();
        ResetSides();
        ResetValidForm();
        ResetEquMode();
        ResetFormulaShape();
        ResetGUI();
        PrintDefaultText();
    }
    private void ResetEvalAns()
    {
        evalAnswer = -1;
    }
    private void ResetInputAns()
    {
        inputAnswer = "";
    }
    private void ResetString()
    {
        displayString = "";
        evalString = "";
    }
    private void ResetSides()
    {
        sideA = 0;
        sideB = 0;
    }
    private void ResetValidForm()
    {
        isValidFormula = false;
    }
    private void ResetEquMode()
    {
        equMode = false;
    }
    private void ResetFormulaShape()
    {
        formulaShape = GameBehaviour.SHAPES.NONE;
    }
    private void ResetGUI()
    {
        //Whoops, forgot to reset the GUI on reset
        formulaDispGUI.text = displayString;
    }
    public void ResetCalcDisp()
    {
        calcDispString = "";
    }

    public void BackspaceInput() // Working on a backspace method
    {
        if (calcMode)
        {
            // Trim the last character from calcDispString
            if (calcDispString.Length > 0)
            {
                calcDispString = calcDispString.Substring(0, calcDispString.Length - 1);
                formulaDispGUI.text = calcDispString;
            }
            return;
        }

        if (equMode)
        {
            //If Last char is a '=' character, deactivate equ mode and set valid formula to false
            if (displayString.Last() == '=')
            {
                isValidFormula = false;
                equMode = false;
            }

            // Remove last char from inputAnswer and displayString
            if (inputAnswer.Length > 0)
                inputAnswer = inputAnswer.Substring(0, inputAnswer.Length - 1);

            if (displayString.Length > 0)
                displayString = displayString.Substring(0, displayString.Length - 1);
        }
        else
        {
            // Formula input mode
            if (evalString.Length > 0)
            {
                if (evalString.Last() == 'i') //Special Case for pi, two backspaces here instead of 1 due to being two chars
                    evalString = evalString.Substring(0, evalString.Length - 2);
                else
                    evalString = evalString.Substring(0, evalString.Length - 1);
            }

            if (displayString.Length > 0)
                displayString = displayString.Substring(0, displayString.Length - 1);
        }

        // Update GUI after edit
        formulaDispGUI.text = displayString;
        PrintDefaultText();

        if (DEBUG)
        {
            Debug.Log("FA: Backspace applied.");
            DebugDisplay();
        }
    }

    //Getters
    public float GetEvalAns() //Can be used to get output of calc mode?
    {
        return evalAnswer;
    }
    public float GetSideA()
    {
        return sideA;
    }
    public float GetSideB()
    {
        return sideB;
    }
    public GameBehaviour.SHAPES GetFormulaShape()
    {
        return formulaShape;
    }
    public bool GetIsEquMode()
    {
        return equMode;
    }

    //Returns the shape of the formula
    private GameBehaviour.SHAPES EvalFormulaShape()
    {
        GameBehaviour.SHAPES evalShape = GameBehaviour.SHAPES.NONE; //Store return value of shape eval
        float[] vals = { 0, 0 }; //Store return of ExtractVariables()

        string cleanString = evalString; //Clean all *0.5, 0.5*, *1/2, 1/2*, /2
        bool isDivided = false; //Flag for any dividers appearing
        string[] dividers = { "*0.5", "0.5*", "*1/2", "1/2*", "/2" };
        foreach (string divider in dividers)
        {
            if (cleanString.Contains(divider))
            {
                cleanString = cleanString.Replace(divider, ""); //Erase divider
                isDivided = true;
                if (DEBUG) Debug.Log("IsDivided string: " + cleanString);
            }
        }

        if (!isDivided && AreaFormulaParser.squareRegex.IsMatch(evalString))
        {
            evalShape = GameBehaviour.SHAPES.SQUARE;
            vals = AreaFormulaParser.ExtractVariables(evalString, evalShape);
        }

        else if (!isDivided && AreaFormulaParser.rectangleRegex.IsMatch(evalString))
        {
            evalShape = GameBehaviour.SHAPES.RECTANGLE;
            vals = AreaFormulaParser.ExtractVariables(evalString, evalShape);
        }

        else if (isDivided && (AreaFormulaParser.squareRegex.IsMatch(cleanString) || AreaFormulaParser.rectangleRegex.IsMatch(cleanString)))
        {
            evalShape = GameBehaviour.SHAPES.TRIANGLE;

            //Use rect or square parser on cleanString, both proven to work
            if (AreaFormulaParser.squareRegex.IsMatch(cleanString))
                vals = AreaFormulaParser.ExtractVariables(cleanString, GameBehaviour.SHAPES.SQUARE);
            else
                vals = AreaFormulaParser.ExtractVariables(cleanString, GameBehaviour.SHAPES.RECTANGLE);
        }

        else if (!isDivided && AreaFormulaParser.circleRegex.IsMatch(evalString))
        {
            evalShape = GameBehaviour.SHAPES.CIRCLE;
            vals = AreaFormulaParser.ExtractVariables(evalString, evalShape);
        }

        else if (isDivided && AreaFormulaParser.circleRegex.IsMatch(cleanString))
        {
            evalShape = GameBehaviour.SHAPES.SEMI_CIRCLE;

            //Use circle parser on cleanString
            vals = AreaFormulaParser.ExtractVariables(cleanString, GameBehaviour.SHAPES.CIRCLE);
        }


        if (DEBUG)
        {
            Debug.Log("Shape Type: " + evalShape);
            if (evalShape == GameBehaviour.SHAPES.NONE) Debug.Log("Failed to find shape");
        }

        //Store vals as sideA and sideB
        try
        {
            if (vals.Length >= 2)
            {
                sideA = vals[0];
                sideB = vals[1];
            }
            else
            {
                sideA = vals[0];
            }
        }
        catch (System.Exception)
        {
            return GameBehaviour.SHAPES.NONE; //Either invalid input or logic error
        }

        return evalShape; //Either invalid input or logic error
    }
    //
    //Prints debug info to Log
    public void DebugDisplay()
    {
        if (!DEBUG) //Do nothing if not debug mode
            return;
        if (!calcMode)
            Debug.Log("evalString: " + evalString + " | displayString: " + displayString + " | sideA: " + sideA);
        else
            Debug.Log("evalString: " + evalString + " | calcDispString: " + calcDispString + " | sideA: " + sideA);
        Debug.Log("isValidFormula: " + isValidFormula + " | evalAnswer: " + evalAnswer + " | inputAnswer: " + inputAnswer + " | sideB: " + sideB);
    }
}