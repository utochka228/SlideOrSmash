using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeWall : MonoBehaviour
{
    public float deathTime = 2f;

    void Start()
    {
        Destroy(gameObject, deathTime);
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageble creature = other.GetComponent<IDamageble>();
        if(creature != null)
        {
            creature.Die(gameObject);
            Dodge.dodge.gamePlayers.Remove(other.gameObject);
        }
    }
}
