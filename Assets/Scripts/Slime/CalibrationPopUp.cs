using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;

public class CalibrationPopUp : MonoBehaviour
{
    public GameEvent onStartCalibration;
    public GameEvent onEndCalibration;
    Transform calibrationModal;

    private void Awake()
    {
        calibrationModal = transform.GetChild(0);
        onStartCalibration.AddListener(ShowCalibrationText);
        onEndCalibration.AddListener(HideCalibrationText);
    }

    void ShowCalibrationText()
    {
        calibrationModal.gameObject.SetActive(true);
    }

    void HideCalibrationText()
    {
        calibrationModal.gameObject.SetActive(false);
    }
}
