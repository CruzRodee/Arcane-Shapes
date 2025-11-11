using System;
using System.Collections.Generic;

public static class GlobalVariables
{
    // LO variables

    public static GameBehaviour.SHAPES loSelectedShape;
    public static float[] loMeasures1 = { 3, 4, 5, 6, 7, 8 };
    public static float[] loMeasures2 = { 3, 3.5f, 4, 4.5f, 5, 5.5f, 6, 6.5f, 7, 7.5f, 8 };
    public static float[] loMeasures3 = { 3, 3.25f, 3.5f, 3.75f, 4, 4.25f, 4.5f, 4.75f, 5, 5.25f, 5.5f, 5.75f,
        6, 6.25f, 6.5f, 6.75f, 7, 7.25f, 7.5f, 7.75f, 8};

    public static float[] loCircleMeasures1 = { 4, 6, 8 };
    public static float[] loCircleMeasures2 = { 3, 4, 5, 6, 7, 8 };
    public static float[] loCircleMeasures3 = { 3, 3.5f, 4, 4.5f, 5, 5.5f, 6, 6.5f, 7, 7.5f, 8 };

    private static readonly int thresholdHO = 2;
    public static readonly int NUM_LO_LEVELS = 3;

    public static float[] GetLOProblemMeasures()
    {
        float[] valArray = new float[2];

        switch (loSelectedShape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                switch (level)
                {
                    case 0:
                    case 1:
                        valArray[0] = 2;
                        valArray[1] = 2;
                        break;
                    case 2:
                        valArray[0] = 7.5f;
                        valArray[1] = 7.5f;
                        break;
                    case 3:
                        valArray[0] = 4.5f;
                        valArray[1] = 4.5f;
                        break;
                }
                break;

            case GameBehaviour.SHAPES.RECTANGLE:
                switch (level)
                {
                    case 0:
                    case 1:
                        valArray[0] = 2;
                        valArray[1] = 6;
                        break;
                    case 2:
                        valArray[0] = 5f;
                        valArray[1] = 2.5f;
                        break;
                    case 3:
                        valArray[0] = 7.5f;
                        valArray[1] = 1.5f;
                        break;
                }
                break;

            case GameBehaviour.SHAPES.TRIANGLE:
                switch (level)
                {
                    case 0:
                    case 1:
                        valArray[0] = 5;
                        valArray[1] = 5;
                        break;
                    case 2:
                        valArray[0] = 3f;
                        valArray[1] = 1.5f;
                        break;
                    case 3:
                        valArray[0] = 1.5f;
                        valArray[1] = 4.5f;
                        break;
                }
                break;

            case GameBehaviour.SHAPES.CIRCLE:
                switch (level)
                {
                    case 0:
                    case 1:
                        valArray[0] = 4;
                        valArray[1] = 4;
                        break;
                    case 2:
                        valArray[0] = 7f;
                        valArray[1] = 7f;
                        break;
                    case 3:
                        valArray[0] = 5f;
                        valArray[1] = 5f;
                        break;
                }
                break;

            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                switch (level)
                {
                    case 0:
                    case 1:
                        valArray[0] = 6;
                        valArray[1] = 6;
                        break;
                    case 2:
                        valArray[0] = 3.0f;
                        valArray[1] = 3.0f;
                        break;
                    case 3:
                        valArray[0] = 7f;
                        valArray[1] = 7f;
                        break;
                }
                break;
        }
        
        return valArray;
    }

    public static bool IsHOUnlocked(GameData save)
    {
        if(save.loPres > 0) //If already completed 1 loop, HO unlocked
            return true;
        if (save.squareLvl >= thresholdHO && save.rectLvl >= thresholdHO &&
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

    // HO variables

    public static List<HOGameBeh.ShapeObject> HOProblem()
    {
        // Define the problem that matches the level
        List<HOGameBeh.ShapeObject> problem = new List<HOGameBeh.ShapeObject>();
        switch (level)
        {
            case 0: //Same problem for level 1 or 0
            case 1: // Define the "house" shape configuration
                //House square walls
                problem.Add(new HOGameBeh.ShapeObject(4, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -1, 0)));
                //House roof, must be offset in y axis by house square side length plus some offset
                problem.Add(new HOGameBeh.ShapeObject(6, 3, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 2, 0)));
                break;
            case 2: //Charged Explosion Spell
                //Cubic mana charge
                problem.Add(new HOGameBeh.ShapeObject(2, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled());
                //Outward triangle arrows
                problem.Add(new HOGameBeh.ShapeObject(2, 2, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 2, 0))); //Up
                problem.Add(new HOGameBeh.ShapeObject(2, 2, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(2, 2, 0)).tilt(90)); //Right
                problem.Add(new HOGameBeh.ShapeObject(2, 2, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 0, 0)).tilt(-90)); //Left
                problem.Add(new HOGameBeh.ShapeObject(2, 2, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(2, 0, 0)).tilt(180)); //Down
                break;
            case 3: //Cubic Barrier
                //Intersection
                problem.Add(new HOGameBeh.ShapeObject(5, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled());
                problem[0].isIntersect = true;
                //Square1
                problem.Add(new HOGameBeh.ShapeObject(6, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(1, 1, 0)));
                problem[1].zOffset = 0.1f; //offset away camera slightly to be selected later
                //Square2
                problem.Add(new HOGameBeh.ShapeObject(6, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SQUARE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 0, 0)));
                problem[2].zOffset = 0.1f; //offset away camera slightly to be selected later
                break;
            case 4: //Holy Halo
                //Inner negative ring
                problem.Add(new HOGameBeh.ShapeObject(6f, HOGameBeh.UNUSED, GameBehaviour.SHAPES.CIRCLE).setIsToBeFilled());
                problem[0].isExcess = true;
                //Outer Positive Ring
                problem.Add(new HOGameBeh.ShapeObject(8f, HOGameBeh.UNUSED, GameBehaviour.SHAPES.CIRCLE).setIsToBeFilled());
                problem[1].zOffset = 0.1f; //offset away camera slightly to be selected last
                break;
            case 5: //Thor Hammer
                //Hammer head
                problem.Add(new HOGameBeh.ShapeObject(6f, 4f, GameBehaviour.SHAPES.RECTANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 3, 0)));
                problem[0].zOffset = 0.2f;
                //Lightning sign
                problem.Add(new HOGameBeh.ShapeObject(2f, 1.5f, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(-1, 3, 0)));
                problem.Add(new HOGameBeh.ShapeObject(2f, 1.5f, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(3, 3, 0)).tilt(180));
                problem[1].isExcess = true;
                problem[2].isExcess = true;
                //Handle
                problem.Add(new HOGameBeh.ShapeObject(2f, 4f, GameBehaviour.SHAPES.RECTANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -1, 0)));
                problem[3].zOffset = 0.4f;
                //Semicircle pommel
                problem.Add(new HOGameBeh.ShapeObject(4, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -2, 0)).tilt(180));
                problem[4].zOffset = 0.2f;
                //Pommel intersect
                problem.Add(new HOGameBeh.ShapeObject(2f, 1f, GameBehaviour.SHAPES.RECTANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -3, 0)));
                problem[5].isIntersect = true;
                break;
            case 6: //Time Stop
                //Outer Dome Semi circle and rect Floot
                problem.Add(new HOGameBeh.ShapeObject(16, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -3, 0)));
                problem[0].zOffset = 0.8f;
                problem.Add(new HOGameBeh.ShapeObject(16, 1, GameBehaviour.SHAPES.RECTANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -4, 0)));
                problem[1].zOffset = 0.8f;
                //Inner Dome void Semi circle
                problem.Add(new HOGameBeh.ShapeObject(14, HOGameBeh.UNUSED, GameBehaviour.SHAPES.SEMI_CIRCLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, -3, 0)));
                problem[2].isExcess = true;
                problem[2].zOffset = 0.6f;
                //Outer Clock wall circle
                problem.Add(new HOGameBeh.ShapeObject(6, HOGameBeh.UNUSED, GameBehaviour.SHAPES.CIRCLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 0, 0)));
                problem[3].zOffset = 0.4f;
                //Inner clock wall void circle
                problem.Add(new HOGameBeh.ShapeObject(5, HOGameBeh.UNUSED, GameBehaviour.SHAPES.CIRCLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 0, 0)));
                problem[4].zOffset = 0.2f;
                problem[4].isExcess = true;
                //Minute hand pointing at 12 rect
                problem.Add(new HOGameBeh.ShapeObject(2f, 3.0f, GameBehaviour.SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new UnityEngine.Vector3(0, 0, 0)));
                break;
        }

        return problem; //Return the problem that is equal to the game level
    }

    //------------------

    // General variables

    public static int level; //Difficulty Level, 0 = not yet played, 1 = whole numbers, 2 = 0.5 , 3 = 0.25
    public static float percent; //Highscore
    public static bool playerWin = false; //If player wins, make true, increase counter to levelup or level or something then set to false
    public static bool gameFinished = false;
    public static bool isLOGame = false;
    public static bool isMute = false;
    public static float defaultBGMVolume = 0.5f;
    public static bool isStartUp = true;
    public static string nextLevel = ""; //Use this to send the next level name to loading
    public static int sessionGameId = 0; //Use this to label the game data in order of play during a session

    public static float introLen = 5f, outroLen = 5f;
    public static bool enteringLO = true;
    public static void GetVideoLens(GameData save)
    {
        if (enteringLO)
        {
            switch (save.totalLOLevel)
            {
                //Case 0 and 1 is default
                //Case 2-5 is default
                case 6:
                    introLen = 5f;
                    outroLen = 5.5f;
                    break;
                //Case 7-9 default
                //Case 10-12 default
                //Case 13-15 default
                default:
                    introLen = 5f;
                    outroLen = 5f;
                    break;
            }
        }
        else 
        {
            switch (level)
            {
                //Level 2-4 default
                case 1:
                    introLen = 5f;
                    outroLen = 5.5f;
                    break;
                case 5:
                    introLen = 5f;
                    outroLen = 6f;
                    break;
                //Level 6 default
                default:
                    introLen = 5f;
                    outroLen = 5f;
                    break;
            }
        }
    }

    //------------------

}
