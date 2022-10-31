using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Random_Animal : MonoBehaviour
{
    public int[] shuffledArray; //랜덤 배열
    //기역 니은 오브젝트 자식의 자식 오브젝트 갯수 만큼 랜덤 배열 저장
    private void Awake()
    {
        var indexArray = Enumerable.Range(0, transform.GetChild(0).transform.GetChild(0).childCount).ToArray();
        System.Random random = new System.Random();
        shuffledArray = indexArray.OrderBy(x => random.Next()).ToArray();
    }
}
