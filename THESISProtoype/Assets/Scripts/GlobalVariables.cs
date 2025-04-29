using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameBehaviour;

public static class GlobalVariables
{
    // LO variables

    public static GameBehaviour.SHAPES loSelectedShape;
    public static float[] loMeasures1 = { 3, 4, 5, 6, 7, 8};
    public static float[] loMeasures2 = { 3, 3.5f, 4, 4.5f, 5, 5.5f, 6, 6.5f, 7, 7.5f, 8};
    public static float[] loMeasures3 = { 3, 3.25f, 3.5f, 3.75f, 4, 4.25f, 4.5f, 4.75f, 5, 5.25f, 5.5f, 5.75f, 
        6, 6.25f, 6.5f, 6.75f, 7, 7.25f, 7.5f, 7.75f, 8};

    public static float[] loCircleMeasures1 = { 4, 6, 8 };
    public static float[] loCircleMeasures2 = { 3, 4, 5, 6, 7, 8 };
    public static float[] loCircleMeasures3 = { 3, 3.5f, 4, 4.5f, 5, 5.5f, 6, 6.5f, 7, 7.5f, 8 };

    private static readonly int thresholdHO = 1;
    public static bool IsHOUnlocked(GameData save)
    {
        if(save.squareLvl >= thresholdHO && save.rectLvl >= thresholdHO && 
            save.triLvl >= thresholdHO && save.circleLvl >= thresholdHO && save.scircleLvl >= thresholdHO)
            return true; // If all levels have been played at least once
        else return false; //default answer no
    }

    public static string ShapeFormulaText(GameBehaviour.SHAPES currentShape)
    {
        switch (currentShape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                return "Ang kailangang mana para sa spell: \n\n A: [S] x [S] = [A]";
            case GameBehaviour.SHAPES.TRIANGLE:
                return "Ang kailangang mana para sa spell: \n\n A: 1/2 x [B] x [H] = [A]";
            case GameBehaviour.SHAPES.CIRCLE:
                return "Ang kailangang mana para sa spell: \n\n A: PI x [R] x [R] = [A]";
            case GameBehaviour.SHAPES.RECTANGLE:
                return "Ang kailangang mana para sa spell: \n\n A: [L] x [W] = [A]";
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                return "Ang kailangang mana para sa spell: \n\n A: PI x [R] x [R] x 1/2 = [A]";
            default:
                return "ERROR: INVALID SHAPE - NO FORUMULA TEXT FOUND";
        }
    }

    //------------------

    // General variables

    public static int level; //Difficulty Level, 0 = not yet played, 1 = whole numbers, 2 = 0.5 , 3 = 0.25
    public static float percent; //Highscore
    public static bool playerWin = false; //If player wins, make true, increase counter to levelup or level or something then set to false
    public static bool gameFinished = false;
    public static bool isLOGame = false;

    //------------------
}
