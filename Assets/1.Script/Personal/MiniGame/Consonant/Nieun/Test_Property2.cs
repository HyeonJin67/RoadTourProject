using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cube2
{
    private int currentHP;
    public int CurrenHP
    {
        get => currentHP;

        set => currentHP = value;
    }
}

public class Test_Property2 : MonoBehaviour
{
    public static Action Num;
    Cube2 cube2 = new Cube2();
    private void OnMouseDown()
    {
     /*   print(cube2.currentHP);*/
    }
    private void Awake()
    {
        Num = () =>
        {
            OnNum();
        };
    }
    public void OnNum()
    {
        cube2.CurrenHP++;
    }
}
