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
    public GameEvent onHideCloseEyesPopUp;
    public GameEvent onHideCalibError;
    public GameEvent onHideTrails;
    public GameEvent onStopReceivingData;
    public BrainData brainData;
    public ConnectionParams connection;

    public void Start()
    {
        onReset.AddListener(ResetSim);
        onSoftReset.AddListener(SoftResetSim);

        onReset.Raise();
    }

    void ResetSim()
    {
        Wenzil.Console.Console.Log($"RESETTING EXPERIENCE");
        brainData.Reset();
        connection.Reset();
        onResetForm.Raise();
        onHideTrails.Raise();
        onHideInstructions.Raise();
        onHideCalibError.Raise();
        onHideCloseEyesPopUp.Raise();
        onShowMainMenu.Raise();
        onStopReceivingData.Raise();
    }

    void SoftResetSim()
    {
        Wenzil.Console.Console.Log($"SOFT RESET EXPERIENCE");
        brainData.Reset();
        onHideTrails.Raise();
        onHideMainMenu.Raise();
        onHideCalibError.Raise();
        onHideCloseEyesPopUp.Raise();
        onShowInstructions.Raise();
        onStopReceivingData.Raise();
    }
}
