using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameCompleteMenu;
    [SerializeField] private Animator healthBar;
    [SerializeField] private CanvasGroup[] cutScenes;
    private bool isPaused = false;


    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Pause Button Input
        if (Input.GetButtonDown("Cancel"))
        {
            Pause();
        }
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(1);
    }

    public void Pause()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;
            pauseMenu.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            isPaused = true;
            pauseMenu.SetActive(true);
        }
    }
    
    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverMenu.SetActive(true);
    }

    public void GameComplete()
    {
        Time.timeScale = 0;
        gameCompleteMenu.SetActive(true);
        StartCoroutine(EndingCutscene());
    }

    public void RemoveHealth()
    {
        healthBar.SetTrigger("Take Damage");
    }

    private IEnumerator EndingCutscene()
    {
        for (int i = 0; i < cutScenes.Length; i++)
        {
            while (cutScenes[i].alpha < 1)
            {
                cutScenes[i].alpha += Time.unscaledDeltaTime / 2;
                yield return null;
            }
        }
    }

}
