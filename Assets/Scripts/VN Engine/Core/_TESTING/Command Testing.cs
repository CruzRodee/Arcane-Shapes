using System.Collections;
using COMMANDS;
using UnityEngine;

public class CommandTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    private Vector2 touchStartPos;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    Vector2 swipeDir = touch.position - touchStartPos;
                    if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
                    {
                        if (swipeDir.x < 0)
                            CommandManager.instance.Execute("moveCharDemo", "left");
                        else
                            CommandManager.instance.Execute("moveCharDemo", "right");
                    }
                    break;
            }

        }

        // if (Input.GetKeyDown(KeyCode.LeftArrow))
        //     CommandManager.instance.Execute("moveCharDemo", "left");
        // else if (Input.GetKeyDown(KeyCode.RightArrow))
        //     CommandManager.instance.Execute("moveCharDemo", "right");
    }

    IEnumerator Running()
    {
        // Testing command executions
        yield return CommandManager.instance.Execute("print");
        yield return CommandManager.instance.Execute("print_1p", "Hello, World!");
        yield return CommandManager.instance.Execute("print_mp", "Line one.", "Line two.", "Line three.");

        // Testing lambda command executions
        yield return CommandManager.instance.Execute("lambda");
        yield return CommandManager.instance.Execute("lambda_1p", "Hello, Lambda!");
        yield return CommandManager.instance.Execute("lambda_mp", "Lambda line one.", "Lambda line two.", "Lambda line three.");

        // Testing coroutine command executions
        yield return CommandManager.instance.Execute("process");
        yield return CommandManager.instance.Execute("process_1p", "5");
        yield return CommandManager.instance.Execute("process_mp", "First line.", "Second line.", "Third line.");
    }
}
