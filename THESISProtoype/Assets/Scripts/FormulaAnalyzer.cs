using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormulaAnalyzer : MonoBehaviour
{
    private float evalAnswer = -1; //Contains the answer to the evaluated expression, default: -1
    private float inputAnswer = -9999; //Contains the answer from player input, default: -9999
    private string evalString = "", displayString = ""; //Strings for storing the input, first for eval second for display to UI
    private bool isValidFormula = false; //Contains result of eval
    private bool equMode = false; //If true, user has inputed "=" and formula eval + checking correct answer begins
    public bool isFinalAnswer = false; //Boolean trigger for comparing inputAnswer and evalAnswer to determine if player formula input is good

    private float sideA = 0, sideB = 0; //Floats for side legths of shape

    //String arrays containing values and operators. NOTE: NEEDS TO BE MATCHED TO CLASSES ARRAY IN OCROBJECT
    private readonly string[] VALUES = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "." }; //"." for decimals
    private readonly string[] OPERATORS = { "\u00f7", "=", "-", "+", "\u00d7" };

    public bool DEBUG = false;
    public bool DEBUG_RESET = false; //Used for restarting everything for testing
    public bool calcMode = false; //Flag for calculator mode so that it spits out the answer
    
    // Start is called before the first frame update
    void Start()
    {
        //DEBUG
        if(DEBUG)
            Debug.Log("FA: HELLO WORLD");
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
        }
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
                evalString += "(";
                displayString += "(";
                break;
            case "Rpar":
                evalString += ")";
                displayString += ")";
                break;
            case "dec":
                evalString += ".";
                displayString += ".";
                break;
            case "min":
                evalString += "-";
                displayString += "-";
                break;
            case "plu":
                evalString += "+";
                displayString += "+";
                break;
            //Different between eval and display
            case "div":
                evalString += "/";
                displayString += "\u00f7";
                break;
            case "tim":
                evalString += "*";
                displayString += "\u00d7";
                break;
            case "pi":
                evalString += input;
                displayString += "\u03C0";
                break;
            //Special case: equ start evaluating the evalString and compares evalAnswer with inputAnswer after a trigger
            case "equ":
                if (calcMode)
                {
                    //Evaluate Formula, show answer, then clear inputs
                    EvaluateFormula();
                    Debug.Log("FA: Answer -> " + evalAnswer); //Just for debug, add actual method to release answer
                    ResetString();
                    break;
                }
                
                if (!equMode)
                {
                    //Call EvaluateFormula and GetFormulaShape(Maybe, needs testing since not sure where to call)
                    EvaluateFormula();
                    if (DEBUG && isValidFormula) //Also need to check if valid formula
                        Debug.Log("FA: Formula Shape is " + GetFormulaShape());
                    //Flag equMode to start getting input answer
                }
                else if (equMode)
                {

                }
                    break;
            //Numbers
            default:
                evalString += input;
                displayString += input;
                break;
        }

        if(!input.Equals("equ"))
            DebugDisplay();
    }

    private void CheckUserAnswer()
    {
        //TODO: USE THIS METHOD TO CHECK IF USER SOLVED MATH INPUT CORRECTLY
    }

    //Evualutaes the string formula with NCalc Expression.Evaluate(), stores answer in evalAnswer formula validity in isValidFormula
    private void EvaluateFormula()
    {
        if(displayString.Length < 3) //There must be at least 2 vals and 1 operator ergo 3 chars
        {
            isValidFormula = false; //Formula not valid if exception
            if (!equMode) //Reset the evalAnswer if formula not valid and user has not inputed "=" successfully yet
                ResetEvalAns();
            DebugDisplay();
            return;
        }
        
        try
        {
            isValidFormula = ExpressionEvaluator.Evaluate(evalString, out evalAnswer);
            //Round evalAnswer to 2 decimal places, do same for all answers
            evalAnswer = (float)System.Math.Round(evalAnswer, 2);
            DebugDisplay();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e); //Logs exception
            isValidFormula = false; //Formula not valid if exception
            if(!equMode) //Reset the evalAnswer if formula not valid and user has not inputed "=" successfully yet
                ResetEvalAns();

            DebugDisplay();
        }
    }

    //Resetters
    private void ResetAnalyzer()
    {
        ResetEvalAns();
        ResetInputAns();
        ResetString();
        ResetSides();
        ResetValidForm();
        ResetEquMode();
        ResetFinalAns();
    }
    private void ResetEvalAns()
    {
        evalAnswer = -1;
    }
    private void ResetInputAns()
    {
        inputAnswer = -9999;
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

    //Returns the shape of the formula
    public GameBehaviour.SHAPES GetFormulaShape()
    {
        //Store in duplicate string and remove all 0.5 or 1/2 or pi from duplicate string to easily find vals
        string duplicateString = new('a', displayString.Length);
        duplicateString = duplicateString.Replace(duplicateString, displayString); //Copy display string
        duplicateString = duplicateString.Replace("0.5", "");
        duplicateString = duplicateString.Replace("1\u00f72", ""); // 1/2
        duplicateString = duplicateString.Replace("\u03C0",""); //pi

        //Separate duplicateString into operators and values to determine if rect or square
        List<string> vals = new();
        vals.Add(""); //Add blank string since we only add new element when we get an operator
        List<string> ops = new();
        bool isOps = false; //Check for if char is an operator

        for(int i = 0; i < duplicateString.Length; i++)
        {
            if (DEBUG)
                Debug.Log("FA: current char is -> " + duplicateString.ToCharArray()[i]);
            
            //Check if char in OPERATORS, add new blank vals element if i < duplicateString.Length and vals.Last() != ""
            foreach(string s in OPERATORS)
            {                
                if (duplicateString.ToCharArray()[i] == s.ToCharArray()[0])
                {
                    if (DEBUG)
                        Debug.Log("FA: Operator found! -> " + s);
                    
                    if(i < duplicateString.Length && vals.Last() != "")
                        vals.Add("");
                    ops.Add(s);
                    isOps = true; //char is an operator
                    break; //Match found end loop early
                }
            }

            //Else add char to vals.Last()
            if (!isOps) //If not an operator
            {
                vals[vals.IndexOf(vals.Last())] += duplicateString[i];
            }
            isOps = false; //Reset flag
        }

        if (DEBUG)
        {
            Debug.Log("FA: vals length is " + vals.Count());
            foreach (string s in vals) {
                Debug.Log("FA: vals contains " + s);
            }
        }

        //Prune All Empty strings from vals until non-empty is reached
        while(vals.Last() == "")
        {
            if (DEBUG)
                Debug.Log("FA: Removed empty string!");
            vals.RemoveAt(vals.IndexOf(vals.Last()));
        }

        //Store vals as sideA and sideB
        sideA = float.Parse(vals[0]);
        sideB = float.Parse(vals[1]);

        // 1.) if has 0.5 or 1/2 but no pi, is triangle
        if ((displayString.Contains("0.5") || displayString.Contains("1\u00f72")) && !displayString.Contains("\u03C0"))
            return GameBehaviour.SHAPES.TRIANGLE;

        //Compare if values are equal, square if yes otherwise rect, invalid if there are more than 2 vals
        if (vals.Count() > 1 && vals.Count() < 3)
        {
            if (vals[0].Equals(vals[1]))
            {
                // 2.) if pi is present and same sides, is circle if no 0.5 or 1/2 else semicircle
                if (displayString.Contains("\u03C0"))
                {
                    if (displayString.Contains("0.5") || displayString.Contains("1\u00f72"))
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
        Debug.Log("evalString: " + evalString + " | displayString: " + displayString + " | sideA: " + sideA);
        Debug.Log("isValidFormula: " + isValidFormula + " | answer: " + evalAnswer + " | sideB: " + sideB);
    }
}
