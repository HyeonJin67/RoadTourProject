using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class ButtonAct : MonoBehaviour
{
    Button button;

    public event Action FuncBasket;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonAction);
    }

    void ButtonAction()
    {
        FuncBasket?.Invoke();
    }
}


