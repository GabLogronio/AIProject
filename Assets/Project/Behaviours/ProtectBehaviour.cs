using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProtectBehaviour : GeneralBehaviour
{
    private NavMeshAgent agent;

    public LayerMask PlayersLayer;
    private Collider[] NearbyPlayers = new Collider[4];

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    override public void ExecuteBehaviour(GameObject target)
    {
        Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenFOV, NearbyPlayers, PlayersLayer);
        GameObject player = FindNearest(NearbyPlayers);

        Vector3 ComingDirection = player.transform.position - target.transform.position;
        agent.SetDestination(target.transform.position + (ComingDirection.normalized) / 1.5f);

    }

    private GameObject FindNearest(Collider[] Neighborgs)
    {
        float distance = 0f;
        float nearestDistance = float.MaxValue;
        GameObject NearestElement = null;

        foreach (Collider NearbyElement in Neighborgs)
        {
            if (NearbyElement != null && NearbyElement.gameObject != gameObject)
            {
                distance = Vector3.Distance(NearbyElement.transform.position, transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    NearestElement = NearbyElement.gameObject;
                }
            }
        }
        return NearestElement;
    }
}