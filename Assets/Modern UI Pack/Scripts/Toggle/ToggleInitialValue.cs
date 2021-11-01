using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleInitialValue : MonoBehaviour
{
    [SerializeField] private BoolReference boolValue;

    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.isOn = boolValue.Value;
    }

}

