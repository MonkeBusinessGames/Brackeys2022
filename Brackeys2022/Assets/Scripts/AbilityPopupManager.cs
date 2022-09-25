using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPopupManager : MonoBehaviour
{
    [SerializeField] private Sprite catAbilityPopup;
    [SerializeField] private Sprite moleAbilityPopup;
    [SerializeField] private Sprite birbAbilityPopup;
    [SerializeField] private Sprite goatAbilityPopup;
    [SerializeField] private Sprite monkeyAbilityPopup;
    [SerializeField] private Sprite doubleJumpAcquired;
    [SerializeField] private Sprite tryToHideWithX;

    private Animator popupAnimator;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        popupAnimator = GetComponent<Animator>();

        popupAnimator.enabled = false;
    }
    private void OnEnable()
    {
        AnimalScript.OnAnimalAcquired += AnimalScript_OnAnimalAcquired;
    }
    private void OnDestroy()
    {
        AnimalScript.OnAnimalAcquired -= AnimalScript_OnAnimalAcquired;
    }
    private void Update()
    {
        if (popupAnimator.enabled)
        {
            CheckPopupDirection();
        }
    }
    public void ShowPopup(Popup popup)
    {
        switch (popup)
        {
            case Popup.CAT_ABILITY_ACQUIRED:
                spriteRenderer.sprite = catAbilityPopup;
                break;
            case Popup.MOLE_ABILITY_ACQUIRED:
                spriteRenderer.sprite = moleAbilityPopup;
                break;
            case Popup.BIRB_ABILITY_ACQUIRED:
                spriteRenderer.sprite = birbAbilityPopup;
                break;
            //case Popup.GOAT_ABILITY_ACQUIRED:
            //    spriteRenderer.sprite = goatAbilityPopup;
            //    break;
            //case Popup.MONKEY_ABILITY_ACQUIRED:
            //    spriteRenderer.sprite = monkeyAbilityPopup;
            //    break;
            case Popup.DOUBLE_JUMP_ACQUIRED:
                spriteRenderer.sprite = doubleJumpAcquired;
                break;
            case Popup.TRY_TO_HIDE_WITH_X:
                spriteRenderer.sprite = tryToHideWithX;
                break;
        }
        popupAnimator.enabled = true;
    }

    private void CheckPopupDirection()
    {
        if(transform.parent.transform.localScale.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
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
    GOAT_ABILITY_ACQUIRED,
    MONKEY_ABILITY_ACQUIRED,
    DOUBLE_JUMP_ACQUIRED,
    TRY_TO_HIDE_WITH_X
}