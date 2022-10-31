using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//안쓰는 테스트 스크립트
public class AI_AnimalMove : MonoBehaviour
{
    NavMeshAgent nav;
    public Transform targetPos;
    float range = 10;
    Vector3 point;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>(); 
    }
    void Update()
    {

        if (Vector3.Distance(transform.position, targetPos.position) <10f)
        {
            if (RandomPoint(targetPos.position, range, out point))
            {
                targetPos.position = point;
            }
        }
        nav.SetDestination(targetPos.position);
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}
