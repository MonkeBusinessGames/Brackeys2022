using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonSFX : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event attackSound;
    [SerializeField]
    private AK.Wwise.Event footstepSound;


    public void PlayFootstepSound()
    {
        AkSoundEngine.PostEvent(footstepSound.Id, this.gameObject);
    }

    public void PlayAttackSound()
    {
        AkSoundEngine.PostEvent(attackSound.Id, this.gameObject);
    }
}
