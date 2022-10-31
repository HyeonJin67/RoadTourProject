using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColor : MonoBehaviour
{
    public Image image;
    public Slider slider;
    

    public void OnEdit()
    {
        Color color = new Color(0,0,0, slider.value);  
        
        image.color = color;
    }

}
