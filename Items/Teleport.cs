using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : Item
{
    private Vector3 telPos = -Vector3.one;
    private bool pointIsNotFound = true;
    public override void UseItem(PlayerController player)
    {
        base.UseItem(player);

        StartCoroutine(RandomizeTelPos(player));

        
    }

    IEnumerator RandomizeTelPos(PlayerController player)
    {
        pointIsNotFound = true;
        while (pointIsNotFound == true)
        {
            int x = Random.Range(0, (int)GameManager.GM.MapSize.x);
            int y = Random.Range(0, (int)GameManager.GM.MapSize.y);
            telPos = new Vector3(x, player.transform.position.y, y);
            Debug.Log("Telpos: " + telPos);

            if (GameManager.GM.CheckIsCellAvaiallable(new Vector2(telPos.x, telPos.z)))
            {
                pointIsNotFound = false;
            }
            yield return null;
        }
        //player.transform.position = telPos;
        player.SetStopPoint(new Vector2(telPos.x, telPos.z));
        player.disableInput = true;
        Destroy(gameObject);
    }
}

