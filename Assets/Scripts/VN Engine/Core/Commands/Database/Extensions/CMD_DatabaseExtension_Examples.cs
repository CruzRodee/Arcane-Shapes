using System;
using System.Collections;
using UnityEngine;

public class CMD_DatabaseExtension_Examples : CMD_DatabaseExtension
{
    new public static void Extend(CommandDatabase database)
    {
        // Add Action with no parameters
        database.AddCommand("print", new Action(PrintDefaultMessage));
        // Add Action with one parameter
        database.AddCommand("print_1p", new Action<string>(PrintUserMessage));
        // Add Action with multiple parameters
        database.AddCommand("print_mp", new Action<string[]>(PrintLines));

        // Add lambda expression with no parameters
        database.AddCommand("lambda", new Action(() => Debug.Log("This is a lambda command message.")));
        // Add lambda expression with one parameter
        database.AddCommand("lambda_1p", new Action<string>((msg) => Debug.Log($"This is a lambda command message: {msg}.")));
        // Add lambda expression with multiple parameters
        database.AddCommand("lambda_mp", new Action<string[]>((msgs) => Debug.Log(string.Join(", ", msgs))));
        // Alternative multi-parameter lambda that prints each line separately
        // database.AddCommand("lambda_mp", new Action<string[]>((msgs) =>
        // {
        //     int i = 1;
        //     foreach (string msg in msgs)
        //         Debug.Log($"Line {i++}: {msg}");
        // }));

        // Add coroutines with no parameters
        database.AddCommand("process", new Func<IEnumerator>(SimpleProcess));
        // Add coroutines with one parameter
        database.AddCommand("process_1p", new Func<string, IEnumerator>(LineProcess));
        // Add coroutines with multiple parameters
        database.AddCommand("process_mp", new Func<string[], IEnumerator>(MultiLineProcess));

        database.AddCommand("moveCharDemo", new Func<string, IEnumerator>(MoveCharacter));
    }

    private static void PrintDefaultMessage()
    {
        Debug.Log("This is a default command message.");
    }

    private static void PrintUserMessage(string message)
    {
        Debug.Log($"This is a user command message: {message}.");
    }

    private static void PrintLines(string[] lines)
    {
        int i = 1;
        foreach (string line in lines)
            Debug.Log($"Line {i++}: {line}");
    }

    private static IEnumerator SimpleProcess()
    {
        for (int i = 1; i <= 5; i++)
        {
            Debug.Log($"Process Running. . . Step {i}/5");
            yield return new WaitForSeconds(1);
        }
    }

    private static IEnumerator LineProcess(string data)
    {
        if (int.TryParse(data, out int num))
        {
            for (int i = 1; i <= num; i++)
            {
                Debug.Log($"Process Running. . . Step {i}/{num}");
                yield return new WaitForSeconds(1);
            }
        }

    }

    private static IEnumerator MultiLineProcess(string[] data)
    {
        foreach (string line in data)
        {
            Debug.Log($"Processing line: {line}");
            yield return new WaitForSeconds(0.5f);
        }

    }

    private static IEnumerator MoveCharacter(string direction)
    {
        bool left = direction.ToLower() == "left";

        // Get the variables. This would be defined somewhere else
        Transform character = GameObject.Find("Image").transform;
        float moveSpeed = 15f;

        // Determine target position
        float step = 5f; // how far to move each time
        float targetX = character.position.x + (left ? -step : step);

        // Get the current position
        // float currentX = character.position.x;

        // Move character towards target position
        while (Mathf.Abs(character.position.x - targetX) > 0.1f)
        {
            // Debug.Log($"Moving {(left ? "left" : "right")} [{character.position.x:F2} → {targetX:F2}]");
            float newX = Mathf.MoveTowards(character.position.x, targetX, moveSpeed * Time.deltaTime);
            character.position = new Vector3(newX, character.position.y, character.position.z);
            yield return null;
        }
    }
}
