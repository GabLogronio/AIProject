using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoamingBehaviour : GeneralBehaviour {

    override public void ExecuteBehaviour(GameObject target)
    {

    }//--------------------VERSIONE CAMBIA DIREZIONE OGNI TOT SECONDI

    public void ResetDirection()
    {
    }

    /*
    private Vector3 Align()
    {
        if (Vector3.Distance(transform.position, Rooster.transform.position) > 1f)
        {
            if (gameObject.layer == 12) return Rooster.transform.forward.normalized * HensParametersManager.HenAlignWeight;
            else if (gameObject.layer == 13) return Rooster.transform.forward.normalized * ChicksParametersManager.ChickAlignWeight;

        }
        return Vector3.zero;

    }

    private Vector3 Cohesion()
    {
        if(Vector3.Distance(transform.position, Rooster.transform.position) > 1f)
        {
            Vector3 cohesion = Rooster.transform.position;
            cohesion -= transform.position;

            if (gameObject.layer == 12) return cohesion.normalized * HensParametersManager.HenCohesionWeight;
            else if (gameObject.layer == 13) return cohesion.normalized * ChicksParametersManager.ChickCohesionWeight;

        }
        return Vector3.zero;

    }

    private Vector3 Separation(Collider[] Neighbors)
    {
        Vector3 separation = Vector3.zero;
        Vector3 tmp;
        for (int i = 0; i < Neighbors.Length; i += 1)
        {
			if (Neighbors [i] != null) {
				tmp = (transform.position - Neighbors[i].transform.position);
                Debug.Log("FOUND ONE!");
				separation += tmp.normalized / (tmp.magnitude + 0.0001f);
			}
        }
        if (gameObject.layer == 12) return separation.normalized * HensParametersManager.HenSeparationWeight;
        else if (gameObject.layer == 13) return separation.normalized * ChicksParametersManager.ChickSeparationWeight;
        return separation;

    }*/
}
