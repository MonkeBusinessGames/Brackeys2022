using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaOrb : MonoBehaviour
{
    /// <summary>
    /// The index number of the mana orb
    /// </summary>
    [SerializeField] private int indexNumber;
    [SerializeField] private PlayerController player;
    [SerializeField] private Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        if (player.data.orbsAcquired.Contains(indexNumber))
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
    /// Used to destory the mana orb after it's been acquired.
    /// </summary>
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
