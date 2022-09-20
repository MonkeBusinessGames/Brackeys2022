using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalScript : MonoBehaviour
{
    [SerializeField] private GameObject checkPoint;

    [Header("SET ANIMAL ABILITY")]
    [Space(5)]
    [SerializeField] private bool catAbility = false;
    [Space(5)]
    [SerializeField] private bool moleAbility = false;
    [Space(5)]
    [SerializeField] private bool birbAbility = false;
    [Space(5)]
    [SerializeField] private bool goatAbility = false;
    [Space(5)]
    [SerializeField] private bool monkeyAbility = false;

    public static event Action<Popup> OnAnimalAcquired;
    Popup popup;

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
                popup = Popup.CAT_ABILITY_ACQUIRED;
                AkSoundEngine.PostEvent(CatSound.Id, this.gameObject);
            }
            else if (moleAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireMoleAbilities();
                popup = Popup.MOLE_ABILITY_ACQUIRED;
                AkSoundEngine.PostEvent(MoleSound.Id, this.gameObject);
            }
            else if (birbAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireBirbAbilities();
                popup = Popup.BIRB_ABILITY_ACQUIRED;
                AkSoundEngine.PostEvent(BirdSound.Id, this.gameObject);
            }
            else if (goatAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireGoatAbilities();
                popup = Popup.GOAT_ABILITY_ACQUIRED;
                //AkSoundEngine.PostEvent(GoatSound.Id, this.gameObject);
            }
            else if (monkeyAbility)
            {
                collision.gameObject.GetComponent<PlayerController>().AcquireMonkeyAbilities();
                popup = Popup.MONKEY_ABILITY_ACQUIRED;
                //AkSoundEngine.PostEvent(MonkeySound.Id, this.gameObject);
            }

            OnAnimalAcquired?.Invoke(popup);
            GetComponent<Animator>().SetTrigger("Disappear");
        }
    }

    public void CreateCheckPoint()
    {
        checkPoint.SetActive(true);
        Destroy(gameObject);
    }

    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// Makes sure, that only one ability is selected per animal
    /// </summary>
    private void OnValidate()
    {
        if (catAbility)
        {
            moleAbility = false;
            birbAbility = false;
            goatAbility = false;
            monkeyAbility = false;
        }
        if (moleAbility)
        {
            birbAbility = false;
            catAbility = false;
            goatAbility = false;
            monkeyAbility = false;
        }
        if (birbAbility)
        {
            moleAbility = false;
            catAbility = false;
            goatAbility = false;
            monkeyAbility = false;
        }
        if (goatAbility)
        {
            moleAbility = false;
            catAbility = false;
            birbAbility = false;
            monkeyAbility = false;
        }
        if (monkeyAbility)
        {
            moleAbility = false;
            catAbility = false;
            birbAbility = false;
            goatAbility = false;
        }
    }
#endif
    #endregion
}
