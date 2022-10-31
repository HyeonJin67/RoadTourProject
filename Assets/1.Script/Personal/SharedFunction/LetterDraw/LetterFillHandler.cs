using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 글자 체우기
public class LetterFillHandler : MonoBehaviour
{
    public void Fill(int curIndex, int num, int count)
    {   
        float amount = (float)((1.0f / count) * num);
        transform.GetChild(curIndex).GetComponent<Image>().fillAmount = amount;
    }
}
