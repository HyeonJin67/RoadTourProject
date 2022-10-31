using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class RandomSpawn : MonoBehaviour
{
    [SerializeField]
    private GameObject[] prefabs; //찍어낼 게임 오브젝트를 넣는다
    private BoxCollider area;     //박스콜라이더의 사이즈를 가져오기 위함
    private int count = 100;      //찍어낼 게임 오브젝트 개수
    private List<GameObject> gameObject = new List<GameObject>();

    private void Start()
    {
        {
            area = GetComponent<BoxCollider>();

            for(int i = 0; i < count; ++i)
            {
                Spawn();//생성 + 스폰위치를 포함하는 함수
            }
            area.enabled = false;
        }
    }
    private Vector3 GetRandomPosition()
    {
        Vector3 basePosition = transform.position;
        Vector3 size = area.size;

        float posX = basePosition.x + Random.Range(-size.x / 2f, size.x / 2f);
        float posY = 0.5f;
        float posZ = basePosition.x + Random.Range(-size.x / 2f, size.x / 2f);

        Vector3 spawnPos = new Vector3(posX, posY, posZ);

        return spawnPos;
    }
    private void Spawn()
    {
        int selection = Random.Range(0, prefabs.Length);
        GameObject selectedPrefab =prefabs[selection];
        Vector3 spawnPos = GetRandomPosition();//랜덤위치함수
        GameObject instance = Instantiate(selectedPrefab , spawnPos , Quaternion.identity);
        gameObject.Add(instance);
    }
}
