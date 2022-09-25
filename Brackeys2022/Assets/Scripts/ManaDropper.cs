using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaDropper : MonoBehaviour
{
    [SerializeField] private GameObject manaPrefab;
    [SerializeField] private int manaCount;

    private void OnDestroy()
    {
        for(int i = 0; i < manaCount; i++)
        {
            Instantiate(manaPrefab, (Vector2)transform.position + new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f)), Quaternion.identity); 
        }
    }
}
