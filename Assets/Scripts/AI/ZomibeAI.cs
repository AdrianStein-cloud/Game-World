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
    public zBehaviour _state;
    private Vector3 _targetPosition;
    private Vector3 _moveTargetPosition;
    private NavMeshAgent _navMeshAgent;
    private Vision _vision;
    private FSM _fsm;
    private Dictionary<zBehaviour, Action> _behaviourMap = new Dictionary<zBehaviour, Action>();
    private int _count;
    private int _look;
    private float _timer;
    private int _wait;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _vision = _eyes.GetComponent<Vision>();
        _navMeshAgent.angularSpeed = _turnSpeed;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.speed = _speed;

    }
    void Awake()
    {
        MakeStateMachine();
    }

    void Update()
    {
    }
    void FixedUpdate()
    {
        _fsm.UpdateState();
        _state = (zBehaviour)_fsm.GetCurrentState();
        if (_behaviourMap.TryGetValue(_state, out var action))
        {
            action();
        }
        else
        {
            Debug.Log("No function mapped");
        }
    }

    /**
    CONTROLLER STUFF, maybe it should be in it's own script one day
    */
    //currently using navmesh agent to move, maybe there should be a controller or animator instead
    public void MoveToPosition(Vector3 destination)
    {
        _navMeshAgent.SetDestination(destination);
        // Start the turn-then-move coroutine
        StartCoroutine(TurnThenMoveRoutine(destination));
    }
    
    IEnumerator TurnThenMoveRoutine(Vector3 destination)
    {
        _navMeshAgent.SetDestination(destination);

        while (true)
        {
            Vector3 targetPoint = (_navMeshAgent.path.corners.Length > 1) ? _navMeshAgent.path.corners[1] : destination;
            
            
            Vector3 flatDir = targetPoint - transform.position;
            flatDir.y = 0;
            
            if(flatDir.sqrMagnitude < 0.001f)
            {
                //this is compensating for the distance to the floor, it should probably be +/- height/distance to floor rather than flat
                _targetPosition = transform.position;
                break;
            }
            
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            float angleDiff = Quaternion.Angle(transform.rotation, targetRot);
            
            if (angleDiff > 5f)
            {
                //_navMeshAgent.isStopped = true;
                _navMeshAgent.speed = _speed * 0.20f;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
            }
            else
            {
                //_navMeshAgent.isStopped = false;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
                _navMeshAgent.speed = _speed;
                break;
            }
            
            yield return null;
        }
    }
    bool CloseEnough(){
        return Vector3.Distance(transform.position, _targetPosition) < 0.5f;
    }
    bool FacingPosition(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;

        return Vector3.Dot(dir, transform.forward) > 0.95f;
    }
    void TurnToFace(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), _turnSpeed * Time.fixedDeltaTime);
        }
    }
    void TurnLeft(){
        transform.Rotate(0, -_turnSpeed * Time.fixedDeltaTime *2.5f, 0);
    }
    void TurnRight(){
        transform.Rotate(0, _turnSpeed * Time.fixedDeltaTime *2.5f, 0);
    }
    void ResetCounts()
    {
        _count = 0;
        _timer = 0;
        _look = 0;
    }
    Vector3 GetNearbyPoint(){
        float angle = 70f;
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
                }
            }
        }else {
            MoveToPosition(_targetPosition);
        }
    }
    void Patrol(){
        if (_patrolPath.Count() == 0 | _patrolPath.Count() == 1 & CloseEnough()) {
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
        }
        if (CloseEnough()){
            _targetPosition = _patrolPath[0];
            _patrolPath.RemoveAt(0);
            _patrolPath.Add(_targetPosition);
        }
        MoveToPosition(_targetPosition);
    }
    /**
    Transitions
    */
    bool SeeSomething(){
        if (_vision._see_something){
            return true;
        }
        return false;
    }
    bool WildGooseChace(){
        if (CloseEnough()){
            ResetCounts();
            return true;
        }
        return false;
    }
    bool CountDown(){
        if (_count == 0){
            ResetCounts();
            return true;
        }
        return false;
    }
    /**
    State Machine Creation
    */
    private void MakeStateMachine(){
        _fsm = new FSM(zBehaviour.Patrol);
        _fsm.AddState(zBehaviour.Patrol, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Chase, WildGooseChace, zBehaviour.Shamble);
        _fsm.AddState(zBehaviour.Shamble, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Shamble, WildGooseChace, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Investigate, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Investigate, CountDown, zBehaviour.Patrol);


        _behaviourMap[zBehaviour.Idle] = Idle;
        _behaviourMap[zBehaviour.Investigate] = Investigate;
        _behaviourMap[zBehaviour.Chase] = Chase;
        _behaviourMap[zBehaviour.Shamble] = ShambleForwards;
        _behaviourMap[zBehaviour.Patrol] = Patrol;
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

}
public enum zBehaviour{
    Chase,
    Shamble,
    Investigate,
    Search,
    Idle,
    Roam,
    Patrol
}
