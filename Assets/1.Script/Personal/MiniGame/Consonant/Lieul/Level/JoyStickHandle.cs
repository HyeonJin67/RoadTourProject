using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//https://luv-n-interest.tistory.com/687
//방향키의 방향을 지정해주는 enum
public enum ArrowDirection
{
    None,
    Left,
    Right,
    Go,
    Back,
}
//방향키 스크립트
public class JoyStickHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip ("방향키로 움직일 대상을 넣어주세요")]
    [SerializeField]
    GameObject ball;
    [Tooltip ("각 방향키별 움직일 방향을 지정해주세요")]
    [SerializeField]
    ArrowDirection arrowDirection;

    Vector3 currentDirection = Vector3.zero;

    Rigidbody ballRigi;
    //방향키 enum값을 주고받는 프로퍼티
    ArrowDirection KeyDirection
    {
        set
        {
            currentDirection = value switch
            {
                ArrowDirection.Go => Vector3.forward,
                ArrowDirection.Back => Vector3.back,
                ArrowDirection.Left => Vector3.left,
                ArrowDirection.Right => Vector3.right,
                _ => Vector3.zero //ArrowDirection.None => Vector3.zero
                //ArrowDirection.Up => ball.transform.forward,
                //ArrowDirection.Down => -ball.transform.forward,
                //ArrowDirection.Left => -ball.transform.right,
                //ArrowDirection.Right => ball.transform.right,
                //_ => Vector3.zero
            };
        }
    }
    void OnEnable() 
    {
        KeyDirection = arrowDirection;
    }
    private void Start()
    {
        ballRigi = ball.GetComponent<Rigidbody>(); 
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        print("엔터"+currentDirection);
        //ballRigi.AddForce(currentDirection * 100); //world좌표기준으로 힘
        ballRigi.AddRelativeForce(currentDirection * 100); //ball기준으로 힘
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        print("스탑");
        //자연스럽게 관성을 받는 버전
        //ballRigi.AddForce(-ballRigi.velocity);
        //ballRigi.AddTorque(-ballRigi.angularVelocity);
        //속도조절 버전
        ballRigi.velocity = Vector3.zero;
        ballRigi.angularVelocity = Vector3.zero;
    }
}
