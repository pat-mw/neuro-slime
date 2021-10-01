using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class FrameRateControl : MonoBehaviour
{
    [SerializeField] [Range(1, 60)] [OnValueChanged("FrameRateChanged")] int frameRate = 30;

    private void Start()
    {
        Application.targetFrameRate = frameRate;
    }

    private void FrameRateChanged()
    {
        Application.targetFrameRate = frameRate;
    }
}
