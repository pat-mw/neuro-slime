using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;
using RetinaNetworking.Server;

public class CalibrationErrorPopUp : MonoBehaviour
{
    public GameEvent onCalibrationError;
    public GameEvent onHideCalibError;

    Transform popUp;

    private void Awake()
    {
        popUp = transform.GetChild(0);
        onCalibrationError.AddListener(CalibError);
        onHideCalibError.AddListener(HideError);
    }

    void CalibError()
    {
        popUp.gameObject.SetActive(true);
    }

    void HideError()
    {
        popUp.gameObject.SetActive(false);
    }

}
