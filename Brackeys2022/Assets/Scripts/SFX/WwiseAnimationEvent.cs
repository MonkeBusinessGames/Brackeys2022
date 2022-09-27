using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseAnimationEvent : MonoBehaviour
{
    public void PlayWwiseEvent(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }
}
