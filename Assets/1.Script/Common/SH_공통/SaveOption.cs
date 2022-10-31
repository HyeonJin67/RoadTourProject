using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

//어느 형태로 된 파일을 저장할 것인가?? : Json을 이용하여 저장할 때 주의 ....
[System.Serializable]
public class SaveFileFormat
{
    public float volume; 
    public Color color;
}


public class SaveOption : MonoBehaviour
{
    string folderPath = "";
    string fileName = "";

    [SerializeField]
    GameObject audioSource;
    [SerializeField]
    Image image;
    private void Awake()
    {
        folderPath = Application.dataPath +"/";
        fileName = "han_setting.json";
        audioSource = GameObject.Find("Audio Source");/*FindObjectOfType<AudioSource>();*/
    }

    //저장할 함수


    public void SaveFile()
    {
        //저장할 정보를 임의로 생성 : 값 불러오기 등을 통해 저장할 파일 정보를 갱신
        SaveFileFormat savefileFormat = new SaveFileFormat();
        savefileFormat.volume = audioSource.GetComponent<AudioSource>().volume;
        savefileFormat.color = image.color;

        var saveData = JsonUtility.ToJson(savefileFormat);

        File.WriteAllText(folderPath + fileName, saveData);
        print("저장 완료 ");
    }

    private void OnEnable()
    {
        SaveFile();
    }
}
