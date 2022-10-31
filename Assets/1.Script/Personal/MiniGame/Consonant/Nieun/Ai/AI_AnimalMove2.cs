using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//안쓰는 스크립트
public class AI_AnimalMove2 : MonoBehaviour
{
    [SerializeField]
    Transform targetPos; //쫒아가는 target 위치
    NavMeshAgent nav;
    float range = 5; //랜덤 범위
    Vector3 point; 
    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        //target과 거리가 3이하이면 point 전환
        if (Vector3.Distance(transform.position, targetPos.position) < 8f)
        {
            if (RandomPoint(targetPos.position, range, out point))
            {
                targetPos.position = point;
            }
        }
        nav.SetDestination(targetPos.position);
    }
    //랜덤으로 Point 변경
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
