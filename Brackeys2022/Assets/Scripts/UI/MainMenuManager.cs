using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private AK.Wwise.Event UISelectSound;
    [SerializeField] private CanvasGroup[] cutScenes;
    [SerializeField] private TMP_Dropdown localeDropdown;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private List<TMP_Text> keyInputs;
    private Key currentKey;
    private bool isWaiting;
    [SerializeField] private GameObject inputScreen;

    [SerializeField] private bool startFresh = false;
    private SaveData data;

    private void Awake()
    {
        if (startFresh)
            data = new SaveData();
        else
            data = SaveSystem.Load();

    }

    private void Start()
    {
        //Localization Initialization
        StartCoroutine(InitializeLocales());

        //Volume Initialization
        InitializeVolume();

        //Controls Initialization
        InitializeControls();
    }

    #region"Button Methods"
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
    #endregion

    #region"Localization"
    IEnumerator InitializeLocales()
    {
        yield return LocalizationSettings.InitializationOperation;
        localeDropdown.ClearOptions();
        List<string> localeLabels = new List<string>();
        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            localeLabels.Add(locale.LocaleName);
        }
        localeDropdown.AddOptions(localeLabels);
        localeDropdown.value = data.languageIndex;
        StartCoroutine(SetLocale(data.languageIndex));
    }

    public void ChangeLocale(int i)
    {
        data.languageIndex = i;
        SaveSystem.Save(data);
        StartCoroutine(SetLocale(i));
    }


    IEnumerator SetLocale(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
    }
    #endregion

    #region"Volume"
    private void InitializeVolume()
    {
        //Sets the slider values
        musicSlider.value = data.musicVolume;
        sfxSlider.value = data.sfxVolume;

        //Sets Music Volume
        //AkSoundEngine.SetRIPCValue("MusicVolume", data.musicVolume);

        //Sets SFX Volume
        //AkSoundEngine.SetRIPCValue("SFXVolume", data.sfxVolume);
    }

    public void ChangeMusicVolume()
    {
        //Sets Music Volume to Slider Value
        //AkSoundEngine.SetRIPCValue("MusicVolume", musicSlider.value);

        //Saves Music Volume for future sessions
        data.musicVolume = musicSlider.value;
        SaveSystem.Save(data);
    }

    public void ChangeSFXVolume()
    {        
        //Sets SFX Volume to Slider Value
        //AkSoundEngine.SetRIPCValue("SFXVolume", sfxSlider.value);

        //Saves Music Volume for future sessions
        data.sfxVolume = sfxSlider.value;
        SaveSystem.Save(data);
    }
    #endregion

    #region"Input Management"
    private void InitializeControls()
    {
        for(int i = 0; i < keyInputs.Count; i++)
        {
            keyInputs[i].text = data.keyManager.keys[(Key)i].ToString();
        }
    }

    public void StartKeyInput(int i)
    {
        currentKey = (Key) i;
        inputScreen.SetActive(true);
        keyInputs[i].fontStyle = FontStyles.Italic;
        isWaiting = true;
    }

    private void OnGUI()
    {
        if (isWaiting)
        {
            Event keyEvent = Event.current;

            if (keyEvent.isKey)
            {
                data.keyManager.keys[currentKey] = keyEvent.keyCode;
                keyInputs[(int)currentKey].text = data.keyManager.keys[currentKey].ToString();
                keyInputs[(int)currentKey].fontStyle = FontStyles.Bold;
                SaveSystem.Save(data);
                inputScreen.SetActive(false);
                isWaiting = false;
            }
        }
    }

    #endregion

}
