using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject creditsMenu;

    public void Play()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void toMainMenu()
    {
        mainMenu.SetActive(true);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void toControls()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }

    public void toCredits()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
