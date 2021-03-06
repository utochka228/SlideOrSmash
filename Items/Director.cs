﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : Item
{
    Vector2 direction;
    bool notRandomized = true;
    void Start()
    {
        int y = 0;
        int x = 0;
        x = Random.Range(-1, 2);
        if (x != 0) notRandomized = false;
        while (notRandomized)
        {
            if (x == 0)
            {
                y = Random.Range(-1, 2);
                if (y != 0)
                    notRandomized = false;
            }
        }
        direction = new Vector2(x, y);

        if (x == -1) transform.eulerAngles += new Vector3(0, 0, 180);
        if (y == 1) transform.eulerAngles += new Vector3(0, 0, 90);
        if (y == -1) transform.eulerAngles += new Vector3(0, 0, -90);
    }

    public override void UseItem(PlayerController player)
    {
        base.UseItem(player);
        player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        player.MovePlayerTo(direction, true); //Rotate player
        Destroy(gameObject);
    }
}
