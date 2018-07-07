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

    public GameObject Rooster;
    private GameObject NearestHen;
    private GameObject NearestPlayer;
    private NavMeshAgent agent;

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

        CurrentStatus = ChickStatus.ROAMING;
        RoosterCollider[0] = Rooster.GetComponent<Collider>();

        agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 6f) timer += Time.deltaTime;
        if (timer > ChicksParametersManager.AlarmTime)
        {
            agent.Resume();
            if (Vector3.Distance(Rooster.transform.position, transform.position) > 4f)
            {
                CurrentStatus = ChickStatus.CATCHINGUP;
                CatchUp();
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
        else agent.Stop();
    }

    private void CatchUp()
    {
        agent.speed = Rooster.GetComponent<NavMeshAgent>().speed;
        agent.SetDestination(Rooster.transform.position);
        //transform.rotation = Quaternion.LookRotation(Rooster.transform.position - transform.position);
        //transform.position = Vector3.MoveTowards(transform.position, Rooster.transform.position, HensParametersManager.HenSpeed * Time.deltaTime);
    }

    private void StopAndStare(GameObject target)
    {
        agent.SetDestination(transform.position);
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
}
