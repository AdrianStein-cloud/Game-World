using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using System.Collections;

public class ZomibeAI : MonoBehaviour
{
    public MonoBehaviour _eyes;
    [SerializeField] private float _speed;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _shambleDistance;
    [SerializeField] private float _bigTurn;
    [SerializeField] private float _smallTurn;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private List<Vector3> _patrolPath;
    public float _toouchDistance;
    public zBehaviour _state;
    public zBehaviour _pState;
    public int _runMultiplier;
    private int _runFactor;
    private Vector3 _targetPosition;
    private Vector3 _moveTargetPosition;
    private NavMeshAgent _navMeshAgent;
    private Vision _vision;
    private FSM _fsm;
    private FSM _patrolFsm;
    private int _count;
    private int _look;
    private float _timer;
    private int _wait;
    private Coroutine _turnRoutine;
    private bool _heardNoise;


    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _vision = _eyes.GetComponent<Vision>();
        _navMeshAgent.angularSpeed = _turnSpeed;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.speed = _speed * _runFactor;

    }
    void Awake()
    {
        MakeStateMachine();
        MakePatrolMachine();
        WalkSpeed();
    }

    void Update()
    {
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
    /**
    CONTROLLER STUFF, maybe it should be in it's own script one day
    */
    //currently using navmesh agent to move, maybe there should be a controller or animator instead
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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * _runFactor* Time.fixedDeltaTime);
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
        return Vector3.Distance(transform.position, _targetPosition) < 1.5f;
    }
    bool FacingPosition(Vector3 target)
    {
        Vector3 diff = target - transform.position;
        if(diff.sqrMagnitude < 0.0001f) {
            // The difference is negligible. Consider the agent as already facing.
            return true;
        }
        Vector3 dir = diff.normalized;
        dir.y = 0;
        Debug.Log("am I looking at " + target + " offset: " + Vector3.Dot(dir, transform.forward) );
        return Vector3.Dot(dir, transform.forward) > 0.98f;
    }
    void TurnToFace(Vector3 target)
    {
        Vector3 diff = target - transform.position;
        diff.y = 0;
        if(diff.sqrMagnitude < 0.0001f) return; // target is essentially at the same position

        Vector3 dir = diff.normalized;
        
        // If the target is almost exactly behind, add a tiny bias
        if(Vector3.Dot(transform.forward, dir) < -0.999f)
        {
            // Adding a small bias in an arbitrary direction (e.g., right)
            dir = (dir + Vector3.right * 0.01f).normalized;
        }
        
        Debug.Log("TurnToFace: direction = " + dir);
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _turnSpeed * _runFactor* Time.fixedDeltaTime);
        }
    }
    void TurnLeft(){
        transform.Rotate(0, -_turnSpeed * _runFactor* Time.fixedDeltaTime, 0);
    }
    void TurnRight(){
        transform.Rotate(0, _turnSpeed * _runFactor* Time.fixedDeltaTime, 0);
    }
    void ResetCounts()
    {
        _count = 0;
        _timer = 0;
        _look = 0;
        _navMeshAgent.ResetPath();
        _navMeshAgent.SetDestination(transform.position);
    }
    Vector3 GetNearbyPoint(){
        float angle = Random.Range(50f,70f);
        Vector3 direction = Quaternion.Euler(angle, Random.Range(45f, 315f), 0) * Vector3.down;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 10f, _layerMask))
        {
            Vector3 targetPoint = hit.point;
            Debug.DrawRay(transform.position, direction * hit.distance, Color.green, 1f);
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1f, NavMesh.AllAreas))
            {
                return targetPoint;
            }
        }
        return Vector3.zero;
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

    void RandomLook(){
        switch (_look){
            case 1:
                if (_timer > _bigTurn){
                    _timer = 0;
                    _look -= 1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnLeft();
                }
                break;
            case 2:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look -= 1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnRight();
                }
                break;
            case 3:
                if (_timer > _bigTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnRight();
                }
                break;
            case 4:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look -=1;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnLeft();
                }
                break;
            case 6:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnLeft();
                }
                break;
            case 8:
                if (_timer > _smallTurn){
                    _timer = 0;
                    _look = 0;
                } else {
                    _timer += Time.fixedDeltaTime;
                    TurnRight();
                }
                break;
            default:
            break;
        }
    }
    void Investigate(){
        if (_count == 0) _count = Random.Range(3,6);
        if(_look != 0){
            RandomLook();
        }
        else if (CloseEnough()){
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
            _look = Random.Range(-10,5)*2;
            if (_look < 0){
                _look = 0;
            }
        }
        else if(_look != 0){
            RandomLook();
        }
    }
    void PatrolFSM(){
        _patrolFsm.UpdateState();
        _pState = (zBehaviour)_patrolFsm.GetCurrentState();
        _patrolFsm.DoAction((zBehaviour)_patrolFsm.GetCurrentState());
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
        if (_look == 0) _look = 1;
        if (FacingPosition(_targetPosition)) _look = 0;
        TurnToFace(_targetPosition);
    }
    /**
    Transitions
    */
    bool PathFucked() {
        if (!_navMeshAgent.pathPending) {
            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) {
                Debug.Log("No valid path at all.");
                return true;  // Fail immediately if the path is invalid.
            }
            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial) {
                // Check if the agent has reached the end of its partial path.
                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.hasPath) {
                    Debug.Log("Reached the end of partial path — treat as failure.");
                    return true; // Agent has gone as far as it can.
                } else {
                    Debug.Log("Partial path — agent is still moving toward the reachable end.");
                    return false; // Still traveling along a partial path.
                }
            }
        }
        return false; // If still pending or a fully valid path, return false.
    }
    bool SeeSomething(){
        if (_vision._see_something){
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
    bool WildGooseChace(){
        if (CloseEnough()){
            WalkSpeed();
            ResetCounts();
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
    bool LookingAtPatrolPoint(){
        if (!_patrolPath.Any() || _patrolPath.Count() == 1 && CloseEnough()) return true;
        if (CloseEnough() && FacingPosition(_patrolPath[0])){
            Debug.Log("look p");
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
    /**
    State Machine Creation
    */
    private void MakeStateMachine(){
        _fsm = new FSM(zBehaviour.PatrolFSM);
        _fsm.AddState(zBehaviour.PatrolFSM, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.PatrolFSM, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.PatrolFSM, HearSomething, zBehaviour.Investigate);

        _fsm.AddState(zBehaviour.Chase, WildGooseChace, zBehaviour.Shamble);
        _fsm.AddState(zBehaviour.Chase, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Chase, PathFucked, zBehaviour.PatrolFSM);

        _fsm.AddState(zBehaviour.Shamble, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Shamble, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Shamble, WildGooseChace, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Shamble, HearSomething, zBehaviour.Investigate);
    

        _fsm.AddState(zBehaviour.TurnTo, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.TurnTo, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.TurnTo, Looked, zBehaviour.Investigate);

        _fsm.AddState(zBehaviour.Investigate, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Investigate, Touched, zBehaviour.TurnTo);
        _fsm.AddState(zBehaviour.Investigate, HearSomething, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Investigate, CountDown, zBehaviour.PatrolFSM);



        _fsm.AddBehaviour(zBehaviour.Investigate, Investigate);
        _fsm.AddBehaviour(zBehaviour.Chase, Chase);
        _fsm.AddBehaviour(zBehaviour.Shamble, ShambleForwards);
        _fsm.AddBehaviour(zBehaviour.PatrolFSM, PatrolFSM);
        _fsm.AddBehaviour(zBehaviour.TurnTo, TurnToTarget);
    }

    /**
    Sub State Machine
    */
    private void MakePatrolMachine(){
        _patrolFsm = new FSM(zBehaviour.Patrol);

        _patrolFsm.AddState(zBehaviour.Patrol, WildGooseChace, zBehaviour.LookAt);
        _patrolFsm.AddState(zBehaviour.LookAt, LookingAtPatrolPoint, zBehaviour.Wait);
        _patrolFsm.AddState(zBehaviour.Wait, TimeOut, zBehaviour.LookOut);
        _patrolFsm.AddState(zBehaviour.LookOut, Looked, zBehaviour.Patrol);

        _patrolFsm.AddBehaviour(zBehaviour.Wait, Wait);
        _patrolFsm.AddBehaviour(zBehaviour.Patrol, Patrol);
        _patrolFsm.AddBehaviour(zBehaviour.LookOut, RLook);
        _patrolFsm.AddBehaviour(zBehaviour.LookAt, LookAtNextPoint);
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
    float GetDistanceToEdge(Vector3 pos){
        NavMeshHit hit;
        NavMesh.FindClosestEdge(pos, out hit, NavMesh.AllAreas);
        float distance = Vector3.Distance(pos, hit.position);
        return distance;
    }

}
public enum zBehaviour{
    Chase,
    Shamble,
    Investigate,
    Search,
    Idle,
    Roam,
    Patrol,
    PatrolFSM,
    Wait,
    LookOut,
    LookAt,
    TurnTo
}
