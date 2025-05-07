using UnityEngine;
using UnityEngine.AI;

public class NPCAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    [SerializeField] private GameObject _target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        if ( _agent == null)
        {
            Debug.LogError("NavMesg Agent is Null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(_target.transform.position);
    }
}
