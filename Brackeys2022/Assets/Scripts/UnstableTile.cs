using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstableTile : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private bool isLooping = false;

    public IEnumerator UnstableLoop()
    {
        if (!isLooping)
        {
            isLooping = true;
            print("Unstable");

            //More unstable
            anim.SetTrigger("Next");
            yield return new WaitForSeconds(1);

            print("Break");
            //Breaking Animation
            anim.SetTrigger("Next");
            yield return new WaitForSeconds(5);

            print("Reform");
            //Reform
            anim.SetTrigger("Next");
            isLooping = false;
        }
    }

}
