/* 
 
Using OSC Protocol to receive brain data from OpenBCI Cyton

References:
- OSC: https://thomasfredericks.github.io/UnityOSC/
- OpenBCI OSC Ref: https://docs.google.com/document/d/e/2PACX-1vR_4DXPTh1nuiOwWKwIZN3NkGP3kRwpP4Hu6fQmy3jRAOaydOuEI1jket6V4V6PG4yIG15H1N7oFfdV/pub
- OpenBCI Networking: https://docs.openbci.com/Software/OpenBCISoftware/GUIWidgets/#osc
 
 */

using UnityEngine;
using System.Collections;

public class ReceivePulse : MonoBehaviour
{
    [Header("NETWORK")]
    public OSC osc;

    [Header("KEYS")]
    [SerializeField] private string pulseKey = "/pulse";

    [Header("MODE")]
    [SerializeField] private HandlerMode handler = HandlerMode.Custom;

    void Start()
    {
        switch (handler)
        {
            case HandlerMode.Custom:
                osc.SetAllMessageHandler(OnReceiveAnything);
                break;
            case HandlerMode.Default:
                osc.SetAddressHandler(pulseKey, OnReceivePulse);
                break;
            default:
                Debug.LogError($"Handler mode not recognised: {handler}");
                break;
        }
    }


    /// <summary>
    /// Default Callback handler
    /// Is called when any data is received
    /// </summary>
    void OnReceiveAnything(OscMessage message)
    {
        Debug.Log($"Receiving message: {message}");
        string address = message.address;

        address = CleanAddress(address);

        switch (address)
        {
            case "/pulse":
                OnReceivePulse(message);
                break;
            default:
                OnReceiveUnknown(message);
                break;
        }
    }


    /// <summary>
    /// Removes the character (CTRL+S) (Unicode 19) from a given string
    /// Response to the strange behaviour from OpenBCI
    /// </summary>
    string CleanAddress(string address)
    {
        string cleanAddress = "";

        foreach (char ch in address)
        {
            int code = ch;
            if (code != 19)
            {
                cleanAddress += ch;
            }
        }
        return cleanAddress;
    }


    /// <summary>
    /// Three integers in the following order:
    /// BPM, Signal, IBI
    /// </summary>
    void OnReceivePulse(OscMessage message)
    {
        Debug.Log($"Receiving Pulse: {message}");
    }


    /// <summary>
    /// default switch case
    /// Called when the message address is unknown
    /// </summary>
    void OnReceiveUnknown(OscMessage message)
    {
        Debug.Log($"Message was from unrecognised address: {message.address} \n message: {message}");
    }
}