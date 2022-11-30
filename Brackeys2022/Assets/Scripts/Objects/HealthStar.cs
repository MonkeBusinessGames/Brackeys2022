using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthStar : MonoBehaviour
{
    /// <summary>
    /// The index number of the health star
    /// </summary>
    [SerializeField] private int indexNumber;
    [SerializeField] private PlayerController player;
    [SerializeField] private Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        if (player.data.starsAcquired.Contains(indexNumber))
        {
            anim.SetTrigger("Acquired");
        }
    }

    public int Acquire()
    {
        anim.SetTrigger("Acquired");
        return indexNumber;
    }

    /// <summary>
    /// Used to destory the health star after it's been acquired.
    /// </summary>
    public void Destroy()
    {
        Destroy(gameObject);
    }

}
