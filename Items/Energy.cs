using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : Item
{
    public override void UseItem(PlayerController player)
    {
        base.UseItem(player);
        player.bombCount++;
        player.ActivateFloatingText(1, player.floatingBombText);
        Destroy(gameObject);
    }
}
