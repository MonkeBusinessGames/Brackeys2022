using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageSFX : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event footstepsEvent;
   

    public void PlayFootstepSound()
    {
        AkSoundEngine.PostEvent(footstepsEvent.Id, this.gameObject);
    }
}
