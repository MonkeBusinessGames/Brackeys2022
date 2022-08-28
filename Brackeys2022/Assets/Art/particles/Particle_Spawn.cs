using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Spawn : MonoBehaviour
{
    public GameObject PS;
    public GameObject Spawner;
    public void SpawnParticles(){
        Instantiate(PS, Spawner.transform.position, Quaternion.identity);
        
    }
}
