using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageParam : MonoBehaviour
{
    [SerializeField]
    string SceneName;
    SceneChanger changer;

    private void Start()
    {
        if (SceneName.Equals("Re"))
        {
            // 씬 이름에 Re를 입력할 경우 현제 활성화된 씬으로 이동 = 새로고침
            SceneName = SceneManager.GetActiveScene().name;            
        }
        changer = FindObjectOfType<SceneChanger>();
        GetComponent<ButtonAct>().FuncBasket += StageScene;
    }


    void StageScene()
    {
        changer.ChangeScene(SceneName);
    }
}
