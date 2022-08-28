using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonSFX : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event attackSound;
    [SerializeField] private AK.Wwise.Event footstepSound;
    [SerializeField] private AK.Wwise.Event deathSound;
    
    public void PlayFootstepSound()
    {
        //AkSoundEngine.PostEvent(footstepSound.Id, this.gameObject);
    }

    public void PlayAttackSound()
    {
       /// AkSoundEngine.PostEvent(attackSound.Id, this.gameObject);
    }

    public void PlayDeathSound()
    {
       // AkSoundEngine.PostEvent(deathSound.Id, this.gameObject);
    }
    
}
