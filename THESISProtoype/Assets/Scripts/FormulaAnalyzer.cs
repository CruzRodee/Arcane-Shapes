using System.Collections.Generic;
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
    public bool isFinalAnswer = false, tempFinal = false; //Boolean trigger for comparing inputAnswer and evalAnswer to determine if player formula input is good

    private float sideA = 0, sideB = 0, tempA = 0, tempB = 0; //Floats for side legths of shape

    //String arrays containing values and operators. NOTE: NEEDS TO BE MATCHED TO CLASSES ARRAY IN OCROBJECT
    private readonly string[] VALUES = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "." }; //"." for decimals
    private readonly string[] OPERATORS = { "\u00f7", "=", "-", "+", "\u00d7" };

    private bool DEBUGCALCFLAG = false;
    public bool DEBUG = false;
    public bool DEBUG_RESET = false; //Used for restarting everything for testing
    public bool calcMode = false; //Flag for calculator mode so that it spits out the answer
    public bool isLOGame = true; //Boolean that determins if game is LO or HO

    public GameObject gbHolder; //Object that holds GB
    private GameBehaviour gb; //Reference to GB script, assign this during Start()
    public GameObject formulaDispObj; //Insert tmp object here
    private TextMeshProUGUI formulaDispGUI; //Set this during Start() with get component

    private const string formulaDefaultText = "Enter Formula in Board";
    private const string calculatorDefaultText = "Calculator";

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
        if (isLOGame)
            gb = gbHolder.GetComponent<GameBehaviour>();
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
        tempFinal = isFinalAnswer;
        tempA = sideA;
        tempB = sideB;

        //Clear data
        ResetAnalyzer();

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
        isFinalAnswer = tempFinal;
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
                    displayString = displayString.Remove(displayString.IndexOf("=") + 1);
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
            if (isLOGame)
            {
                if (gb != null)
                {
                    gb.InputAnswer(float.Parse(inputAnswer));
                    return true;
                }
                return false; //Error if gb is not defined for some reason
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

    //Evualutaes the string formula with NCalc Expression.Evaluate(), stores answer in evalAnswer formula validity in isValidFormula
    private void EvaluateFormula()
    {
        if (displayString.Length - 1 < 3) //There must be at least 2 vals and 1 operator ergo 3 chars, reduce length by 1 to remove "="
        {
            isValidFormula = false; //Formula not valid if exception
            if (!equMode) //Reset the evalAnswer if formula not valid and user has not inputed "=" successfully yet
                ResetEvalAns();
            DebugDisplay();
            return;
        }

        if (DEBUG) Debug.Log("FA: PASS 1");

        //Check if ending char of evalString is a number, "pi" or ".", if not, invalid formula
        bool endIsNumber = false;
        char[] maybePi = new char[2];
        foreach (string val in VALUES)
        {
            maybePi[0] = evalString[^2];
            maybePi[1] = evalString[^1];
            if (maybePi[0].ToString() + maybePi[1].ToString() == "pi")
            {
                endIsNumber = true;
                break;
            }
            if (evalString[^1] == val.ToCharArray()[0])
            {
                endIsNumber = true;
                break;
            }
        }

        if (DEBUG) Debug.Log("FA: maybePi: " + maybePi[0].ToString() + maybePi[1].ToString());

        if (!endIsNumber)
        {
            isValidFormula = false;
            return;
        }

        if (DEBUG) Debug.Log("FA: PASS 2");

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
            //Final eval, check shape
            formulaShape = EvalFormulaShape();
            if (formulaShape == GameBehaviour.SHAPES.NONE && !calcMode) //If no shape, formula invalid if not calcMode
                isValidFormula = false;
            DebugDisplay();
        }
    }

    private void PrintDefaultText()
    {
        //Display default text based on mode
        if (!calcMode && displayString.Length < 1)
            formulaDispGUI.text = formulaDefaultText;
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
        ResetFinalAns();
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
    private void ResetFinalAns()
    {
        isFinalAnswer = false;
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


    //Returns the shape of the formula
    private GameBehaviour.SHAPES EvalFormulaShape()
    {
        //Store in duplicate string and remove all 0.5 or 1/2 or pi or unreadables from duplicate string to easily find vals
        string duplicateString = new('a', displayString.Length);
        duplicateString = duplicateString.Replace(duplicateString, displayString); //Copy display string
        duplicateString = duplicateString.Replace("0.5", "");
        duplicateString = duplicateString.Replace("1\u00f72", ""); // 1/2
        duplicateString = duplicateString.Replace("\u00f72", ""); // /2, removed after 1/2 to not mess things up
        duplicateString = duplicateString.Replace("\u03C0", ""); //pi
        duplicateString = duplicateString.Replace("=", ""); // remove the "=" too
        duplicateString = duplicateString.Replace("(", ""); // remove the "(" too
        duplicateString = duplicateString.Replace(")", ""); // remove the ")" too

        if (DEBUG)
        {
            Debug.Log("FA: duplicateString: " + duplicateString);
        }

        //Separate duplicateString into operators and values to determine if rect or square
        List<string> vals = new();
        vals.Add(""); //Add blank string since we only add new element when we get an operator
        List<string> ops = new();
        bool isOps = false; //Check for if char is an operator

        for (int i = 0; i < duplicateString.Length; i++)
        {
            if (DEBUG)
                Debug.Log("FA: current char is -> " + duplicateString.ToCharArray()[i]);

            //Check if char in OPERATORS, add new blank vals element if i < duplicateString.Length and vals.Last() != ""
            foreach (string s in OPERATORS)
            {
                if (duplicateString.ToCharArray()[i] == s.ToCharArray()[0])
                {
                    if (DEBUG)
                        Debug.Log("FA: Operator found! -> " + s);

                    if (i < duplicateString.Length && vals.Last() != "")
                        vals.Add("");
                    ops.Add(s);
                    isOps = true; //char is an operator
                    break; //Match found end loop early
                }
            }

            //Else add char to vals.Last()
            if (!isOps) //If not an operator
            {
                vals[^1] += duplicateString[i];
            }
            isOps = false; //Reset flag
        }

        if (DEBUG)
        {
            Debug.Log("FA: vals length is " + vals.Count());
            foreach (string s in vals)
            {
                Debug.Log("FA: vals contains " + s);
            }
        }

        //Prune All Empty strings from vals until non-empty is reached
        while (vals.Last() == "")
        {
            if (DEBUG)
                Debug.Log("FA: Removed empty string!");
            vals.RemoveAt(vals.IndexOf(vals.Last()));
        }

        //Store vals as sideA and sideB
        try
        {
            sideA = float.Parse(vals[0]);
            sideB = float.Parse(vals[1]);
        }
        catch (System.Exception)
        {
            return GameBehaviour.SHAPES.NONE; //Either invalid input or logic error
        }

        //No shape if there are operators other than *, /, and =
        foreach (string op in ops)
        {
            if (op == "-" || op == "+")
            {
                return GameBehaviour.SHAPES.NONE;
            }
        }

        Regex triangleRegex = new(@"\(\d+\.?\d*\u00d7\d+\.?\d*\)\u00f72"); //Checks if the formula is surrounded by () div 2
        //Checks if the formula is surrounded by () div 2 and has pi
        Regex semiCircleRegex = new(@"\(\u03C0?\u00d7?\d+\.?\d*\u00d7?\u03C0?\u00d7\d+\.?\d*\u00d7?\u03C0?\)\u00f72");

        // 1.) if has 0.5 or 1/2 or ()÷2 but no pi, is triangle
        if ((displayString.Contains("0.5") || displayString.Contains("1\u00f72") ||
            triangleRegex.IsMatch(displayString)) && !displayString.Contains("\u03C0"))
            return GameBehaviour.SHAPES.TRIANGLE;

        //Compare if values are equal, square if yes otherwise rect, invalid if there are more than 2 vals
        if (vals.Count() > 1 && vals.Count() < 3)
        {
            if (sideA == sideB) //Compare parsed instead, should be equal if same value
            {
                // 2.) if pi is present and same sides, is circle if no 0.5 or 1/2 or () div 2 else semicircle
                if (displayString.Contains("\u03C0"))
                {
                    if (displayString.Contains("0.5") || displayString.Contains("1\u00f72") ||
                        semiCircleRegex.IsMatch(displayString))
                        return GameBehaviour.SHAPES.SEMI_CIRCLE;
                    else return GameBehaviour.SHAPES.CIRCLE;
                }
                else //If no pi its a square
                    return GameBehaviour.SHAPES.SQUARE;
            }
            else if (!displayString.Contains("\u03C0")) //If is doesn't contain pi and not same side rectangle
                return GameBehaviour.SHAPES.RECTANGLE;
        }

        return GameBehaviour.SHAPES.NONE; //Either invalid input or logic error
    }

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
