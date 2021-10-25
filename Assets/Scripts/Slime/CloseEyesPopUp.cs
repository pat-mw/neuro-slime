using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;

public class CloseEyesPopUp : MonoBehaviour
{
    public GameEvent onCloseEyes;
    public GameEvent onStartStimuli;

    public float popUpDuration = 3;

    Transform closeEyesModal;

    private void Awake()
    {
        closeEyesModal = transform.GetChild(0);
        onCloseEyes.AddListener(CloseEyes);
    }


    async void CloseEyes()
    {
        closeEyesModal.gameObject.SetActive(true);
        await CloseEyesDelay(popUpDuration);
        onStartStimuli.Raise();
        closeEyesModal.gameObject.SetActive(false);
    }

    async UniTask CloseEyesDelay(float seconds)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
        return;
    }

}
