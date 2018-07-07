using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == 12) {
            other.gameObject.GetComponent<RoosterBehaviour>().Trap();
            Invoke("Despawn", 6f);
        }
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
        Invoke("Respawn", 8f);
    }

    private void Respawn()
    {
        gameObject.SetActive(true);
    }
}
