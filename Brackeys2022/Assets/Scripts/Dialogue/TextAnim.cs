using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAnim : MonoBehaviour
{
    private Animator anim;

	private void Start()
	{
		anim = gameObject.GetComponent<Animator>();
    }

	private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            OnEnter();
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            OnExit();
        }
    }

	public void OnEnter()
	{
            anim.ResetTrigger("Dissapear");
            anim.SetTrigger("Appear");
	}

    public void OnExit()
    {
        anim.ResetTrigger("Appear");
        anim.SetTrigger("Dissapear");
    }
}
