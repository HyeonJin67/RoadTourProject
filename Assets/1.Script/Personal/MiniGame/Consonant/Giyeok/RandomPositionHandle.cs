using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//Hierarchy창에 바꾸고 싶은 자음의 단어 오브젝트들을 알맞은 더미 위치에 넣어주세요 
//Hierarchy창에 넣어준 오브젝트들을 랜덤위치에 넣어주는 스크립트
public class RandomPositionHandle : MonoBehaviour
{
    [Tooltip("물속에 위치할 오브젝트들 모두 넣어주세요")]
    [SerializeField]
    GameObject[] randomOb_inWater;
    [Tooltip("물속에 랜덤으로 바뀔 위치값용 오브젝트를 모두 넣어주세요")]
    [SerializeField]
    GameObject[] randomPosition_inWater;
    [Tooltip("육지에 위치할 오브젝트를 모두 넣어주세요")]
    [SerializeField]
    GameObject[] randomOb_inLand;
    [Tooltip("육지에 랜덤으로 바뀔 위치값용 오브젝트를 모두 넣어주세요")]
    [SerializeField]
    GameObject[] randomPosition_inLand;
    [Tooltip("틀린단어용 오브젝트를 모두 넣어주세요")]
    [SerializeField]
    GameObject[] wrongOb;
    int plus = 0;

    void Start()
    {
        System.Random random = new System.Random(); 
        var randomArray1 = Enumerable.Range(0, randomPosition_inWater.Length).ToArray(); //인덱스용으로 순서대로 배열에 넣어주기
        var randomArray2 = Enumerable.Range(0, randomOb_inWater.Length).ToArray();
        var shuffle1 = randomArray1.OrderBy(x => random.Next()).ToArray(); //순서대로 넣어둔 배열을 랜덤으로 섞어서 새로운 셔플배열 만들기
        var shuffle2 = randomArray2.OrderBy(x => random.Next()).ToArray();
        var randomArray5 = Enumerable.Range(0, wrongOb.Length).ToArray();
        var shuffle5 = randomArray5.OrderBy(x => random.Next()).ToArray();
        while (plus < 3)
        {
            if (plus == 0)
            {
                wrongOb[shuffle5[plus]].transform.position = randomPosition_inWater[shuffle1[plus]].transform.position;
                wrongOb[shuffle5[plus]].transform.rotation = randomPosition_inWater[shuffle1[plus]].transform.rotation;
                wrongOb[shuffle5[plus]].SetActive(true);
            }
            else
            {
                randomOb_inWater[shuffle2[plus]].transform.position = randomPosition_inWater[shuffle1[plus]].transform.position;
                randomOb_inWater[shuffle2[plus]].transform.rotation = randomPosition_inWater[shuffle1[plus]].transform.rotation;
                randomOb_inWater[shuffle2[plus]].SetActive(true);
            }
            plus++;
        }
        plus = 0;
        var randomArray3 = Enumerable.Range(0, randomPosition_inLand.Length).ToArray();
        var randomArray4 = Enumerable.Range(0, randomOb_inLand.Length).ToArray();
        var shuffle3 = randomArray3.OrderBy(x => random.Next()).ToArray();
        var shuffle4 = randomArray4.OrderBy(x => random.Next()).ToArray();
        while (plus < 2)
        {
            if (plus == 3)
            {
                wrongOb[shuffle5[plus]].transform.position = randomPosition_inLand[shuffle3[plus]].transform.position;
                wrongOb[shuffle5[plus]].transform.rotation = randomPosition_inLand[shuffle3[plus]].transform.rotation;
                wrongOb[shuffle5[plus]].SetActive(true);
            }
            else
            {
                randomOb_inLand[shuffle4[plus]].transform.position = randomPosition_inLand[shuffle3[plus]].transform.position;
                randomOb_inLand[shuffle4[plus]].transform.rotation = randomPosition_inLand[shuffle3[plus]].transform.rotation;
                randomOb_inLand[shuffle4[plus]].SetActive(true);
            }
            plus++;
        }
    }

}
