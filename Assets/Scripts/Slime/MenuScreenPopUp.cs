using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;

public class MenuScreenPopUp : MonoBehaviour
{
    public GameEvent onShowMainMenu;
    public GameEvent onHideMainMenu;
    Transform mainMenuModal;

    private void Awake()
    {
        mainMenuModal = transform.GetChild(0);
        onShowMainMenu.AddListener(ShowMenu);
        onHideMainMenu.AddListener(HideMenu);
    }

    void ShowMenu()
    {
        mainMenuModal.gameObject.SetActive(true);
    }
    
    void HideMenu()
    {
        mainMenuModal.gameObject.SetActive(false);
    }
}

