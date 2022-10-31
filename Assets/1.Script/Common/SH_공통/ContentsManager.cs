using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ContentsManager : MonoBehaviour
{
    [SerializeField]
    public Slider AudioVolum;
    public Image IColor;
    float Volum_Data = 0;
    Color Image_Data = Color.black;

    public void SaveData()
    {
        Volum_Data = AudioVolum.value;
        Image_Data = IColor.color;
        print("저장완료");
    }
}
