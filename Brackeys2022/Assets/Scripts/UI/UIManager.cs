using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameCompleteMenu;
    [SerializeField] private Animator[] healthStars;
    [SerializeField] private RectTransform manaBar;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private CanvasGroup[] cutScenes;
    private bool isPaused = false;
    public KeyCode pauseKey;


    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;

    }

    // Update is called once per frame
    void Update()
    {
        //Pause Button Input
        if (Input.GetKeyDown(pauseKey))
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

    public void RemoveHealth(int healthRemaining)
    {
        healthStars[healthRemaining].SetBool("Broken", true);
    }

    public int RecoverHealth()
    {
        int health = 0;
        for(int i = 0; i < healthStars.Length; i++)
        {
            healthStars[i].SetBool("Broken", false);    
            if (healthStars[i].isActiveAndEnabled)
                health++;
        }

        return health;
    }

    public int AddHealthStar(int starCount)
    {
        print("Star added!");

        for (int i = 0; i < starCount; i++)
        {
            healthStars[i].gameObject.SetActive(true);
        }
        return RecoverHealth();
    }
    public float RemoveMana(float manaDrained)
    {
        manaSlider.value -= manaDrained; 
        if (manaSlider.value < 0)
        {
            manaSlider.value = 0;
        }

        return manaSlider.value;

    }

    public float RecoverMana(float manaGained)
    {
        manaSlider.value += manaGained;
        if(manaSlider.value > manaSlider.maxValue)
        {
            manaSlider.value = manaSlider.maxValue;
        }
        return manaSlider.value;
    }

    public float IncreaseManaLimit(float manaIncrease)
    {

        manaSlider.maxValue += manaIncrease;
        manaBar.sizeDelta = new Vector2(manaBar.sizeDelta.x + (10 * manaIncrease), manaBar.sizeDelta.y);
        RecoverMana(manaIncrease);
        return manaSlider.value;
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
