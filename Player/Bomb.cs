using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float timeToBoom = 3f;
    public GameObject itemEffect;
    List<IDamageble> targetsInRadius = new List<IDamageble>();

    Collider[] inSphere;

    public GameObject owner;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, Mathf.RoundToInt(transform.position.z));
        inSphere = Physics.OverlapSphere(transform.position, 1.2f, 1 << 8);
        foreach (var obj in inSphere)
        {
            ChangeMaterial change = obj.GetComponent<ChangeMaterial>();
            change.ChangeToNewMat();
        }
        StartCoroutine(Explode());
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageble pl = other.GetComponent<IDamageble>();
        if (pl != null)
        {
            if(!targetsInRadius.Contains(pl))
                targetsInRadius.Add(pl);
        }
    }
    void OnTriggerExit(Collider other)
    {
        IDamageble pl = other.GetComponent<IDamageble>();
        if (pl != null)
        {
            targetsInRadius.Remove(pl);
        }
    }

    IEnumerator Explode()
    {
        GetComponent<TraumaInducer>().Delay = timeToBoom;

        yield return new WaitForSeconds(timeToBoom);
        foreach (var target in targetsInRadius)
        {
            target.TakeDamage(owner);
            Debug.Log("TAKEN DAMAGE");
        }

        foreach (var obj in inSphere)
        {
            ChangeMaterial change = obj.GetComponent<ChangeMaterial>();
            change.ChangeToOldMat();
        }

        Destroy(gameObject);
    }
}
