using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LaserLife());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator LaserLife()
    {
        yield return new WaitForSeconds(3);
        {
            Destroy(gameObject);
        }
    }

}
