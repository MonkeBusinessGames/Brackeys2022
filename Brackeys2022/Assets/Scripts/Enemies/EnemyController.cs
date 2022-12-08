using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public virtual void TakeDamage(float damageDealt, Vector2 playerPosition)
    {
    }
}

public enum EnemyState
{
    Idle,
    Walking,
    Chasing,
    Hit,
    Die,
    Attack
}
