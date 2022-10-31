using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 진행도 저장용 스크립트
public class DecorationHit : MonoBehaviour
{
    [SerializeField]
    int index;
    int count;
    private void OnEnable()
    {
        index = transform.GetSiblingIndex();
        count = transform.parent.childCount;
    }

    public (int, int) GetParam()
    {
        transform.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
        return (index, count);
    }

    public void SetCount(int num)
    {
        count = num;
    }
}
