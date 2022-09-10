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
    [SerializeField] private Transform tryToHideWithX;

    private Animator popupAnimator;

    private SpriteRenderer spriteRenderer;
    private Sprite popupSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        popupAnimator = GetComponent<Animator>();

        popupAnimator.enabled = false;
    }
    private void Start()
    {
        AnimalScript.OnAnimalAcquired += AnimalScript_OnAnimalAcquired;
    }

    public void ShowPopup(Popup popup)
    {
        switch (popup)
        {
            case Popup.CAT_ABILITY_ACQUIRED:
                popupSprite = catAbilityPopup.GetComponent<SpriteRenderer>().sprite;
                spriteRenderer.sprite = popupSprite;
                break;
            case Popup.MOLE_ABILITY_ACQUIRED:
                popupSprite = moleAbilityPopup.GetComponent<SpriteRenderer>().sprite;
                spriteRenderer.sprite = popupSprite;
                break;
            case Popup.BIRB_ABILITY_ACQUIRED:
                popupSprite = birbAbilityPopup.GetComponent<SpriteRenderer>().sprite;
                spriteRenderer.sprite = popupSprite;
                break;
            case Popup.DOUBLE_JUMP_ACQUIRED:
                popupSprite = doubleJumpAcquired.GetComponent<SpriteRenderer>().sprite;
                spriteRenderer.sprite = popupSprite;
                break;
            case Popup.TRY_TO_HIDE_WITH_X:
                popupSprite = tryToHideWithX.GetComponent<SpriteRenderer>().sprite;
                spriteRenderer.sprite = popupSprite;
                break;
        }
        popupAnimator.enabled = true;
    }

    private void AnimalScript_OnAnimalAcquired(Popup popup)
    {
        ShowPopup(popup);
    }

    public void ResetAnimator()
    {
        popupAnimator.SetTrigger("Reset");
        spriteRenderer.sprite = null;
        popupAnimator.enabled = false;
    }
}

public enum Popup
{
    CAT_ABILITY_ACQUIRED,
    MOLE_ABILITY_ACQUIRED,
    BIRB_ABILITY_ACQUIRED,
    DOUBLE_JUMP_ACQUIRED,
    TRY_TO_HIDE_WITH_X
}