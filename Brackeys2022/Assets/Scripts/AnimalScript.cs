using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalScript : MonoBehaviour
{
    [Header("SET ANIMAL ABILITY")]
    [Space(5)]
    [SerializeField] private bool catAbility = false;
    [Space(5)]
    [SerializeField] private bool moleAbility = false;
    [Space(5)]
    [SerializeField] private bool birbAbility = false;

    [SerializeField] private AK.Wwise.Event BirdSound;
    [SerializeField] private AK.Wwise.Event CatSound;
    [SerializeField] private AK.Wwise.Event MoleSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (catAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireCatAbilities();
                AkSoundEngine.PostEvent(CatSound.Id, this.gameObject);
            }
            else if (moleAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireMoleAbilities();
                AkSoundEngine.PostEvent(MoleSound.Id, this.gameObject);
            }
            else if (birbAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireBirbAbilities();
                AkSoundEngine.PostEvent(BirdSound.Id, this.gameObject);
            }

            //Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }

    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// Makes sure, that only one ability is selected per animal
    /// </summary>
    private void OnValidate()
    {
        if (catAbility && (moleAbility || birbAbility))
        {
            moleAbility = false;
            birbAbility = false;
        }
        if (moleAbility && (catAbility || birbAbility))
        {
            birbAbility = false;
            catAbility = false;
        }
        if (birbAbility && (moleAbility || birbAbility))
        {
            moleAbility = false;
            catAbility = false;
        }
    }
#endif
    #endregion
}
