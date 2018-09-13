using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoosterBehaviour : MonoBehaviour {

    public static RoosterBehaviour instance = null;
    public enum RoosterStates { ROAMING, CHASING, VULNERABLE, RESETTING }
    public RoosterStates CurrentState;
    public bool IdleRooster = false;
    private NavMeshAgent movAgent;
    //-------------------------------------------------------------- Components --------------------------------------------------------------
    private Animator anim;
    private FSM fsm;
    //-------------------------------------------------------------- Current Status --------------------------------------------------------------
    private GameObject CurrentTarget;
    private Vector3 CurrentDestination;
    private int CurrentLives = 3;
    private bool Trapped = false;
    private bool Alarm = false;
    //-------------------------------------------------------------- Timers --------------------------------------------------------------
    private float ResettingTimer = 0f;
    private float VulnerableTimer = 0f;
    private float AttackingTimer = 0f;
    public float WalkingTime = 7;
    public float WaitingTime = 6f;
    //-------------------------------------------------------------- Others --------------------------------------------------------------
    public LayerMask PlayersLayer;
    private Collider[] NearbyPlayers = new Collider[4];
    private float RotationSpeed = 0.15f;
    private float RoamingSpeed = 2f;
    private float ChasingSpeed = 5.5f;
    private float ResettingSpeed = 8.5f;

    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start() {

        anim = GetComponent<Animator>();
        movAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        CurrentDestination = new Vector3(20, 0, 20);
        CurrentTarget = null;
        CurrentState = RoosterStates.ROAMING;

        FSMState Roaming = new FSMState();
        Roaming.stayActions.Add(RoamingState);

        FSMState Chasing = new FSMState();
        Chasing.stayActions.Add(ChasingState);

        FSMState Resetting = new FSMState();
        Resetting.stayActions.Add(ResettingState);

        FSMState Vulnerable = new FSMState();
        Vulnerable.stayActions.Add(VulnerableState);

        FSMTransition TransitionEnemyInFOW = new FSMTransition(EnemyInFOW);
        FSMTransition TransitionEnemyOutOfSight = new FSMTransition(EnemyOutOfSight);
        FSMTransition TransitionEnemyInAttackRange = new FSMTransition(EnemyInAttackRange);
        FSMTransition TransitionRemoveALife = new FSMTransition(RemoveALife);
        FSMTransition TransitionReset = new FSMTransition(Reset);
        FSMTransition TransitionEndedReset = new FSMTransition(EndedReset);
        FSMTransition TransitionTrapped = new FSMTransition(InTrappedState);
        FSMTransition TransitionTriggeredAlarm = new FSMTransition(TriggeredAlarm);

        Roaming.AddTransition(TransitionEnemyInFOW, Chasing);
        Roaming.AddTransition(TransitionTrapped, Vulnerable);
        Chasing.AddTransition(TransitionEnemyOutOfSight, Roaming);
        Chasing.AddTransition(TransitionEnemyInAttackRange, Roaming);
        Chasing.AddTransition(TransitionTrapped, Vulnerable);
        Chasing.AddTransition(TransitionTriggeredAlarm, Roaming);
        Vulnerable.AddTransition(TransitionRemoveALife, Resetting);
        Vulnerable.AddTransition(TransitionReset, Resetting);
        Resetting.AddTransition(TransitionEndedReset, Roaming);

        fsm = new FSM(Roaming);

        ResetTimers();

    }

    public void Update()
    {
        fsm.Update();
    }

    //---------------------------------------------------------------- FSM States ----------------------------------------------------------------------

    private void RoamingState()
    {
        CurrentState = RoosterStates.ROAMING;

        if (Vector3.Distance(transform.position, CurrentDestination) < 1f) WalkingTime = 0f;

        if (WalkingTime > 0f)
        {
            //cammina
            WalkingTime -= Time.deltaTime;
            anim.SetBool("Walking", true);

            movAgent.SetDestination(CurrentDestination);
            movAgent.speed = RoamingSpeed;


        }
        else if (WaitingTime > 0f)
        {
            //stai
            WaitingTime -= Time.deltaTime;
            anim.SetBool("Walking", false);

            movAgent.SetDestination(transform.position);
            movAgent.speed = 0f;

        }
        if (WalkingTime <= 0f && WaitingTime <= 0f)
        {
            ResetTimers();
            int RandomDestination = Random.Range(0, 4);
            switch (RandomDestination)
            {
                case 0:
                    CurrentDestination = new Vector3(20, 0, 20);
                    break;

                case 1:
                    CurrentDestination = new Vector3(-20, 0, 20);
                    break;

                case 2:
                    CurrentDestination = new Vector3(20, 0, -20);
                    break;

                case 3:
                    CurrentDestination = new Vector3(-20, 0, -20);
                    break;
            }
        }

    }

    private void ChasingState()
    {
        CurrentState = RoosterStates.CHASING;
        anim.SetBool("Walking", true);

        movAgent.SetDestination(CurrentTarget.transform.position);
        movAgent.speed = ChasingSpeed;

        if (Vector3.Distance(transform.position, CurrentTarget.transform.position) < 1.5f)
        {
            SingleTargetAttack();
        }

    }

    private void ResettingState()
    {
        CurrentState = RoosterStates.RESETTING;

        ResettingTimer -= Time.deltaTime;

        if (ResettingTimer >= 9f)
        {
            movAgent.SetDestination(transform.position);
            anim.SetBool("Walking", false);

        }
        else if (ResettingTimer < 9f)
        {
            anim.SetBool("Walking", true);
            movAgent.SetDestination(CurrentDestination);
            movAgent.speed = ResettingSpeed;

        }
    }

    private void VulnerableState()
    {
        CurrentState = RoosterStates.VULNERABLE;

        VulnerableTimer -= Time.deltaTime;

        if (Physics.OverlapSphereNonAlloc(transform.position, 2.5f, NearbyPlayers, PlayersLayer) > 0) AttackingTimer -= Time.deltaTime;
        else AttackingTimer = 3f;

    }

    //--------------------------------------------------------- FSM Conditions -----------------------------------------------------------------

    private bool EnemyInFOW()
    {

        if (Physics.OverlapSphereNonAlloc(transform.position, 7f, NearbyPlayers, PlayersLayer) > 0)
        {
            GameObject target = FindNearest(NearbyPlayers);
            Vector3 DirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, DirToTarget) < 55)
            {
                float DistToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (Physics.Raycast(transform.position, DirToTarget, DistToTarget, PlayersLayer))
                {
                    CurrentTarget = target;
                    return true;
                }
            }
        }

        return false;
    }

    private bool EnemyOutOfSight()
    {
        RaycastHit AbilityHit;
        Ray AbilityRay = new Ray(transform.position + new Vector3(0, 1.5f, 0), CurrentTarget.transform.position - (transform.position + new Vector3(0, 1.5f, 0)));
        if (Physics.Raycast(AbilityRay, out AbilityHit, 15))
        {
            if (AbilityHit.collider.gameObject == CurrentTarget)
            {
                return false;
            }
        }
        return true;
    }

    private bool EnemyInAttackRange()
    {
        return Vector3.Distance(CurrentTarget.transform.position, transform.position) < 1f;
    }

    private bool RemoveALife()
    {
        if (AttackingTimer <= 0f)
        {
            CurrentLives--;
            if (CurrentLives == 0)
            {
                //End Game
            }
            else PushBack(false);
            return true;
        }
        return false;
    }

    private bool Reset()
    {
        if (VulnerableTimer <= 0f)
        {
            PushBack(true);
            return true;
        }
        return false;
    }

    private bool EndedReset()
    {
        return ResettingTimer <= 0f || (CurrentState == RoosterStates.RESETTING && Vector3.Distance(CurrentDestination, transform.position) < 4f);
    }

    private bool InTrappedState()
    {
        return Trapped;
    }

    private bool TriggeredAlarm()
    {
        return Alarm;
    }

    //----------------------------------------------------------------- TOOL FUNCTIONS ----------------------------------------------------------------------

    private void ResetTimers()
    {
        ResettingTimer = 10f;
        VulnerableTimer = 6f;
        AttackingTimer = 3f;
        if (IdleRooster) WalkingTime = 0f;
        else WalkingTime = 5f;
        WaitingTime = 6f;
    }

    public void ActivateAlarm(GameObject target)
    {
        CurrentDestination = target.transform.position;
        WalkingTime = 7f;
        WaitingTime = 6f;
        Alarm = true;
    }

    public void Trap()
    {
        Trapped = true;
        anim.SetBool("Walking", false);
        anim.SetBool("Vulnerable", true);
    }

    private GameObject FindNearest(Collider[] Neighborgs)
    {
        float distance = 0f;
        float nearestDistance = float.MaxValue;
        GameObject NearestElement = null;

        foreach (Collider NearbyElement in Neighborgs)
        {
            if (NearbyElement != null)
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

    //--------------------------------------------------------- Attacks -------------------------------------------------------------------

    public void SingleTargetAttack()
    {
        anim.SetTrigger("Attack");
        CurrentTarget.GetComponent<Rigidbody>().AddForce((CurrentTarget.transform.position - transform.position).normalized * 100f, ForceMode.Impulse);
        CurrentTarget.GetComponent<Rigidbody>().AddForce(transform.up * 100f, ForceMode.Impulse);
        CurrentTarget.GetComponent<BasicPlayerController>().Die ();
        CurrentState = RoosterStates.ROAMING;
    }

    private void PushBack(bool kill)
    {
        Trapped = false;

        anim.SetBool("Vulnerable", false);
        anim.SetTrigger("Attack");

        foreach (Collider Player in NearbyPlayers)
        {
            if (Player != null)
            {
                Player.gameObject.GetComponent<Rigidbody>().AddForce((Player.gameObject.transform.position - transform.position).normalized * 100f, ForceMode.Impulse);
                Player.gameObject.GetComponent<Rigidbody>().AddForce(transform.up * 100f, ForceMode.Impulse);
                if(kill) Player.gameObject.GetComponent<BasicPlayerController>().Die();

            }
        }
        switch (CurrentLives)
        {
            case 3: CurrentDestination = new Vector3(30f, 0, -30f); break;

            case 2: CurrentDestination = new Vector3(30f, 0, 30f); break;

            case 1: CurrentDestination = new Vector3(-30f, 0, -30f); break;

        }

        ResetTimers();
    }
}
