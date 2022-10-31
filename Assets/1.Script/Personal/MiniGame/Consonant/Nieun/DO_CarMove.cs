using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class DO_CarMove : MonoBehaviour
{
    [SerializeField]
    GameObject Car;
    [SerializeField]
    GameObject Tiller;//경운기
    [SerializeField]
    GameObject Windmill;//풍차

    public float turnSpeed = 10;
    private void Start()
    {
        //자동차 경운기 이동 
        Car.transform.DOLocalMoveZ(200, 10).SetLoops(-1, LoopType.Restart);
        Tiller.transform.DOLocalMoveX(100, 10).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Restart);
    }
    private void Update()
    {
        //풍차 회전 
        Windmill.transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime);
    }
}
