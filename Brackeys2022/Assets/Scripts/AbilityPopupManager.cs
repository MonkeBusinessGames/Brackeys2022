using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPopupManager : MonoBehaviour
{
    [SerializeField] private Transform catAbilityPopup;
    [SerializeField] private Transform moleAbilityPopup;
    [SerializeField] private Transform birbAbilityPopup;
    [SerializeField] private Transform doubleJumpAcquired;

    private Animator popupAnimator;

    private SpriteRenderer transformImage, popupImage;

    private void Awake()
    {
        transformImage = GetComponent<SpriteRenderer>();
        popupAnimator = GetComponent<Animator>();

        popupAnimator.enabled = false;
    }
    private void Start()
    {
        
    }

    public void ShowPopup(Popup popup)
    {
        switch (popup)
        {
            case Popup.CAT_ABILITY_ACQUIRED:
                popupImage = catAbilityPopup.GetComponent<SpriteRenderer>();
                transformImage = popupImage;

                

                break;
            case Popup.MOLE_ABILITY_ACQUIRED:

                break;
            case Popup.BIRB_ABILITY_ACQUIRED:

                break;
            case Popup.DOUBLE_JUMP_ACQUIRED:

                break;
            case Popup.TRY_TO_HIDE_WITH_X:

                break; 
            case Popup.ATTACK_WITH_C:

                break;
        }
        popupAnimator.Play("AbilityPopup");
    }

    public enum Popup
    {
        CAT_ABILITY_ACQUIRED,
        MOLE_ABILITY_ACQUIRED,
        BIRB_ABILITY_ACQUIRED,
        DOUBLE_JUMP_ACQUIRED,
        TRY_TO_HIDE_WITH_X,
        ATTACK_WITH_C
    }
}
