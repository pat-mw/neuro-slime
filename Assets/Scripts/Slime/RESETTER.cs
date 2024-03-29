using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using RetinaNetworking.Server;
using Wenzil.Console;


public class RESETTER : MonoBehaviour
{

    public GameEvent onReset;
    public GameEvent onSoftReset;
    public GameEvent onResetForm;
    public GameEvent onShowMainMenu;
    public GameEvent onHideMainMenu;
    public GameEvent onShowInstructions;
    public GameEvent onHideInstructions;
    public GameEvent onHideTrails;
    public BrainData brainData;
    public ConnectionParams connection;

    public void Awake()
    {
        onReset.AddListener(ResetSim);
        onSoftReset.AddListener(SoftResetSim);
    }

    void ResetSim()
    {
        Wenzil.Console.Console.Log($"RESETTING EXPERIENCE");
        brainData.Reset();
        connection.Reset();
        onResetForm.Raise();
        onHideTrails.Raise();
        onHideInstructions.Raise();
        onShowMainMenu.Raise();
    }

    void SoftResetSim()
    {
        Wenzil.Console.Console.Log($"SOFT RESET EXPERIENCE");
        brainData.Reset();
        onHideTrails.Raise();
        onHideMainMenu.Raise();
        onShowInstructions.Raise();
    }
}
