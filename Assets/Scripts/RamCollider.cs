using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RamCollider : MonoBehaviour
{
    public Ram Ram;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            Ram.RamEnemiesIntoWall();
        }
    }
}
