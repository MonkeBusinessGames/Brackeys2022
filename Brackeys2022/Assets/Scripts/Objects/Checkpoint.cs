using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    /// <summary>
    /// The index number of the checkpoint
    /// </summary>
    public int indexNumber;
    [SerializeField] private PlayerController player;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject gate;


    // Start is called before the first frame update
    void Start()
    {
        if (player.data.checkPointsUnlocked.Contains(indexNumber))
        {
            anim.SetBool("Checked", true);
        }
    }

    public int Check()
    {
        anim.SetBool("Checked", true);
        return indexNumber;
    }

    public static Vector2 GetCheckPointPosition(int index)
    {
        foreach (Checkpoint point in FindObjectsOfType<Checkpoint>())
        {
            if (point.indexNumber == index)
                return point.transform.position;
        }

        return Vector2.zero;
    }

    public void DestroyGate()
    {
        Destroy(gate);
    }
}
