using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class Setting : MonoBehaviour
{
    void Start()
    {
        GetComponent<ButtonAct>().FuncBasket += Set;
    }
    void Set()
    {

        //SceneManager.LoadScene("º≥¡§ æ¿ ¿Ã∏ß");
    }
}
