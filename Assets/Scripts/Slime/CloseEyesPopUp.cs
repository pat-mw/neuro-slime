using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Cysharp.Threading.Tasks;
using RetinaNetworking.Server;

public class CloseEyesPopUp : MonoBehaviour
{
    public GameEvent onCloseEyes;
    public GameEvent onHideCloseEyesPopUp;
    public GameEvent onStartStimuli;
    public ConnectionParams connectionParams;

    public float popUpDuration = 3;

    Transform closeEyesModal;

    private void Awake()
    {
        closeEyesModal = transform.GetChild(0);
        onCloseEyes.AddListener(CloseEyes);
        onHideCloseEyesPopUp.AddListener(HidePopUp);
    }


    async void CloseEyes()
    {
        closeEyesModal.gameObject.SetActive(true);
        await CloseEyesDelay(popUpDuration);

        await UniTask.WaitUntil(() => connectionParams.CalibrationReceived() == true);

        onStartStimuli.Raise();
        closeEyesModal.gameObject.SetActive(false);
    }

    void HidePopUp()
    {
        closeEyesModal.gameObject.SetActive(false);
    }

    async UniTask CloseEyesDelay(float seconds)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
        return;
    }

}
