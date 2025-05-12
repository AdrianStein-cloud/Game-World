using UnityEngine;
using UnityEngine.UIElements;

public class Vision : MonoBehaviour
{
    public bool _see_something = false;
    public Transform _target;
    public int _sight_range;
    public int _field_of_view;
    public LayerMask _layerMask;

    private void Awake()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    void FixedUpdate()
    {
        SeeTheTarget();
    }
    void SeeTheTarget()
    {
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-_field_of_view/2, Vector3.up) * transform.forward *15, Color.yellow); 
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(_field_of_view/2, Vector3.up) * transform.forward *15, Color.yellow); 

        //is target within field of view
        float angle = Vector3.Angle(transform.forward, _target.position - transform.position);
        if (angle > _field_of_view/2) 
        {
            _see_something = false;
            return;
        }
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, (_target.position - transform.position).normalized, out hit, (_target.position - transform.position).magnitude, _layerMask))
        {
            if (hit.collider.gameObject == _target.gameObject)
            { 
                Debug.DrawRay(transform.position, _target.position - transform.position, Color.green); 
                _see_something = true;
            }
            else
            { 
                Debug.DrawRay(transform.position, (_target.position - transform.position).normalized * hit.distance, Color.red); 
                _see_something = false;
            }
        }
        else
        {
            _see_something = false;
        }
    }
    public bool SeeTarget(Transform target)
    {
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-_field_of_view/2, Vector3.up) * transform.forward *15, Color.yellow); 
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(_field_of_view/2, Vector3.up) * transform.forward *15, Color.yellow); 

        //is target within field of view
        float angle = Vector3.Angle(transform.forward, target.position - transform.position);
        if (angle > _field_of_view/2) 
        {
            //_see_something = false;
            return false;
        }
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, (target.position - transform.position).normalized, out hit, (target.position - transform.position).magnitude, _layerMask))
        {
            if (hit.collider.gameObject == target.gameObject)
            { 
                Debug.DrawRay(transform.position, target.position - transform.position, Color.green); 
                return true;
            }
            else
            { 
                Debug.DrawRay(transform.position, (target.position - transform.position).normalized * hit.distance, Color.red); 
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
