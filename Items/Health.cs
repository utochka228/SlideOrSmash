using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Item
{
    public override void UseItem(PlayerController player)
    {
        base.UseItem(player);
        player.health++;
        player.ActivateFloatingText(1, player.floatingHealthText);
        Destroy(gameObject);
    }
}
