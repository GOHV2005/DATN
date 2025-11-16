using UnityEngine;
using UnityEngine.AI;

public class MonsterNavigator : MonoBehaviour
{
    public Transform target; // The object to guard
    public float patrolRadius = 5f;
    public float patrolInterval = 3f;

    private NavMeshAgent agent;
    private float patrolTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        patrolTimer = patrolInterval;
    }

    void Update()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0f)
        {
            Vector2 patrolPoint = GetRandomPatrolPoint();
            agent.SetDestination(patrolPoint);
            patrolTimer = patrolInterval;
        }
    }

    Vector2 GetRandomPatrolPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        Vector2 point = (Vector2)target.position + randomOffset;
        return point;
    }
}
