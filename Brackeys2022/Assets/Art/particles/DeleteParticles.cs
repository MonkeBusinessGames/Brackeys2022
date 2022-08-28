using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteParticles : MonoBehaviour
{
    public float delay;
    void Start()
    {
        StartCoroutine("DestroyParticles");
    }

    IEnumerator DestroyParticles(){
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
