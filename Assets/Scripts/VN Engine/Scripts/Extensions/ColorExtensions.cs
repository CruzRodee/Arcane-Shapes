using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
    public static Color SetAlpha(this Color original, float alpha)
    {
        return new Color(original.r, original.g, original.b, alpha);
    }

    public static Color GetColorFromName(this Color original, string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "black":
                return Color.black;
            case "white":
                return Color.white;
            case "yellow":
                return Color.yellow;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "gray":
            case "grey":
                return Color.gray;
            case "clear":
                return Color.clear;
            case "orange":
                return new Color(1f, 0.5f, 0f); // orange is not a predefined color so we create it manually
            default:
                Debug.LogWarning($"[ColorExtensions] GetColorFromName: Color name '{colorName}' not recognized. Returning original color.");
                return original;
        }
    }
}
