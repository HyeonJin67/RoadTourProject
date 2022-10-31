using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVolum : MonoBehaviour
{

    public Slider audioSlider;
    public AudioSource audioSource;


    public void AudioControl()
    {
        float sound = audioSlider.value;
        audioSource.volume = audioSlider.value;
        PlayerPrefs.SetFloat("audioSlider", audioSlider.value);
    }


    public void MuteToggle(bool muted)
    {
        if(muted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }
    }


}
