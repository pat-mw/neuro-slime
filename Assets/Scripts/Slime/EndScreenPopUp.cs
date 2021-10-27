using UnityEngine;
using ScriptableObjectArchitecture;

public class EndScreenPopUp : MonoBehaviour
{
    public GameEvent onEndSimulation;
    public GameEvent onReset;
    public GameEvent onSoftReset;

    Transform endScreenModal;

    private void Awake()
    {
        endScreenModal = transform.GetChild(0);
        onEndSimulation.AddListener(ShowEndScreen);
        onReset.AddListener(HideButton);
        onSoftReset.AddListener(HideButton);
    }

    void ShowEndScreen()
    {
        endScreenModal.gameObject.SetActive(true);
    }

    void HideButton()
    {
        endScreenModal.gameObject.SetActive(false);
    }
}

