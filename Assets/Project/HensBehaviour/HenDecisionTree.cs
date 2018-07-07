using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HenDecisionTree : MonoBehaviour {

    public enum HenPersonalities { AGGRESSIVE, COWARD, PROTECTIVE, UNLIKABLE};
    public enum HenStatus { CATCHINGUP, ENGAGING, FLEEING, GOINGTO, PROTECTING, ROAMING};

    public HenPersonalities Personality;
    public HenStatus CurrentStatus;

    public GameObject Rooster;
    private GameObject NearestChick;
    private GameObject NearestPlayer;
    private NavMeshAgent agent;

    public LayerMask HensLayer;
    private Collider[] NearbyHens = new Collider[7];

    public LayerMask ChicksLayer;
    private Collider[] NearbyChicks = new Collider[7];

    public LayerMask PlayersLayer;
    private Collider[] NearbyPlayers = new Collider[4];

    private float timer = 2f;

    // Use this for initialization
    void Start () {

        float RandomPersonality = Random.value;
        Debug.Log(gameObject.name + ": " + RandomPersonality);
        if (RandomPersonality < 0.2f) Personality = HenPersonalities.AGGRESSIVE;
        if (RandomPersonality >= 0.2f && RandomPersonality < 0.5f) Personality = HenPersonalities.COWARD;
        if (RandomPersonality >= 0.5f && RandomPersonality < 0.9f) Personality = HenPersonalities.PROTECTIVE;
        if (RandomPersonality >= 0.9f) Personality = HenPersonalities.UNLIKABLE;
        CurrentStatus = HenStatus.ROAMING;

        agent = GetComponent<NavMeshAgent>();

	}

    // Update is called once per frame
    void Update()
    {
        if (timer < 2f) timer += Time.deltaTime;
        if (timer > HensParametersManager.AttackTime)
        {
            if (Vector3.Distance(Rooster.transform.position, transform.position) > 5f)
            {
                CurrentStatus = HenStatus.CATCHINGUP;
                CatchUp();
            }
            else
            {

                if ((Personality == HenPersonalities.AGGRESSIVE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenAggressiveFOV, NearbyPlayers, PlayersLayer) > 0)
                    || Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenFOV, NearbyPlayers, PlayersLayer) > 0)
                {
                    NearestPlayer = FindNearest(NearbyPlayers);

                    if ((Personality == HenPersonalities.PROTECTIVE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenProtectiveFOV, NearbyChicks, ChicksLayer) > 0))
                    {
                        NearestChick = FindNearest(NearbyChicks);

                        if (Vector3.Distance(transform.position, NearestChick.transform.position) < Vector3.Distance(NearestPlayer.transform.position, NearestChick.transform.position))
                        {
                            if (Vector3.Distance(NearestPlayer.transform.position, transform.position) > HensParametersManager.HenAttackFOV)
                            {
                                CurrentStatus = HenStatus.PROTECTING;
                                GetComponent<ProtectBehaviour>().ExecuteBehaviour(NearestChick);

                            }
                            else
                            {
                                timer = 0;
                                CurrentStatus = HenStatus.ENGAGING;
                                GetComponent<EngageBehaviour>().ExecuteBehaviour(NearestPlayer);
                            }

                        }
                        else
                        {
                            if (Vector3.Distance(NearestPlayer.transform.position, transform.position) > HensParametersManager.HenAttackFOV)
                            {
                                CurrentStatus = HenStatus.GOINGTO;
                                GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestPlayer);
                            }
                            else
                            {
                                timer = 0;
                                CurrentStatus = HenStatus.ENGAGING;
                                GetComponent<EngageBehaviour>().ExecuteBehaviour(NearestPlayer);
                            }
                        }
                    }
                    else if ((Personality == HenPersonalities.UNLIKABLE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenProtectiveFOV, NearbyChicks, ChicksLayer) > 0))
                    {
                        NearestChick = FindNearest(NearbyChicks);

                        if (Vector3.Distance(NearestChick.transform.position, transform.position) > HensParametersManager.HenAttackFOV)
                        {
                            CurrentStatus = HenStatus.GOINGTO;
                            GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestChick);
                        }
                        else
                        {
                            timer = 0;
                            CurrentStatus = HenStatus.ENGAGING;
                            GetComponent<EngageBehaviour>().ExecuteBehaviour(NearestChick);
                        }

                    }
                    else
                    {
                        if ((Personality == HenPersonalities.COWARD) && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenCowardFOV, NearbyHens, HensLayer) <= 1)
                        {
                            CurrentStatus = HenStatus.FLEEING;
                            GetComponent<FleeBehaviour>().ExecuteBehaviour(NearestPlayer);
                        }
                        else
                        {
                            if (Vector3.Distance(NearestPlayer.transform.position, transform.position) > HensParametersManager.HenAttackFOV)
                            {
                                CurrentStatus = HenStatus.GOINGTO;
                                GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestPlayer);
                            }
                            else
                            {
                                timer = 0;
                                CurrentStatus = HenStatus.ENGAGING;
                                GetComponent<EngageBehaviour>().ExecuteBehaviour(NearestPlayer);
                            }

                        }
                    }

                }
                else
                {
                    Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenFOV, NearbyHens, HensLayer);
                    if(CurrentStatus != HenStatus.ROAMING) GetComponent<RoamingBehaviour>().ResetDirection();
                    CurrentStatus = HenStatus.ROAMING;
                    GetComponent<RoamingBehaviour>().ExecuteBehaviour(Rooster);
                }
            }
        }
    }

    private void CatchUp()
    {
        agent.speed = Rooster.GetComponent<NavMeshAgent>().speed;
        agent.SetDestination(Rooster.transform.position);
        //transform.rotation = Quaternion.LookRotation(Rooster.transform.position - transform.position);
        //transform.position = Vector3.MoveTowards(transform.position, Rooster.transform.position, HensParametersManager.HenSpeed * Time.deltaTime);
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
