using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonSFX : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event attackSound;
    [SerializeField] private AK.Wwise.Event footstepSound;
    [SerializeField] private AK.Wwise.Event deathSound;
    [SerializeField] private AK.Wwise.Event gethitSound;
    [SerializeField] private AK.Wwise.Event skeletonAngeringSound;
    
    public void PlayFootstepSound()
    {
        AkSoundEngine.PostEvent(footstepSound.Id, this.gameObject);
    }

    public void PlayAttackSound()
    {
        AkSoundEngine.PostEvent(attackSound.Id, this.gameObject);
    }

    public void PlayDeathSound()
    {
        AkSoundEngine.PostEvent(deathSound.Id, this.gameObject);
    }

    public void PlayGethitSound()
    {
         AkSoundEngine.PostEvent(deathSound.Id, this.gameObject);
    } 
    public void PlaySkeletonAngeringSound()
    {
         AkSoundEngine.PostEvent(skeletonAngeringSound.Id, this.gameObject);
    }

}
