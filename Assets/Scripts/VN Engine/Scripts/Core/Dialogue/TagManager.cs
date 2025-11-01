using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class TagManager
{
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {
        {"<playerName>", () => "Player" },
        {"<time>", () => DateTime.Now.ToString("hh:mm tt") },
        {"<playerLevel>", () => "15" },
        {"<input>", () => InputPanel.instance.lastInput },
        {"<tempVal1>", () => "42"}
    };
    private static readonly Regex tagRegex = new Regex("<\\w+>");
    public static string Inject(string text, bool injectTags = true, bool injectVariables = true)
    {

        if (injectTags)
            text = InjectTags(text);

        if (injectVariables)
            text = InjectVariables(text);


        return text;
    }

    private static string InjectTags(string value)
    {
        if (tagRegex.IsMatch(value))
        {
            foreach (Match match in tagRegex.Matches(value))
            {
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    value = value.Replace(match.Value, tagValueRequest());
                }
            }
        }
        return value;
    }

    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();

        for (int i = matchesList.Count - 1; i >= 0; i--)
        {
            var match = matchesList[i];
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID);

            if (!VariableStore.TryGetValue(variableName, out object variableValue))
            {
                Debug.LogError($"[TagManager.InjectVariables] Variable '{variableName}' does not exist.");
                continue;
            }

            int lengthToBeRemoved = match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;

            value = value.Remove(match.Index, lengthToBeRemoved);
            value = value.Insert(match.Index, variableValue.ToString());
        }

        return value;
    }
}
