using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 라인 데코레이션 조절
public class DecorationHandler : MonoBehaviour
{
    private void Start()
    {
        ClearDeco();
    }

    // 라인 초기화
    public void ClearDeco()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    // 라인 설정
    public void SetLine(int num)
    {
        ClearDeco();
        transform.GetChild(num).gameObject.SetActive(true);
    }
}
