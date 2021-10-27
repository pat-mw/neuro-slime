using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-15)]
public class ConsoleAwake : MonoBehaviour
{

    [TextArea(3, 50)] public string textToDisplay;

    private void Start()
    {
        Wenzil.Console.Console.Log(textToDisplay);
    }
}
