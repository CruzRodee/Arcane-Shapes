using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Expressions;
using System;

namespace DIALOGUE.LogicalLines
{
    public class LL_Operator : ILogicalLine
    {
        public string keyword => throw new System.NotImplementedException();

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string trimmedLine = line.rawData.Trim();
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMETIC);

            if (parts.Length < 3)
            {
                Debug.LogError($"[LL_Operator] Invalid operator line: {trimmedLine}");
                yield break;
            }

            string variable = parts[0].Trim().TrimStart(VariableStore.VARIABLE_ID);
            string operatorSymbol = parts[1].Trim();
            string[] remainingParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainingParts, 0, parts.Length - 2);

            object value = CalculateValue(remainingParts);

            if (value == null)
                yield break;

            ProcessOperator(variable, operatorSymbol, value);
        }

        private void ProcessOperator(string variable, string operatorSymbol, object value)
        {
            if (VariableStore.TryGetValue(variable, out object currentValue))
            {
                ProcessOperatorOnVariable(variable, operatorSymbol, value, currentValue);
            }
            else if (operatorSymbol == "=")
            {
                VariableStore.CreateVariable(variable, value);
            }
            else
            {
                Debug.LogError($"[LL_Operator] Variable '{variable}' does not exist for operator '{operatorSymbol}'.");
            }
        }

        private void ProcessOperatorOnVariable(string variable, string operatorSymbol, object value, object currentValue)
        {
            switch (operatorSymbol)
            {
                case "=":
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=":
                    VariableStore.TrySetValue(variable, ConcatenateOrAdd(value, currentValue));
                    break;
                case "-=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) / Convert.ToDouble(value));
                    break;
                default:
                    Debug.LogError($"[LL_Operator] Unsupported operator '{operatorSymbol}' in line.");
                    break;
            }
        }

        private object ConcatenateOrAdd(object value, object currentValue)
        {
            if (value is string)
                return currentValue.ToString() + value;

            return Convert.ToDouble(value) + Convert.ToDouble(currentValue);
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.rawData.Trim(), REGEX_OPERATOR_LINE);

            return match.Success;
        }
    }
}