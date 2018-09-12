using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChickDecisionTree : MonoBehaviour
{
    public enum ChickPersonalities { COCKY, COWARD, CURIOUS, SISSY, SLY }
    public enum ChickStatus { ALARM, CATCHINGUP, FLEEING, GOINGTO, ROAMING, STARING };

    public ChickPersonalities Personality;
    public ChickStatus CurrentStatus;
    private NavMeshAgent movAgent;

    public GameObject Rooster;
    private DecisionTree dt;
    private GameObject NearestHen;
    private GameObject NearestPlayer;

    public LayerMask HensLayer;
    private Collider[] NearbyHens = new Collider[7];

    public LayerMask ChicksLayer;
    private Collider[] NearbyChicks = new Collider[7];

    public LayerMask PlayersLayer;
    private Collider[] NearbyPlayers = new Collider[4];

    private Collider[] RoosterCollider = new Collider[1];

    public float AnimationTimer = ChicksParametersManager.AlarmTime;

    // Use this for initialization
    void Start()
    {

        float RandomPersonality = Random.value;
        Debug.Log(gameObject.name + ": " + RandomPersonality);
        if (RandomPersonality < 0.1f) Personality = ChickPersonalities.COCKY;
        if (RandomPersonality >= 0.1f && RandomPersonality < 0.45f) Personality = ChickPersonalities.COWARD;
        if (RandomPersonality >= 0.45f && RandomPersonality < 0.6f) Personality = ChickPersonalities.CURIOUS;
        if (RandomPersonality >= 0.6f && RandomPersonality < 0.7f) Personality = ChickPersonalities.SISSY;
        if (RandomPersonality >= 0.7f) Personality = ChickPersonalities.SLY;

        movAgent = GetComponent<NavMeshAgent>();

        CurrentStatus = ChickStatus.ROAMING;

        // DT Decisions
        DTDecision decAnimationStop = new DTDecision(AnimationStop);
        DTDecision decDistantFromRooster = new DTDecision(DistantFromRooster);
        DTDecision decHenInFOV = new DTDecision(NearAHen);
        DTDecision decPlayerInFOV = new DTDecision(PlayerInFOV);
        DTDecision decPlayerInAlarmRange = new DTDecision(PlayerInAlarmRange);
        DTDecision decCockyChick = new DTDecision(CockyChick);
        DTDecision decCuriousChick = new DTDecision(CuriousChick);
        DTDecision decSissyChick = new DTDecision(SissyChick);
        DTDecision decSlyChick = new DTDecision(SlyChick);

        // DT Actions
        DTAction actWait = new DTAction(Wait);
        DTAction actChaseRooster = new DTAction(ChaseRooster);
        DTAction actChasePlayer = new DTAction(ChasePlayer);
        DTAction actChaseHen = new DTAction(ChaseHen);
        DTAction actStareAtPlayer = new DTAction(StareAtPlayer);
        DTAction actFleeFromPlayer = new DTAction(FleeFromPlayer);
        DTAction actFleeFromHen = new DTAction(FleeFromHen);
        DTAction actAlarm = new DTAction(Alarm);
        DTAction actRoam = new DTAction(Roam);

        // DT Links

        decAnimationStop.AddLink(true, actWait);
        decAnimationStop.AddLink(false, decDistantFromRooster);

        decDistantFromRooster.AddLink(true, actChaseRooster);
        decDistantFromRooster.AddLink(false, decPlayerInFOV);

        decPlayerInFOV.AddLink(true, decPlayerInAlarmRange);
        decPlayerInFOV.AddLink(false, decSissyChick);

        decPlayerInAlarmRange.AddLink(true, actAlarm);
        decPlayerInAlarmRange.AddLink(false, decCockyChick);

        decCockyChick.AddLink(true, actChasePlayer);
        decCockyChick.AddLink(false, decCuriousChick);

        decCuriousChick.AddLink(true, actStareAtPlayer);
        decCuriousChick.AddLink(false, decSlyChick);

        decSissyChick.AddLink(true, actFleeFromHen);
        decSissyChick.AddLink(false, actRoam);

        decSlyChick.AddLink(true, decHenInFOV);
        decSlyChick.AddLink(false, actFleeFromPlayer);

        decHenInFOV.AddLink(true, actChaseHen);
        decHenInFOV.AddLink(false, actChaseRooster);


        // Setup DT
        dt = new DecisionTree(decAnimationStop);
        
        RoosterCollider[0] = Rooster.GetComponent<Collider>();

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

    private object NearAHen(object o)
    {
        return Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickSlyFOV, NearbyHens, HensLayer) > 0;
    }

    private object PlayerInFOV(object o)
    {
        return (Personality == ChickPersonalities.COWARD && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickCowardFOV, NearbyPlayers, PlayersLayer) > 0)
                    || Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyPlayers, PlayersLayer) > 0;
    }

    private object PlayerInAlarmRange(object o)
    {
        GetComponent<RoamingBehaviour>().ResetDirection();
        NearestPlayer = FindNearest(NearbyPlayers);
        return Vector3.Distance(NearestPlayer.transform.position, transform.position) <= ChicksParametersManager.ChickDangerFOV;
    }

    private object CowardChick(object o)
    {
        return Personality == ChickPersonalities.COWARD && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickCowardFOV, NearbyPlayers, PlayersLayer) > 0;
    }

    private object CockyChick(object o)
    {
        return Personality == ChickPersonalities.COCKY;
    }

    private object CuriousChick(object o)
    {
        return Personality == ChickPersonalities.CURIOUS;
    }

    private object SissyChick(object o)
    {
        return Personality == ChickPersonalities.SISSY && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyHens, HensLayer) > 0;
    }

    private object SlyChick(object o)
    {
        return Personality == ChickPersonalities.SLY;
    }

    //---------------------------------------------------------------- DT Actions ----------------------------------------------------------------------

    private object Wait(object o)
    {
        CurrentStatus = ChickStatus.ALARM;
        movAgent.SetDestination(transform.position);
        AnimationTimer += Time.deltaTime;

        return null;
    }

    private object ChaseRooster(object o)
    {
        CurrentStatus = ChickStatus.CATCHINGUP;
        movAgent.SetDestination(Rooster.transform.position);
        movAgent.speed = 5f;
        return null;
    }

    private object ChasePlayer(object o)
    {
        CurrentStatus = ChickStatus.GOINGTO;

        movAgent.speed = 3.5f;
        if (Vector3.Distance(transform.position, NearestPlayer.transform.position) > 0.5f)
            movAgent.SetDestination(NearestPlayer.transform.position);
        else transform.LookAt(new Vector3(NearestPlayer.transform.position.x, transform.position.y, NearestPlayer.transform.position.z));
        return null;

    }

    private object ChaseHen(object o)
    {
        CurrentStatus = ChickStatus.GOINGTO;
        NearestHen = FindNearest(NearbyHens);

        movAgent.speed = 3.5f;
        if (Vector3.Distance(transform.position, NearestHen.transform.position) > 0.5f)
            movAgent.SetDestination(NearestHen.transform.position);
        else transform.LookAt(new Vector3(NearestHen.transform.position.x, transform.position.y, NearestHen.transform.position.z));
        return null;

    }

    private object StareAtPlayer(object o)
    {
        CurrentStatus = ChickStatus.STARING;
        transform.rotation = Quaternion.LookRotation(NearestPlayer.transform.position - transform.position);
        movAgent.SetDestination(transform.position);
        return null;
    }

    private object FleeFromPlayer(object o)
    {
        CurrentStatus = ChickStatus.FLEEING;
        Vector3 EscapeDirection = transform.position - NearestPlayer.transform.position;
        movAgent.SetDestination(transform.position + (EscapeDirection.normalized) / 1.5f);
        movAgent.speed = 3.5f;
        return null;
    }

    private object FleeFromHen(object o)
    {
        CurrentStatus = ChickStatus.FLEEING;
        NearestHen = FindNearest(NearbyHens);
        Vector3 EscapeDirection = transform.position - NearestHen.transform.position;
        movAgent.SetDestination(transform.position + (EscapeDirection.normalized) / 1.5f);
        movAgent.speed = 3.5f;
        return null;
    }

    private object Alarm(object o)
    {
        AnimationTimer = 0;
        CurrentStatus = ChickStatus.ALARM;
        Rooster.GetComponent<RoosterBehaviour>().ActivateAlarm(NearestPlayer);
        return null;
    }

    private object Roam(object o)
    {
        CurrentStatus = ChickStatus.ROAMING;
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

        if (timer < 6f) timer += Time.deltaTime;
        if (timer > ChicksParametersManager.AlarmTime)
        {
            //agent.Resume();
            if (Vector3.Distance(Rooster.transform.position, transform.position) > 4f)
            {
                CurrentStatus = ChickStatus.CATCHINGUP;
                //CatchUp();
            }
            else
            {
                if ((Personality == ChickPersonalities.COWARD && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickCowardFOV, NearbyPlayers, PlayersLayer) > 0)
                    || Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyPlayers, PlayersLayer) > 0)
                {
                    GetComponent<RoamingBehaviour>().ResetDirection();
                    NearestPlayer = FindNearest(NearbyPlayers);

                    if (Vector3.Distance(NearestPlayer.transform.position, transform.position) > ChicksParametersManager.ChickDangerFOV)
                    {
                        if (Personality == ChickPersonalities.COCKY)
                        {
                            CurrentStatus = ChickStatus.GOINGTO;
                            GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestPlayer);
                        }
                        else if (Personality == ChickPersonalities.CURIOUS)
                        {
                            CurrentStatus = ChickStatus.STARING;
                            StopAndStare(NearestPlayer);
                        }
                        else if (Personality == ChickPersonalities.SLY)
                        {
                            if (Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickSlyFOV, NearbyHens, HensLayer) > 0)
                            {
                                NearestHen = FindNearest(NearbyHens);
                                CurrentStatus = ChickStatus.GOINGTO;
                                GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestHen);
                            }
                            else
                            {
                                CurrentStatus = ChickStatus.GOINGTO;
                                GetComponent<GoToBehaviour>().ExecuteBehaviour(Rooster);
                            }
                        }
                        else
                        {
                            CurrentStatus = ChickStatus.FLEEING;
                            GetComponent<FleeBehaviour>().ExecuteBehaviour(NearestPlayer);
                        }

                    }
                    else
                    {
                        timer = 0;
                        CurrentStatus = ChickStatus.ALARM;
                        GetComponent<AlarmBehaviour>().ExecuteBehaviour(NearestPlayer);
                    }
                }
                else
                {
                    if (Personality == ChickPersonalities.SISSY && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyHens, HensLayer) > 0)
                    {
                        NearestHen = FindNearest(NearbyHens);

                        if (Vector3.Distance(NearestHen.transform.position, transform.position) 
                            > ChicksParametersManager.ChickDangerFOV)
                        {
                            CurrentStatus = ChickStatus.FLEEING;
                            GetComponent<FleeBehaviour>().ExecuteBehaviour(NearestHen);
                        }
                        else
                        {
                            timer = 0;
                            CurrentStatus = ChickStatus.ALARM;
                            GetComponent<AlarmBehaviour>().ExecuteBehaviour(NearestHen);
                        }
                    }
                    else
                    {
                        Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyChicks, ChicksLayer);
                        CurrentStatus = ChickStatus.ROAMING;
                        GetComponent<RoamingBehaviour>().ExecuteBehaviour(Rooster);
                    }
                }
            }
        }
        
        //else agent.Stop();
    }*/

}
