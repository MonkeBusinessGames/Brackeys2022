using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicSFXVolume : MonoBehaviour
{
    public Slider thisSlider;
    public float musicVolume;

    void Update()
    { 
        float sliderValue = thisSlider.value;

        sliderValue = musicVolume;
        AkSoundEngine.SetRTPCValue("MusicVolume", sliderValue);
    }
}
