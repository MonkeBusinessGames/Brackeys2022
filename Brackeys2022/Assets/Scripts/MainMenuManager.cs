using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private AK.Wwise.Event UISelectSound;
    [SerializeField] private CanvasGroup[] cutScenes;

    public void Play()
    {
        StartCoroutine(OpeningCutscenes());
        AkSoundEngine.PostEvent(UISelectSound.Id, this.gameObject);
    }

    private IEnumerator OpeningCutscenes()
    {
        for(int i = 0; i < cutScenes.Length; i++)
        {
            while (cutScenes[i].alpha < 1)
            {
                cutScenes[i].alpha += Time.deltaTime/2;
                yield return null;
            }
        }

        SceneManager.LoadSceneAsync(1);
    }

    public void toMainMenu()
    {
        AkSoundEngine.PostEvent(UISelectSound.Id, this.gameObject);
        mainMenu.SetActive(true);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void toControls()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(true);
        creditsMenu.SetActive(false);
        AkSoundEngine.PostEvent(UISelectSound.Id, this.gameObject);
    }

    public void toCredits()
    {
        mainMenu.SetActive(false);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(true);
        AkSoundEngine.PostEvent(UISelectSound.Id, this.gameObject);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
