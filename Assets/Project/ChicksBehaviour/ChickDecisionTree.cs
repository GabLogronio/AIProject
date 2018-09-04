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
    private MovementGoToDelegate movDelegate;

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

    public float timer = 5f;

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

        movDelegate = GetComponent<MovementGoToDelegate>();

        CurrentStatus = ChickStatus.ROAMING;

        // DT Decisions
        DTDecision dDistantFromRooster = new DTDecision(DistantFromRooster);
        DTDecision dNearAHen = new DTDecision(NearAHen);
        DTDecision dPlayerInRange = new DTDecision(PlayerInRange);
        DTDecision dNoPlayerTooClose = new DTDecision(NoPlayerTooClose);
        DTDecision dNoHenTooClose = new DTDecision(NoHenTooClose);
        DTDecision dCowardChick = new DTDecision(CowardChick);
        DTDecision dCockyChick = new DTDecision(CockyChick);
        DTDecision dCuriousChick = new DTDecision(CuriousChick);
        DTDecision dSissyChick = new DTDecision(SissyChick);
        DTDecision dSlyChick = new DTDecision(SlyChick);

        // DT Actions
        DTAction aCatchUp = new DTAction(CatchUp);
        DTAction aGoTo = new DTAction(GoTo);
        DTAction aStareAt = new DTAction(StareAt);
        DTAction aFlee = new DTAction(Flee);
        DTAction aAlarm = new DTAction(Alarm);
        DTAction aRoam = new DTAction(Roam);

        // DT Links
        dDistantFromRooster.AddLink(true, aCatchUp);
        dDistantFromRooster.AddLink(false, dCowardChick);

        dCowardChick.AddLink(true, dNoPlayerTooClose);
        dCowardChick.AddLink(false, dPlayerInRange);

        dPlayerInRange.AddLink(true, dNoPlayerTooClose);
        dPlayerInRange.AddLink(false, dSissyChick);

        dNoPlayerTooClose.AddLink(true, dCockyChick);
        dNoPlayerTooClose.AddLink(false, aAlarm);

        dCockyChick.AddLink(true, aGoTo);
        dCockyChick.AddLink(false, dCuriousChick);

        dCuriousChick.AddLink(true, aStareAt);
        dCuriousChick.AddLink(false, dSlyChick);

        dSlyChick.AddLink(true, dNearAHen);
        dSlyChick.AddLink(false, aFlee);

        dNearAHen.AddLink(true, aGoTo);
        dNearAHen.AddLink(false, aGoTo);

        dSissyChick.AddLink(true, dNoHenTooClose);
        dSissyChick.AddLink(false, aRoam);

        dNoHenTooClose.AddLink(true, aFlee);
        dNoHenTooClose.AddLink(false, aAlarm);

        // Setup DT
        dt = new DecisionTree(dDistantFromRooster);
        StartCoroutine(Patrol());

        RoosterCollider[0] = Rooster.GetComponent<Collider>();

    }

    public IEnumerator Patrol()
    {
        while (true)
        {
            dt.walk();
            yield return new WaitForSeconds(0.5f);
        }
    }

    //---------------------------------------------------------------- DT Decisions ----------------------------------------------------------------------

    private object DistantFromRooster(object o)
    {
        return Vector3.Distance(Rooster.transform.position, transform.position) > 4f;
    }

    private object NearAHen(object o)
    {
        return Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickSlyFOV, NearbyHens, HensLayer) > 0;
    }

    private object PlayerInRange(object o)
    {
        return Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyPlayers, PlayersLayer) > 0;
    }

    private object NoPlayerTooClose(object o)
    {
        return Vector3.Distance(NearestPlayer.transform.position, transform.position) > ChicksParametersManager.ChickDangerFOV;
    }

    private object NoHenTooClose(object o)
    {
        return Vector3.Distance(NearestHen.transform.position, transform.position) > ChicksParametersManager.ChickDangerFOV;
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
        return Personality == ChickPersonalities.SISSY;
    }

    private object SlyChick(object o)
    {
        return Personality == ChickPersonalities.SISSY && Physics.OverlapSphereNonAlloc(transform.position, ChicksParametersManager.ChickFOV, NearbyHens, HensLayer) > 0;
    }

    //---------------------------------------------------------------- DT Actions ----------------------------------------------------------------------

    private object CatchUp(object o)
    {
        CurrentStatus = ChickStatus.CATCHINGUP;
        movDelegate.SetDestination(Rooster.transform.position);
        movDelegate.SetSpeed(6f);
        return null;
    }

    private object GoTo(object o)
    {
        CurrentStatus = ChickStatus.GOINGTO;
        GetComponent<GoToBehaviour>().ExecuteBehaviour(NearestPlayer);
        return null;

    }

    private object StareAt(object o)
    {
        CurrentStatus = ChickStatus.STARING;
        transform.rotation = Quaternion.LookRotation(NearestPlayer.transform.position - transform.position);
        return null;
    }

    private object Flee(object o)
    {
        CurrentStatus = ChickStatus.FLEEING;
        GetComponent<FleeBehaviour>().ExecuteBehaviour(NearestPlayer);
        return null;
    }

    private object Alarm(object o)
    {
        timer = 0;
        CurrentStatus = ChickStatus.ALARM;
        GetComponent<AlarmBehaviour>().ExecuteBehaviour(NearestPlayer);
        return null;
    }

    private object Roam(object o)
    {
        CurrentStatus = ChickStatus.ROAMING;
        GetComponent<RoamingBehaviour>().ExecuteBehaviour(Rooster);
        return null;
    }

    private void StopAndStare(GameObject target)
    {
        //agent.SetDestination(transform.position);
        transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
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
