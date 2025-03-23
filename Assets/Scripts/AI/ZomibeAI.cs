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
    public float _speed;
    public float _turnSpeed;
    public float _shambleDistance;
    public float _bigTurn;
    public float _smallTurn;

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
    public LayerMask _layerMask;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _vision = _eyes.GetComponent<Vision>();
        _navMeshAgent.angularSpeed = _turnSpeed;
        _navMeshAgent.speed = _speed;

    }
    void Awake()
    {
        MakeStateMap();
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
            action(); // Calls the mapped function
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
    /*
    void MoveToPosition(Vector3 position){
        Vector3 target;
        if (_moveTargetPosition != position){
            _moveTargetPosition = position;
            _navMeshAgent.SetDestination(position);
        }
        if (_navMeshAgent.path.corners.Count() > 1) target = _navMeshAgent.path.corners[1];
        else target = position;
        Debug.Log(target);
        if (!FacingPosition(target)){
            _navMeshAgent.speed = 0;
            TurnToFace(target);
            return;
        }
        _navMeshAgent.speed = _speed;
        
    }
    */
    
    public void MoveToPosition(Vector3 destination)
    {
        _navMeshAgent.SetDestination(destination);
        // Start the turn-then-move coroutine
        StartCoroutine(TurnThenMoveRoutine(destination));
    }
    
    IEnumerator TurnThenMoveRoutine(Vector3 destination)
    {
        // Continuously update the destination to ensure the path is valid
        _navMeshAgent.SetDestination(destination);

        while (true)
        {
            // Get the desired direction.
            // Use the first corner of the path if available; otherwise, use the destination.
            Vector3 targetPoint = (_navMeshAgent.path.corners.Length > 1) ? _navMeshAgent.path.corners[1] : destination;
            
            // Compute the horizontal direction (ignore Y)
            Vector3 flatDir = targetPoint - transform.position;
            flatDir.y = 0;
            //if(Vector3.Distance(targetPoint, transform.position) < 0.5f)
            if(flatDir.sqrMagnitude < 0.001f)
            {
                //this is compensating for the distance to the floor, it should probably be +/- height/distance to floor rather than flat
                _targetPosition = transform.position;
                Debug.Log("very close" + Vector3.Distance(targetPoint, transform.position));
                break;
            }
            
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            float angleDiff = Quaternion.Angle(transform.rotation, targetRot);
            
            // If we're not facing the target within our threshold, turn.
            if (angleDiff > 5f)
            {
                Debug.Log("I'm turning");
                _navMeshAgent.isStopped = true; // Stop moving while turning
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
            }
            else
            {
                Debug.Log("I'm moving");
                // Aligned enough; resume movement.
                _navMeshAgent.isStopped = false;
                _navMeshAgent.speed = _speed;
                break;
            }
            
            yield return null; // Wait until next frame
        }
    }
    bool FacingPosition(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0; // Ignore vertical difference

        return Vector3.Dot(dir, transform.forward) > 0.95f;
    }
    void TurnToFace(Vector3 target)
    {
        Debug.Log("I'm turning");
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

    Vector3 GetNearbyPoint(){
        float angle = 70f;
        Vector3 direction = Quaternion.Euler(angle, Random.Range(45f, 315f), 0) * Vector3.down;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 10f, _layerMask))
        {
            Vector3 targetPoint = hit.point;
            Debug.DrawRay(transform.position, direction * hit.distance, Color.green, 1f);
            Debug.Log("Hit point: " + hit.collider.name);
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1f, NavMesh.AllAreas))
            {
                Debug.Log("Hit on NavMesh: " + navMeshHit.position);
                return targetPoint;
            }
        }
        return Vector3.zero;
    }
    /**
    Behaviours
    */
    void Idle(){
        Debug.Log("I'm idling");
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
        Debug.Log("I'm Chasing: " + _targetPosition);
        MoveToPosition(_targetPosition);
    }
    void ShambleForwards(){
        if (Vector3.Distance(transform.position, _targetPosition) < 0.5f){
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
        Debug.Log(_count);
        if(_look != 0){
            RandomLook();
        }
        else if (Vector3.Distance(transform.position, _targetPosition) < 0.5f){
            _count -=1;
            _look = Random.Range(1,5)*2;

            for (int i = 0; i < 10; i++){
                var target = GetNearbyPoint();
                if (target != Vector3.zero){
                    _targetPosition = target;
                    //MoveToPosition(_targetPosition);
                }
            }
        }else {
            MoveToPosition(_targetPosition);
        }
    }
    /**
    Transitions
    */
    bool SeeSomething(){
        Debug.Log("trying to see");
        if (_vision._see_something){
            Debug.Log("Saw Something and changing state");
            return true;
        }
        return false;
    }
    bool WildGooseChace(){
        if (Vector3.Distance(transform.position, _targetPosition) < 0.5f){
            _count = 0;
            _timer = 0;
            _look = 0;
            Debug.Log("Hit target position");
            return true;
        }
        return false;
    }
    bool CountDown(){
        return _count == 0;
    }
    /**
    State Machine Creation
    */
        private void MakeStateMap(){
        _fsm = new FSM(zBehaviour.Idle);
        _fsm.AddState(zBehaviour.Idle, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Chase, WildGooseChace, zBehaviour.Shamble);
        _fsm.AddState(zBehaviour.Shamble, SeeSomething, zBehaviour.Chase);
        _fsm.AddState(zBehaviour.Shamble, WildGooseChace, zBehaviour.Investigate);
        _fsm.AddState(zBehaviour.Investigate, CountDown, zBehaviour.Idle);
        _fsm.AddState(zBehaviour.Investigate, SeeSomething, zBehaviour.Chase);


        _behaviourMap[zBehaviour.Idle] = Idle;
        _behaviourMap[zBehaviour.Investigate] = Investigate;
        _behaviourMap[zBehaviour.Chase] = Chase;
        _behaviourMap[zBehaviour.Shamble] = ShambleForwards;
    }

}
public enum zBehaviour{
    Chase,
    Shamble,
    Investigate,
    Search,
    Idle,
    Roam
}
