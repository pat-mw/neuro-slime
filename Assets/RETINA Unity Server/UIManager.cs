using UnityEngine;
using UnityEngine.UI;
using RetinaNetworking.Server;
using Wenzil.Console;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject startMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Console.Log("UI Manager instance already exists - destroying object");
            Destroy(this);
        }
    }

    public void StartServer()
    {
        startMenu.SetActive(false);
        NetworkManager.Instance.StartServer();
    }
}
