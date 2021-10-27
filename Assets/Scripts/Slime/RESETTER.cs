using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using RetinaNetworking.Server;
using Wenzil.Console;


public class RESETTER : MonoBehaviour
{

    public GameEvent onReset;
    public GameEvent onResetForm;
    public GameEvent onShowMainMenu;
    public GameEvent onHideTrails;
    public BrainData brainData;
    public ConnectionParams connection;

    public void Awake()
    {
        onReset.AddListener(ResetSim);
    }

    void ResetSim()
    {
        Wenzil.Console.Console.Log($"RESETTING EXPERIENCE");
        brainData.Reset();
        connection.Reset();
        onResetForm.Raise();
        onHideTrails.Raise();
        onShowMainMenu.Raise();
    }
}
