using System;
using System.Linq;
using System.Text.RegularExpressions;

public class AreaFormulaParser
{
    public static readonly Regex squareRegex = new Regex(@"^\(*(\d+(?:\.\d+)?)\)*\*\(*\1\)*$", RegexOptions.Compiled);
    public static readonly Regex rectangleRegex = new Regex(@"^\(*(\d+(?:\.\d+)?)\)*\*\(*((?!\1)\d+(?:\.\d+)?)\)*$", RegexOptions.Compiled);
    public static readonly Regex circleRegex = new Regex(@"^(?:\(*(\d+(?:\.\d+)?)\)*\*\(*\1\)*\*\(*(pi|π|3\.1416)\)*|\(*(\d+(?:\.\d+)?)\)*\*\(*(pi|π|3\.1416)\)*\*\(*\3\)*|\(*((pi|π|3\.1416))\)*\*\(*(\d+(?:\.\d+)?)\)*\*\(*\7\)*)$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static float[] ExtractVariables(string formula, GameBehaviour.SHAPES shape)
    {
        string clean = formula.Replace("(", "").Replace(")", "");

        // Square: a * a
        if (shape == GameBehaviour.SHAPES.SQUARE)
        {
            var num = float.Parse(clean.Split('*')[0]);
            return new[] { num };
        }

        // Rectangle: a * b (a ≠ b)
        if (shape == GameBehaviour.SHAPES.RECTANGLE)
        {
            var parts = clean.Split('*');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out float a) &&
                float.TryParse(parts[1], out float b))
            {
                return new[] { a, b };
            }
        }

        // Triangle: various forms (NOTE: BROKEN AND UNUSED, USING SQUARE OR RECT EXTRACTOR INSTEAD)
        if (shape == GameBehaviour.SHAPES.TRIANGLE)
        {
            string[] parts;
            if (clean.StartsWith("0.5*") || clean.StartsWith("1/2*"))
            {
                parts = clean.Split('*');
                if (float.TryParse(parts[1], out float b1) &&
                    float.TryParse(parts[2], out float h1))
                    return new[] { b1, h1 };
            }
            else if ((clean.EndsWith("*0.5") || clean.EndsWith("*1/2")) && clean.Count(c => c == '*') == 2)
            {
                parts = clean.Split('*');
                if (float.TryParse(parts[0], out float b2) &&
                    float.TryParse(parts[1], out float h2))
                    return new[] { b2, h2 };
            }
            else if (clean.Contains("*") && clean.Contains("/"))
            {
                parts = clean.Split(new[] { '*', '/' });
                if (float.TryParse(parts[0], out float b3) &&
                    float.TryParse(parts[1], out float h3))
                    return new[] { b3, h3 };
            }
        }

        // Circle: pi * r^2 or pi * r * r
        if (shape == GameBehaviour.SHAPES.CIRCLE)
        {
            var parts = clean.Split('*');
            foreach (var part in parts)
            {
                if (float.TryParse(part, out float r))
                    return new[] { r };
            }
        }

        // Semi-circle: same as circle but includes /2 or starts with 0.5 (NOTE: BROKEN AND UNUSED, USING CIRCLE EXTRACTOR INSTEAD)
        if (shape == GameBehaviour.SHAPES.SEMI_CIRCLE)
        {
            var parts = clean.Split('*', '/', '^');
            foreach (var part in parts)
            {
                if (float.TryParse(part, out float r))
                    return new[] { r };
            }
        }

        return Array.Empty<float>();
    }
}