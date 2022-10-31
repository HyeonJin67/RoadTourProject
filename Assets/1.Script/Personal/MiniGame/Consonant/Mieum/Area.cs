using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField]
    private float destoryDistance = 15;
    private AreaSpawner areaSpawner;
    private Transform playerTransform;

    public void Setup(AreaSpawner areaSpawner, Transform playerTransform)
    {
        this.areaSpawner = areaSpawner;
        this.playerTransform = playerTransform;
    }
    private void Update()
    {
        //플레이어 뒤로 가게 되면 새로운 구역을 생성하고 뒤에있는 구역은 삭제 시킨다
        if(playerTransform.position.z - transform.position.z >= destoryDistance)
        {
            //새로운 구역을 생성
            areaSpawner.SpawnArea();
            //현재 구역은 삭제
            Destroy(gameObject);
        }
    }
}
