using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public class CommandParameters
    {

        private const char PARAMETER_IDENTIFIER = '-';
        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private List<string> unlabeledParameters = new List<string>();

        public CommandParameters(string[] parameterArray)
        {
            for (int i = 0; i < parameterArray.Length; i++)
            {
                parameters[$"param{i}"] = parameterArray[i];
                Debug.Log($"CommandParameters: Added {parameters[$"param{i}"]} with key param{i}");

                if (parameterArray[i].StartsWith(PARAMETER_IDENTIFIER) && !float.TryParse(parameterArray[i], out _))
                {
                    string pName = parameterArray[i];
                    string pValue = "";

                    if (i + 1 < parameterArray.Length && !parameterArray[i + 1].StartsWith(PARAMETER_IDENTIFIER))
                    {
                        Debug.Log($"CommandParameters: Found value {parameterArray[i + 1]} for parameter {pName}");
                        pValue = parameterArray[i + 1];
                        i++; // Skip the next element since it's a value
                    }
                    parameters.Add(pName, pValue);

                    Debug.Log($"CommandParameters: Added {pValue} with key {pName}");
                }
                else
                {
                    unlabeledParameters.Add(parameterArray[i]);
                    Debug.Log($"CommandParameters: Added unlabeled parameter {parameterArray[i]}");
                }
            }
        }

        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);

        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            foreach (string parameterName in parameterNames)
            {
                if (parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if (TryCastParameter<T>(parameterValue, out value))
                    {
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"CommandParameters: Failed to cast parameter {parameterName} with value {parameterValue} to type {typeof(T)}. Using default value.");
                        value = defaultValue;
                        return false;
                    }
                }
            }
            // if we reach here, the parameter was not found so search the unlabeled ones if present
            foreach (string parameterName in unlabeledParameters)
            {

                if (TryCastParameter<T>(parameterName, out value))
                {
                    unlabeledParameters.Remove(parameterName);
                    return true;
                }

            }

            value = defaultValue;
            return false;
        }

        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
