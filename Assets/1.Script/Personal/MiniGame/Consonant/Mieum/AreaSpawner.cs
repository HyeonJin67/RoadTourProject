using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] areaPrefabs;       //구역 프리팹 배열
    [SerializeField]
    private int spawnAreaCountAtStart = 3;  //게임 시작 시 최초 생성되는 구역 개수 
    [SerializeField]
    private float zDistance = 20;           //구역 사이의 거리 (z)
    private int areaIndex = 0;              //구역 인덱스 (배치되는 구역의 z위치 연산에 사용)

    [SerializeField]
    private Transform playerTransform;      //플레이어 Transform
    private void Awake()
    {
        //spawnAreaCountAtStart에 저장된 개수만큼 최초 구역 생성
        for(int i = 0; i<spawnAreaCountAtStart; ++i)
        {
            //첫 번째 구역은 항상 0번 구역 프리팹으로 설정
            if(i == 0)
            {
                SpawnArea(false);
            }
            else
            {
                SpawnArea();
            }
        }
    }
    public void SpawnArea(bool isRandom = true)
    {
        GameObject clone = null;

        if( isRandom == false )
        {
            clone = Instantiate(areaPrefabs[0]);
        }
        else//isRandom true이면 랜덤한 구역을 생성한다
        {
            int index = Random.Range(0, areaPrefabs.Length);
            clone = Instantiate(areaPrefabs[index]);
        }
        //구역이 배치되는 위치 설정 (z축은 현재 구역 인덱스 * zDistance)
        clone.transform.position = new Vector3(0, 0, areaIndex * zDistance);

        //구역이 삭제될 때 새로운 구역을 생성할 수 있도록 this와 플레이어의 Transform 정보 전달
        clone.GetComponent<Area>().Setup(this,playerTransform);

        areaIndex++;
    }
}
