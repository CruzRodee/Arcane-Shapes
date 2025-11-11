using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelDataModel
{
    public int level;
    public bool isLowOrder;
    public GameBehaviour.SHAPES loSelectedShape;
    public int numChooseShape;
    public int numWrongShape;
    public int measureLinesDrawn;
    public int numUndoUsed;
    public int numOCRInput;
    public int numCalcUsed;
    public int numInvalidFormula;
    public int numMismatchCalc;
    public bool isWin; //Note, quitting counts as loss
    public float error;

    private string temp;

    public GameLevelDataModel()
    {
        // Grab these from parameters or GlobalVariables
        level = GlobalVariables.level;
        isLowOrder = GlobalVariables.enteringLO;
        loSelectedShape = GlobalVariables.loSelectedShape;

        // Set default values
        numChooseShape = 0;
        numWrongShape = 0;
        measureLinesDrawn = 0;
        numUndoUsed = 0;
        numOCRInput = 0;
        numCalcUsed = 0;
        numInvalidFormula = 0;
        numMismatchCalc = 0;
        isWin = false;
        error = 100;
        temp = isLowOrder ? "LO" : "HO";

        // Create starting checkpoint
        PlayerDataManager.instance.SaveCheckpoint($"{temp}_Level_Start_ID_{GlobalVariables.sessionGameId}");
    }

    public void GameEndDataEntry(bool win, GameBehaviour main = null, HOGameScript hoMain = null) 
    {
        if(main != null)
        {
            isWin = win;
            error = main.error;
        }
        else if (hoMain != null)
        {
            isWin = win;
            error = hoMain.error;
        }
        else
        {
            Debug.LogError("Cannot Find Script for current game level");
            return;
        }

        Dictionary<string, object> currentLevelData = new Dictionary<string, object>();

        currentLevelData.Add("level", level);
        currentLevelData.Add("isLowOrder", isLowOrder);
        currentLevelData.Add("loSelectedShape", loSelectedShape);
        currentLevelData.Add("numChooseShape", numChooseShape);
        currentLevelData.Add("numWrongShape", numWrongShape);
        currentLevelData.Add("measureLinesDrawn", measureLinesDrawn);
        currentLevelData.Add("numUndoUsed", numUndoUsed);
        currentLevelData.Add("numOCRInput", numOCRInput);
        currentLevelData.Add("numCalcUsed", numCalcUsed);
        currentLevelData.Add("numInvalidFormula", numInvalidFormula);
        currentLevelData.Add("numMismatchCalc", numMismatchCalc);
        currentLevelData.Add("isWin", isWin);
        currentLevelData.Add("error", error);

        PlayerDataManager.instance.SaveCheckpoint($"{temp}_Level_End_ID_{GlobalVariables.sessionGameId}", currentLevelData);

        GlobalVariables.sessionGameId++;
    }
}
