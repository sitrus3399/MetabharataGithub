using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private bool isDemo;
    [SerializeField] private bool closeOnline;

    [SerializeField] private MainPage mainPage;

    void Start()
    {
        if (closeOnline)
        {
            mainPage.CloseOnline();
        }
    }

    void Update()
    {
        
    }
}
