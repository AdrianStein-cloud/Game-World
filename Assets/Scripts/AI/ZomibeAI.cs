using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEditor.UI;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class ZomibeAI : MonoBehaviour
{
    public MonoBehaviour _eyes;
    [SerializeField] private float _speed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _shambleDistance;
    [SerializeField] private float _bigTurn;
    [SerializeField] private float _smallTurn;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private List<Vector3> _patrolPath;
    [SerializeField] private float _stopDistance;
    public float _toouchDistance;
    public zBehaviour _state;
    public zBehaviour _pState;
    public zBehaviour _iState;
    public int _runMultiplier;
    private int _runFactor;
    public Vector3 _targetPosition;
    private Vector3 _moveTargetPosition;
    private NavMeshAgent _navMeshAgent;
    private Vision _vision;
    private FSM _fsm;
    private FSM _patrolFsm;
    private FSM _investigateFsm;
    private int _count;
    private int _look;
    private float _timer;
    private int _wait;
    private Coroutine _turnRoutine;

    private Animator anim;

    private bool _heardNoise;
    private int _attack;
    private float _armLayerWeight;
    private float _turnDir;
    private float _currentLookWeight;
    private float _currentArmWeight;
    private float _currentAttackWeight;
    private float _currentFreakWeight;
    [SerializeField] private bool _freak;
    private ZomibeAI _ztarget;
    private ZombieSound _zSound;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _vision = _eyes.GetComponent<Vision>();
        _navMeshAgent.angularSpeed = _turnSpeed;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.speed = _speed * _runFactor;
        _zSound = GetComponent<ZombieSound>();
        anim = GetComponent<Animator>();
        ZombieManager.Instance?.Register(this);
        if(_patrolPath.Count > 0) _targetPosition = _patrolPath[0];
        else _targetPosition = transform.position;

    }
    void Awake()
    {
        MakeStateMachine();
        MakePatrolMachine();
        MakeInvestigateMachine();
        WalkSpeed();
        _armLayerWeight = 1;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        anim.SetFloat("Speed", SpeedPercentage());
        anim.SetInteger("Look", _look);
        anim.SetInteger("Attack", _attack);
        anim.SetFloat("Turn", _turnDir, 0.2f, Time.deltaTime);
        anim.SetBool("FreakOut",_freak);
        

        //Animation layers
        //general arm placement
        _currentArmWeight = Mathf.MoveTowards(_currentArmWeight, _armLayerWeight, 6f * Time.deltaTime);
        anim.SetLayerWeight(1,_currentArmWeight);

        //head movement for looking around
        var _lookWeight = 0;
        if (_look > 0) _lookWeight = 1;
        _currentLookWeight = Mathf.MoveTowards(_currentLookWeight, _lookWeight, 6f * Time.deltaTime);
        anim.SetLayerWeight(2, _currentLookWeight);

        //Freaking
        var freakWeight = 0;
        if (_freak) freakWeight = 1;
        _currentFreakWeight = Mathf.MoveTowards(_currentFreakWeight, freakWeight, 6f * Time.deltaTime);
        anim.SetLayerWeight(3, _currentFreakWeight);

        //Attacking
        var attackWeight = 0;
        if (_attack > 0) attackWeight = 1;
        _currentAttackWeight = Mathf.MoveTowards(_currentAttackWeight, attackWeight, 6f * Time.deltaTime);
        anim.SetLayerWeight(4, _currentAttackWeight);
    }
    void FixedUpdate()
    {
        _fsm.UpdateState();
        _state = (zBehaviour)_fsm.GetCurrentState();
        _fsm.DoAction(_state);
    }
    /**
    * Senses, here for now
    */
    public void HearNoise(Vector3 position){
        _heardNoise = true;
        _targetPosition = position;
    }
    public bool TryHack(){
        if (_vision._see_something) return false;
        _vision._see_something = false;
        _vision.enabled = false;
        ZombieManager.Instance?.NudgeAllZombies(this);
        _freak = true;
        return true;
    }
    public void Strike(){
        _freak = false;
    }
    public void StrikeConnect(){
        if (_ztarget != null) _ztarget.Strike();
        _ztarget = null;
    }
    /*
    CONTROLLER STUFF, maybe it should be in it's own script one day
    */
    void MoveToPosition(Vector3 destination)
    {
        if (_turnRoutine != null) return; // prevent spamming new turn commands

        _turnRoutine = StartCoroutine(TurnThenMoveRoutine(destination));
    }
    

    IEnumerator TurnThenMoveRoutine(Vector3 destination)
    {
        // Calculate initial direction
        Vector3 targetPoint = (_navMeshAgent.path.corners.Length > 1)
            ? _navMeshAgent.path.corners[1]
            : destination;

        Vector3 flatDir = targetPoint - transform.position;
        flatDir.y = 0;

        if (flatDir.sqrMagnitude < 0.001f)
        {
            _turnRoutine = null;
            yield break;
        }

        Quaternion targetRot = Quaternion.LookRotation(flatDir);
        float angleDiff = Quaternion.Angle(transform.rotation, targetRot);

        _navMeshAgent.speed = _speed * _runFactor * 0.10f;
        _navMeshAgent.SetDestination(destination);

        while (angleDiff > 5f)
        {
            TurnToFace(targetPoint);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * _runFactor* Time.fixedDeltaTime);
            angleDiff = Quaternion.Angle(transform.rotation, targetRot);
            yield return new WaitForFixedUpdate();
        }

        _navMeshAgent.speed = _speed * _runFactor;

        _turnRoutine = null;
    }
    void RunSpeed(){
        _runFactor = _runMultiplier;
    }
    void WalkSpeed(){
        _runFactor = 1;
    }
    bool CloseEnough(){
        return FlatDistance(transform.position, _targetPosition) < _stopDistance;
    }
    
    bool FacingPosition(Vector3 target)
    {
        Vector3 diff = target - transform.position;
        diff.y = 0;
        if(diff.sqrMagnitude < 0.001f) {
            // The difference is negligible. Consider the agent as already facing.
            return true;
        }
        Vector3 dir = diff.normalized;
        //Debug.Log("am I looking at " + target + " offset: " + Vector3.Dot(dir, transform.forward) );
        return Vector3.Dot(dir, transform.forward) > 0.98f;
    }
    
    void TurnToFace(Vector3 target)
    {
        var dirr = GetTurnDirection(target);
        _turnDir = dirr/1.7f;
        Vector3 diff = target - transform.position;
        diff.y = 0;
        if(diff.sqrMagnitude < 0.0001f){
            _turnDir = 0;
            return;
        } 

        Vector3 dir = diff.normalized;
        
        //If the target is almost exactly behind, add a bias
        if(Vector3.Dot(transform.forward, dir) < -0.999f)
        {
            dir = (dir + Vector3.right * 0.01f).normalized;
        }
        
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _turnSpeed * _runFactor* Time.fixedDeltaTime);
        }
    }
    
    void ResetCounts()
    {
        _count = 0;
        _timer = 0;
        _look = 0;
        _navMeshAgent.ResetPath();
        _navMeshAgent.SetDestination(transform.position);
        _attack = 0;
        _turnDir = 0f;

    }
    /*
    */
    Vector3 GetNearbyPoint(){
       
        float angle = Random.Range(25f,85f);
        Vector3 direction = Quaternion.Euler(angle, Random.Range(0f, 360f), 0) * Vector3.down;
       
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 15f, _layerMask))
        {
            Vector3 targetPoint = hit.point;
            NavMeshHit navMeshHit;
            Debug.DrawRay(transform.position, direction * hit.distance, Color.green, 1f);
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 0.01f, NavMesh.AllAreas))
            {
                Debug.Log("got hit");
                return navMeshHit.position;
            }
        }
        return Vector3.zero;
    }

    public Vector3 GetRandomPointOnNavMesh(float radius = 5f)
    {
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(2f,radius);
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        // If no valid position found, return zero vector
        return Vector3.zero;
    }
    public bool IsWallBetween(Vector3 target)
    {
        
        Vector3 direction = (target - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target);
        Debug.DrawRay(transform.position, direction * distance, Color.green, 1f);

        return Physics.Raycast(transform.position, direction, distance, _wallLayerMask);
    }
   
    /**
    Behaviours
    */
    void Idle(){
        if (_look == 0) {
            if (_timer == 0 && _wait == 0){
                _wait = Random.Range(5,15);
            }
            _timer += Time.fixedDeltaTime;
            if (_timer > _wait){
                _timer = 0;
                _wait = 0;
                _look = Random.Range(1,5)*2;
            }
        }
        else RandomLook();
    }
    void Wait(){
        if (_timer == 0 && _wait == 0){
                _wait = Random.Range(-15,15);
                if (_wait < 5){
                    _wait = 0;
                }
            }
            _timer += Time.fixedDeltaTime;
    }
    void Chase(){
        if (_vision._see_something){
            _targetPosition = _vision._target.position;
        }
        MoveToPosition(_targetPosition);
    }
    void ShambleForwards(){
        if (CloseEnough()){
            _targetPosition = transform.position + transform.forward * _shambleDistance;
        }
        _navMeshAgent.destination = _targetPosition;
    }
    void LookAtNextPoint(){
        if (_patrolPath.Any() && _patrolPath.Count() > 1){
            TurnToFace(_patrolPath[0]);
        }
    }
    /*
    Random look should be changed to coincide with animations
    */
    void RandomLook(){
        switch (_look){
            case 1:
                if (_timer > _bigTurn){
                    _timer = 0;
                    _look -= 1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnLeft();
                }
                break;
            case 2:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look -= 1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnRight();
                }
                break;
            case 3:
                if (_timer > _bigTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnRight();
                }
                break;
            case 4:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look -=1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnLeft();
                }
                break;
            case 6:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnLeft();
                }
                break;
            case 8:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    //TurnRight();
                }
                break;
            default:
            break;
        }
    }

    void Investigate(){
        if (_count == 0) _count = Random.Range(2,4);
        if(_look != 0){
            RandomLook();
        }
        else if (CloseEnough()){
            _turnDir = 0;
            _count -=1;
            _look = Random.Range(1,5)*2;

            for (int i = 0; i < 10; i++){
                var target = GetNearbyPoint();
                if (target != Vector3.zero){
                    _targetPosition = target;
                    return;
                }
                else _targetPosition = transform.position;
            }
        }else {
            MoveToPosition(_targetPosition);
        }
    }
    void RLook(){
        if (_look == 0){
            _look = Random.Range(-5,5)*2;
            if (_look < 0){
                _look = 0;
            }
        }
        else if(_look != 0){
            RandomLook();
        }
    }
    void RLook2(){
        if (_look == 0){
            _look = Random.Range(2,5)*2;
            if (_look < 0){
                _look = 0;
            }
        }
        else if(_look != 0){
            RandomLook();
        }
    }
    void GoToTarget(){
        MoveToPosition(_targetPosition);
    }

    void PatrolFSM(){
        _patrolFsm.UpdateState();
        _pState = (zBehaviour)_patrolFsm.GetCurrentState();
        _patrolFsm.DoAction((zBehaviour)_patrolFsm.GetCurrentState());
    }
    void InvestigateFSM(){
        _investigateFsm.UpdateState();
        _iState = (zBehaviour)_investigateFsm.GetCurrentState();
        _investigateFsm.DoAction((zBehaviour)_investigateFsm.GetCurrentState());
    }
    void Patrol(){
        if (!_patrolPath.Any() || _patrolPath.Count() == 1 && CloseEnough()) {
            Idle();
            return;
            }
        if (!_patrolPath.Contains(_targetPosition)){
            _targetPosition = _patrolPath.OrderBy(n => GetNMDistance(transform.position, n)).First();
            if (GetNMDistance(transform.position, _targetPosition) > 199999){
                Idle();
                return;
            }
            while(_patrolPath[0] != _targetPosition){
                var tmp = _patrolPath[0];
                _patrolPath.RemoveAt(0);
                _patrolPath.Add(tmp);
            }
            _patrolPath.RemoveAt(0);
            _patrolPath.Add(_targetPosition);
        }
        if (CloseEnough()){
            _targetPosition = _patrolPath[0];
            _patrolPath.RemoveAt(0);
            _patrolPath.Add(_targetPosition);
        }
        MoveToPosition(_targetPosition);
    }
    void TurnToTarget(){
        //if (_look == 0) _look = 1;
        //if (FacingPosition(_targetPosition)) _look = 0;
        TurnToFace(_targetPosition);
    }
    void Attack(){
        if (_attack == 0){
            _attack = 1;
        }
        TurnToTarget();
    }
    void SlashAttack(){
        if (_attack == 0){
            _attack = 2;
            _wait = 2;
        }
        TurnToTarget();
    }
    void FreakOut(){
        if (_freak == false){
            _timer = 6;
            _freak = true;
        }
        if (_timer > 5){
            _zSound.PlayFromList("freak");
            _timer = 0;
        }
    }
    /**
    Transitions
    */
    bool PathFucked() {
        if (!_navMeshAgent.pathPending) {

            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) {
                Debug.Log("No valid path at all.");
                _patrolFsm.SetState(zBehaviour.Patrol);
                return true;
            }
            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial) {
                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.hasPath) {
                    Debug.Log("Reached the end of partial path — treat as failure.");
                    _patrolFsm.SetState(zBehaviour.Patrol);
                    return true;
                } else {
                    Debug.Log("Partial path — agent is still moving toward the reachable end.");
                    return false;
                }
            }
        }
        return false;
    }
    bool SeeSomething(){
        if (_vision._see_something){
            Debug.Log("See something");
            ResetCounts();
            RunSpeed();
            _patrolFsm.SetState(zBehaviour.Patrol);
            return true;
        }
        return false;
    }
    bool HearSomething(){
        if(_heardNoise){
            WalkSpeed();
            ResetCounts();
            _patrolFsm.SetState(zBehaviour.Patrol);
            _heardNoise = false;
            return true;
        }
        return false;
    }
    bool WildGooseChase(){
        if (CloseEnough()){
            WalkSpeed();
            ResetCounts();
            Debug.Log("wildGooseChase");
            return true;
        }
        return false;
    }
    bool WildGooseChased(){
        if (CloseEnough()){
            WalkSpeed();
            ResetCounts();
            _count = Random.Range(4,6);
            _look = 10;//Random.Range(1,5)*2;
            _patrolFsm.SetState(zBehaviour.LookOut);
            _investigateFsm.SetState(zBehaviour.LookOut);
            Debug.Log("wildGooseChase2");
            return true;
        }
        return false;
    }
    bool GotTo(){
        if (CloseEnough()){
            _targetPosition = GetSafeNearbyPoint();
            WalkSpeed();
            Debug.Log("got to");
            _count -=1;
            return true;
        }
        return false;
    }
    bool CountDown(){
        if (_count == 0){
            ResetCounts();
            _patrolFsm.SetState(zBehaviour.Patrol);
            return true;
        }
        return false;
    }
    bool TimeOut(){
        if (_timer > _wait){
            Debug.Log("timeout");
            ResetCounts();
            return true;
        }
        return false;
    }
    bool Looked(){
        if (_look == 0){
            Debug.Log("looked done");
            WalkSpeed();
            ResetCounts();
            return true;
        }
        return false;
    }
    bool LookedToInvestigate(){
        if (FacingPosition(_targetPosition)){
            Debug.Log("looked done");
            WalkSpeed();
            ResetCounts();
            _count = Random.Range(2,6);
            return true;
        }
        return false;
    }
    bool Turned(){
        if (FacingPosition(_targetPosition)){
            Debug.Log("Turned done");
            WalkSpeed();
            _turnDir = 0f;
            
            return true;
        }
        return false;
    }
    bool LookedRTarget(){
        if (_look == 0){
            WalkSpeed();
            Debug.Log("Looked R");
            
            return true;
        }
        return false;
    }
    bool LookingAtPatrolPoint(){
        if (!_patrolPath.Any() || _patrolPath.Count() == 1 && CloseEnough()) return true;
        if ( FacingPosition(_patrolPath[0]) ||CloseEnough()){
            _turnDir = 0f;
            Debug.Log("look p" + CloseEnough());
            return true;
        }
        return false;
    }
    bool Touched(){
        var targetPos = _eyes.GetComponent<Vision>()._target.position;
        if (Vector3.Distance(targetPos, transform.position) < _toouchDistance){

            RunSpeed();
            ResetCounts();
            _targetPosition = targetPos;
            _patrolFsm.SetState(zBehaviour.Patrol);
            Debug.Log("touched");
            return true;
        } 
        return false;
    }
    bool SeeAndTouched(){
        if (_attack > 0 || (Touched() && SeeSomething())){
            _armLayerWeight = 0;
            Debug.Log("seeAndTouch");
            return true;
        }
        return false;
    }

    bool Whiff(){
        if (_attack > 0 && !(SeeSomething() || Touched())){
            Debug.Log("WhiFF!!!");
            _attack = 0;
            ResetCounts();
            _count = Random.Range(4,6);
            _look = Random.Range(1,5)*2;
            _investigateFsm.SetState(zBehaviour.LookOut);
            return true;
        }
        return false;
    }
    bool SnapOut(){
        if (_freak == false){
            _vision.enabled = true;
            _vision._see_something = false;
            return true;
        }
        return false;
    }
    bool Freak(){
        if (_freak == true){
            _vision.enabled = false;
            WalkSpeed();
            ResetCounts();
            _timer = 6;
            return true;
        }
        return false;
    }
    bool NearFreakingZ(){
        foreach (var z in ZombieManager.Instance?.GetZomibes())
        {
            if (z._freak && FlatDistance(z.transform.position, transform.position) < _toouchDistance){
                ResetCounts();
                _targetPosition = z.transform.position;
                _ztarget = z;
                Debug.Log("SWIPE!");
                return true;
            }
        }
        return false;
    }
    /**
    State Machine Creation
    */
    private void MakeStateMachine(){
        _fsm = new FSM(zBehaviour.PatrolFSM);

        //Patrol behaviour, will go to the closest point in patrol point list and then go from point to point
        _fsm.AddState(zBehaviour.PatrolFSM, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.PatrolFSM, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.PatrolFSM, HearSomething, zBehaviour.Checkout);
        _fsm.AddState(zBehaviour.PatrolFSM, Freak, zBehaviour.FreakOut);

        //chase behaviour is for when the zombie sees the player and starts chasing
        _fsm.AddState(zBehaviour.Chase, SeeAndTouched, zBehaviour.Attack);
        _fsm.AddState(zBehaviour.Chase, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Chase, WildGooseChase, zBehaviour.Shamble);
        _fsm.AddState(zBehaviour.Chase, PathFucked, zBehaviour.PatrolFSM);
        //_fsm.AddState(zBehaviour.Chase, Freak, zBehaviour.FreakOut);

        //Shamble is taking a few steps forward after a chase ends with broken vision
        _fsm.AddState(zBehaviour.Shamble, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Shamble, Touched, zBehaviour.TurnTo);
        //_fsm.AddState(zBehaviour.Shamble, WildGooseChased, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Shamble, WildGooseChased, zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.Shamble, Freak, zBehaviour.FreakOut);
        //_fsm.AddState(zBehaviour.Shamble, HearSomething, zBehaviour.Checkout);
    
        //checkout is basically when the zombie hears something and goes to investigate
        _fsm.AddState(zBehaviour.Checkout, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Checkout, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Checkout, NearFreakingZ, zBehaviour.SlashAttack);
        //_fsm.AddState(zBehaviour.Checkout, HearSomething, zBehaviour.Checkout);
        _fsm.AddState(zBehaviour.Checkout, PathFucked, zBehaviour.PatrolFSM);
        //_fsm.AddState(zBehaviour.Checkout, WildGooseChased, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Checkout, WildGooseChased, zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.Checkout, Freak, zBehaviour.FreakOut);

        //TurnTo is used when the zombie is "touched" by player for now
        _fsm.AddState(zBehaviour.TurnTo, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.TurnTo, Touched, zBehaviour.TurnTo);
        //_fsm.AddState(zBehaviour.TurnTo, LookedToInvestigate, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.TurnTo, LookedToInvestigate, zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.TurnTo, Freak, zBehaviour.FreakOut);

        //investigate is when the zombie starts looking around a couple of times because it thought there was something there
        _fsm.AddState(zBehaviour.Investigate, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Investigate, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Investigate, HearSomething, zBehaviour.Checkout);
        _fsm.AddState(zBehaviour.Investigate, CountDown, zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.Investigate, PathFucked, zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.Investigate, Freak, zBehaviour.FreakOut);

        //Attack behaviour for attacking I guess
        _fsm.AddState(zBehaviour.Attack, Whiff, zBehaviour.Investigate);
        
        _fsm.AddState(zBehaviour.SlashAttack, TimeOut, zBehaviour.PatrolFSM);
        
        //Freaking out (being hacked)
        _fsm.AddState(zBehaviour.FreakOut, SnapOut, zBehaviour.PatrolFSM);    
    
        //add behaviours
        _fsm.AddBehaviour(zBehaviour.Investigate, InvestigateFSM);
        _fsm.AddBehaviour(zBehaviour.Chase, Chase);
        _fsm.AddBehaviour(zBehaviour.Shamble, ShambleForwards);
        _fsm.AddBehaviour(zBehaviour.PatrolFSM, PatrolFSM);
        _fsm.AddBehaviour(zBehaviour.TurnTo, TurnToTarget);
        _fsm.AddBehaviour(zBehaviour.Checkout, Chase);
        _fsm.AddBehaviour(zBehaviour.Attack, Attack);
        _fsm.AddBehaviour(zBehaviour.SlashAttack, SlashAttack);
        _fsm.AddBehaviour(zBehaviour.FreakOut, FreakOut);
    }

    /**
    Sub State Machines
    */
    private void MakePatrolMachine(){
        _patrolFsm = new FSM(zBehaviour.LookOut);

        _patrolFsm.AddState(zBehaviour.Patrol, WildGooseChase, zBehaviour.LookAt);
        _patrolFsm.AddState(zBehaviour.LookAt, LookingAtPatrolPoint, zBehaviour.Wait);
        _patrolFsm.AddState(zBehaviour.Wait, TimeOut, zBehaviour.LookOut);
        _patrolFsm.AddState(zBehaviour.LookOut, Looked, zBehaviour.Patrol);

        //add behaviours
        _patrolFsm.AddBehaviour(zBehaviour.Wait, Wait);
        _patrolFsm.AddBehaviour(zBehaviour.Patrol, Patrol);
        _patrolFsm.AddBehaviour(zBehaviour.LookOut, RLook);
        _patrolFsm.AddBehaviour(zBehaviour.LookAt, LookAtNextPoint);
    }
    private void MakeInvestigateMachine(){
        _investigateFsm = new FSM(zBehaviour.LookOut);

        _investigateFsm.AddState(zBehaviour.LookOut, LookedRTarget, zBehaviour.GoTo);
        _investigateFsm.AddState(zBehaviour.TurnTo, Turned, zBehaviour.LookOut);
        _investigateFsm.AddState(zBehaviour.TurnTo, GotTo, zBehaviour.LookOut);
        _investigateFsm.AddState(zBehaviour.GoTo, GotTo, zBehaviour.TurnTo);
        _investigateFsm.AddState(zBehaviour.GoTo, PathFucked, zBehaviour.LookOut);

        //add behaviours
        _investigateFsm.AddBehaviour(zBehaviour.LookOut, RLook2);
        _investigateFsm.AddBehaviour(zBehaviour.TurnTo, TurnToTarget);
        _investigateFsm.AddBehaviour(zBehaviour.GoTo, GoToTarget);
    }

    /*
    Tools
    */
    float GetPathDistance(NavMeshPath path)
    {
        return path.corners
                .Zip(path.corners.Skip(1), (a, b) => Vector3.Distance(a, b))
                .Sum();
    }
    float GetNMDistance(Vector3 from, Vector3 to){
        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path)) return GetPathDistance(path);
        return 200000;
    }
    float GetDistanceToEdge(Vector3 pos)
    {
        if (NavMesh.FindClosestEdge(pos, out NavMeshHit hit, NavMesh.AllAreas))
        {
            return Vector3.Distance(pos, hit.position);
        }
        return float.MaxValue; // or some fallback value
    }
    float SpeedPercentage(){
        return _navMeshAgent.velocity.magnitude/(_speed * _runMultiplier);
    }

    public Vector3 GetSafeNearbyPoint(float clearance = .1f, int maxAttempts = 200)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            //Vector3 candidate = GetNearbyPoint();
            var candidate = GetRandomPointOnNavMesh();
            if (candidate == Vector3.zero)// || IsWallBetween(candidate))
                continue;
            Debug.DrawRay(candidate, Vector3.up * clearance, Color.red, 1f);
            // Skip NavMesh sampling entirely
            //if (Physics.OverlapSphere(candidate, clearance, _wallLayerMask, QueryTriggerInteraction.Ignore).Length == 0){   
            Debug.Log("found Candidate " + candidate);
            Debug.Log("dist" + Vector3.Magnitude(transform.position - candidate));
            return candidate;
            //}
        }

        return _targetPosition;
    }

    int GetTurnDirection(Vector3 targetPosition)
    {
        Vector3 toTarget = (targetPosition - transform.position).normalized;
        Vector3 forward = transform.forward;

        //Project to XZ plane
        toTarget.y = 0f;
        forward.y = 0f;

        //Get signed angle between forward and direction to target
        float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

        if (angle > 0f)
            //Turn right
            return 1;   
        else if (angle < 0f)
            //Turn left
            return -1;  
        else
            //Already facing
            return 0;   
    }

    Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 diff = to - from;
        diff.y = 0f;
        return diff.sqrMagnitude < 0.0001f ? Vector3.zero : diff.normalized;
    }
    float FlatDistance(Vector3 from, Vector3 to){
        Vector3 a = from;
        Vector3 b = to;
        a.y = b.y = 0f;
        return Vector3.Distance(a,b);
    }

}
public enum zBehaviour{
    Chase,
    Shamble,
    Investigate,
    Checkout,
    Patrol,
    PatrolFSM,
    Wait,
    LookOut,
    LookAt,
    TurnTo,
    Attack,
    GoTo,
    FreakOut,
    SlashAttack
}
