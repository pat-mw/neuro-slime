using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;

public class MenuScreenPopUp : MonoBehaviour
{
    public GameEvent onShowMainMenu;
    Transform mainMenuModal;

    private void Awake()
    {
        mainMenuModal = transform.GetChild(0);
        onShowMainMenu.AddListener(ShowMenu);
    }

    void ShowMenu()
    {
        mainMenuModal.gameObject.SetActive(true);
    }
}

