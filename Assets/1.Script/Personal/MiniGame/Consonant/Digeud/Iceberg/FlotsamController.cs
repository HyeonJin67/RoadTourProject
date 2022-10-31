using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 표류물 컨트롤러
public class FlotsamController : MonoBehaviour
{
    #region 변수
    // 물고기 돌아다닐 경로
    public List<Transform> wayPoint = new List<Transform>();
    // 생성된 물고기 탐길 부모
    public Transform FishBasket;
    //물고기 숫자
    public int Count;

    //물고기 프리팹
    [SerializeField]
    GameObject[] Fishes;

    // 랜덤 배치용 리스트
    [SerializeField]
    List<int> indexList = new List<int>();
    #endregion

    #region 함수

    private void Start()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            wayPoint.Add(transform.GetChild(i).transform);
        }

        for (int i = 0; i < Count; i++)
        {
            AddFish();
        }
    }

    //물고기 추가
    public void AddFish()
    {   
        int h = Random.Range(0, Fishes.Length);
        int[] array = indexList.ToArray();

        int p = RandomNumberPicker.GetRandomNumberByArray(array, wayPoint.Count);
        indexList.Add(p);

        if(indexList.Count >= wayPoint.Count)
        {
            indexList.Clear();
        }

        var fish = Instantiate(Fishes[h], wayPoint[p].position, Quaternion.identity, FishBasket);
        //fish.GetComponent<FishController>().Index = p;
        fish.GetComponent<FishHandler>().Index = p;
    }
    #endregion
}
