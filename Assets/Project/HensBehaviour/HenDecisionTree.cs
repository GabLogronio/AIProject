using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HenDecisionTree : MonoBehaviour {

    public enum HenPersonalities { AGGRESSIVE, COWARD, PROTECTIVE, UNLIKABLE };
    public enum HenStatus { CATCHINGUP, ENGAGING, FLEEING, GOINGTO, PROTECTING, ROAMING };

    public HenPersonalities Personality;
    public HenStatus CurrentStatus;
    public float PushForce = 350f;

    private NavMeshAgent movAgent;

    public GameObject Rooster;
    private DecisionTree dt;
    private GameObject NearestChick;
    private GameObject NearestPlayer;

    public LayerMask HensLayer;
    private Collider[] NearbyHens = new Collider[7];

    public LayerMask ChicksLayer;
    private Collider[] NearbyChicks = new Collider[7];

    public LayerMask PlayersLayer;
    private Collider[] NearbyPlayers = new Collider[4];

    private float AnimationTimer = HensParametersManager.AttackTime;

    // Use this for initialization
    void Start() {

        float RandomPersonality = Random.value;
        Debug.Log(gameObject.name + ": " + RandomPersonality);
        if (RandomPersonality < 0.2f) Personality = HenPersonalities.AGGRESSIVE;
        if (RandomPersonality >= 0.2f && RandomPersonality < 0.5f) Personality = HenPersonalities.COWARD;
        if (RandomPersonality >= 0.5f && RandomPersonality < 0.9f) Personality = HenPersonalities.PROTECTIVE;
        if (RandomPersonality >= 0.9f) Personality = HenPersonalities.UNLIKABLE;
        CurrentStatus = HenStatus.ROAMING;

        movAgent = GetComponent<NavMeshAgent>();

        // DT Decisions
        DTDecision decAnimationStop = new DTDecision(AnimationStop);
        DTDecision decDistantFromRooster = new DTDecision(DistantFromRooster);
        DTDecision decPlayerInFOV = new DTDecision(PlayerInFOV);
        DTDecision decPlayerInAttackRange = new DTDecision(PlayerInAttackRange);
        DTDecision decChickInAttackRange = new DTDecision(ChickInAttackRange);
        DTDecision decNearerToTheChick = new DTDecision(NearerToTheChick);
        DTDecision decCowardHen = new DTDecision(CowardHen);
        DTDecision decUnlikableHen = new DTDecision(UnlikableHen);
        DTDecision decProtectiveHen = new DTDecision(ProtectiveHen);

        // DT Actions
        DTAction actWait = new DTAction(Wait);
        DTAction actChaseRooster = new DTAction(ChaseRooster);
        DTAction actFlee = new DTAction(Flee);
        DTAction actProtect = new DTAction(Protect);
        DTAction actEngageChick = new DTAction(EngageChick);
        DTAction actEngagePlayer = new DTAction(EngagePlayer);
        DTAction actChasePlayer = new DTAction(ChasePlayer);
        DTAction actRoam = new DTAction(Roam);

        // DT Links
        decAnimationStop.AddLink(true, actWait);
        decAnimationStop.AddLink(false, decDistantFromRooster);

        decDistantFromRooster.AddLink(true, actChaseRooster);
        decDistantFromRooster.AddLink(false, decPlayerInFOV);

		decPlayerInFOV.AddLink(true, decCowardHen);
        decPlayerInFOV.AddLink(false, decUnlikableHen);

        decPlayerInAttackRange.AddLink(true, actEngagePlayer);
        decPlayerInAttackRange.AddLink(false, decProtectiveHen);

        decChickInAttackRange.AddLink(true, actEngageChick);
        decChickInAttackRange.AddLink(false, actRoam);

        decNearerToTheChick.AddLink(true, actProtect);
        decNearerToTheChick.AddLink(false, actChasePlayer);

        decCowardHen.AddLink(true, actFlee);
        decCowardHen.AddLink(false, decPlayerInAttackRange);

        decUnlikableHen.AddLink(true, decChickInAttackRange);
        decUnlikableHen.AddLink(false, actRoam);

        decProtectiveHen.AddLink(true, decNearerToTheChick);
        decProtectiveHen.AddLink(false, actChasePlayer);

        // Setup DT
        dt = new DecisionTree(decAnimationStop);

    }

    private void Update()
    {
		dt.walk();
    }

    //---------------------------------------------------------------- DT Decisions ----------------------------------------------------------------------

    private object AnimationStop(object o)
    {
        return AnimationTimer <= HensParametersManager.AttackTime;
    }

    private object DistantFromRooster(object o)
    {
        return Vector3.Distance(Rooster.transform.position, transform.position) > 4f;
    }

    private object PlayerInFOV(object o)
    {
        if ((Personality == HenPersonalities.AGGRESSIVE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenAggressiveFOV, NearbyPlayers, PlayersLayer) > 0)
                    || Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenFOV, NearbyPlayers, PlayersLayer) > 0)
        {
            NearestPlayer = FindNearest(NearbyPlayers);
            return true;
        }
        else return false;
    }

    private object PlayerInAttackRange(object o)
    {
        return Vector3.Distance(NearestPlayer.transform.position, transform.position) <= HensParametersManager.HenAttackFOV;
    }

    private object ChickInAttackRange(object o)
    {
        NearestChick = FindNearest(NearbyChicks);
        return Vector3.Distance(NearestChick.transform.position, transform.position) <= HensParametersManager.HenAttackFOV;
    }

    private object NearerToTheChick(object o)
    {
        NearestChick = FindNearest(NearbyChicks);
        return Vector3.Distance(transform.position, NearestChick.transform.position) < Vector3.Distance(NearestPlayer.transform.position, NearestChick.transform.position);
    }

    private object CowardHen(object o)
    {
        return Personality == HenPersonalities.COWARD && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenCowardFOV, NearbyHens, HensLayer) <= 1;
    }

    private object UnlikableHen(object o)
    {
        return Personality == HenPersonalities.UNLIKABLE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenProtectiveFOV, NearbyChicks, ChicksLayer) > 0;
    }

    private object ProtectiveHen(object o)
    {
        return Personality == HenPersonalities.PROTECTIVE && Physics.OverlapSphereNonAlloc(transform.position, HensParametersManager.HenProtectiveFOV, NearbyChicks, ChicksLayer) > 0;
    }

    //---------------------------------------------------------------- DT Actions ----------------------------------------------------------------------

    private object Wait(object o)
    {
        CurrentStatus = HenStatus.ENGAGING;
        movAgent.SetDestination(transform.position);
        AnimationTimer += Time.deltaTime;

        return null;
    }

    private object ChaseRooster(object o)
    {
        CurrentStatus = HenStatus.CATCHINGUP;
        movAgent.SetDestination(Rooster.transform.position);
        movAgent.speed = 5f;

        return null;
    }

    private object ChasePlayer(object o)
    {
        CurrentStatus = HenStatus.GOINGTO;

        movAgent.speed = 3.5f;
        if (Vector3.Distance(transform.position, NearestPlayer.transform.position) > 0.5f)
            movAgent.SetDestination(NearestPlayer.transform.position);
        else transform.LookAt(NearestPlayer.transform.position);
        return null;

    }

    private object Flee(object o)
    {
        CurrentStatus = HenStatus.FLEEING;
        Vector3 EscapeDirection = transform.position - NearestPlayer.transform.position;
        movAgent.SetDestination(transform.position + (EscapeDirection.normalized) / 2f);
        movAgent.speed = 3.5f;
        return null;
    }

    private object Protect(object o)
    {
        CurrentStatus = HenStatus.PROTECTING;
        GetComponent<ProtectBehaviour>().ExecuteBehaviour(NearestChick);
        return null;
    }

    private object EngagePlayer(object o)
    {
        AnimationTimer = 0;
        CurrentStatus = HenStatus.ENGAGING;
        NearestPlayer.GetComponent<Rigidbody>().AddForce(transform.forward * PushForce, ForceMode.Impulse);
        NearestPlayer.GetComponent<Rigidbody>().AddForce(transform.up * PushForce, ForceMode.Impulse);
        transform.LookAt(NearestPlayer.transform);
        return null;
    }

    private object EngageChick(object o)
    {
        AnimationTimer = 0;
        CurrentStatus = HenStatus.ENGAGING;
        NearestChick.GetComponent<Rigidbody>().AddForce(transform.forward * PushForce, ForceMode.Impulse);
        NearestChick.GetComponent<Rigidbody>().AddForce(transform.up * PushForce, ForceMode.Impulse);
        transform.LookAt(NearestChick.transform);
        return null;
    }

    private object Roam(object o)
    {
        CurrentStatus = HenStatus.ROAMING;
        movAgent.speed = 2.5f;
        GetComponent<RoamingBehaviour>().ExecuteBehaviour(Rooster);
        return null;
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

    /* Update is called once per frame
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
    }*/
}
