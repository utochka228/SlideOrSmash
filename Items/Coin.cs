using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Item
{
    public override void UseItem(PlayerController player)
    {
        base.UseItem(player);
        GameTypeManager.GTM.moneyTrophy++;
        Destroy(gameObject);
    }
}
