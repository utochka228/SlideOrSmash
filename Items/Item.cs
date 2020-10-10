using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IItem
{
    void Use(PlayerController player);
}

public class Item : MonoBehaviour, IItem
{
    public new string name;
    public GameObject itemEffect;
    public float spawnChance;

    public float spawningOffsetY = 0.01f;
    public void Use(PlayerController player)
    {
        UseItem(player);
    }

    public virtual void UseItem(PlayerController player)
    {

    }

    public void SpawnEffect()
    {
        GameObject effect = Instantiate(itemEffect);
        effect.transform.position = transform.position;
    }
    public void SpawnEffect(Vector3 pos)
    {
        GameObject effect = Instantiate(itemEffect);
        effect.transform.position = pos;
    }
}
