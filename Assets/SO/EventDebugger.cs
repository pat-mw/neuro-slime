using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Reflection;

public class EventDebugger : SerializedMonoBehaviour
{
    [Header("IS DEBUGGING")]
    [SerializeField] BoolReference isEventDebugging;

    [Header("EVENTS")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    [System.NonSerialized]
    [OdinSerialize] List<GameEvent> gameEvents;

    void Start()
    {

        int i = 0;
        foreach (GameEvent gameEvent in gameEvents)
        {
            System.Action<string> logGameEvent;
            logGameEvent = delegate (string eventName) { LogEvent(eventName); };
            gameEvent.AddListener(logGameEvent);
            i++;
        }
    }

    void LogEvent(string name)
    {
        if (isEventDebugging.Value == true)
            Wenzil.Console.Console.Log($"EVENT RAISED: {name}");
    }
}
