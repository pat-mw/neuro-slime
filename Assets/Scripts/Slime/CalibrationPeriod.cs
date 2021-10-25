using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ScriptableObjectArchitecture;

public class CalibrationPeriod : MonoBehaviour
{
    public GameEvent onStartCalibration;
    public GameEvent onStartInference;
    public GameEvent onStartReceivingBrainData;
    public GameEvent onEndCalibration;
    public GameEvent onSendCalibrationData;
    public GameEvent onCloseEyes;

    private float calibrationDuration = GlobalConfig.CALIBRATION_DURATION;

    private void Awake()
    {
        onStartCalibration.AddListener(StartCalibration);
        onEndCalibration.AddListener(EndCalibration);
    }

    async void StartCalibration()
    {
        onStartReceivingBrainData.Raise();
        onStartInference.Raise();

        await CalibrationDelay(calibrationDuration);

        onEndCalibration.Raise();
    }

    void EndCalibration()
    {
        onSendCalibrationData.Raise();
        onCloseEyes.Raise();
    }

    
    async UniTask CalibrationDelay(float calibrationLength)
    {
        Debug.Log($"Calibration started! Duration: {calibrationLength}s");
        await UniTask.Delay(System.TimeSpan.FromSeconds(calibrationLength), ignoreTimeScale: false);
        Debug.Log($"Calibration finished!");
        return;
    }

}
