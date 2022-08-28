using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPopupManager : MonoBehaviour
{
    [SerializeField] private Sprite catAbilityPopup;
    [SerializeField] private Sprite moleAbilityPopup;
    [SerializeField] private Sprite birbAbilityPopup;

    private Animator popupAnimator;

    private void Awake()
    {
        popupAnimator = GetComponent<Animator>();
    }


}
