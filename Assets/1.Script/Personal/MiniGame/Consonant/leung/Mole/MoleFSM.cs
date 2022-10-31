using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script 설명 : 두더지의 행동을 제어하는 스크립트
// 지하에 대기 , 지상에 대기 , 지하 -> 지상 이동 , 지상 -> 지하 이동
public enum MoleState {  UnderGround = 0, OnGround , MoveUp, MoveDown}
//두더지 종류 (기본 ,점수 - , 시간 + )
public enum MoleType {  Normal = 0, Red , Blue}
public class MoleFSM : MonoBehaviour
{
    [SerializeField]
    private float waitTimeOnGround;         //지면에 올라와서 내려가기 까지 기다리는 시간
    [SerializeField]
    private float limitMinY;                //내려갈 수 있는 최소 y 위치
    [SerializeField]
    private float limitMaxY;                //올라올 수 있는 최대 Y 위치

    private Movement3D movement3D;          //위/아래 이동을 위한 Movement3D
    private MeshRenderer meshRenderer;      //두더지의 색상 설정을 위한 MeshRenderer

    private MoleType moleType;              //두더지의 종류
    private Color defaultColor;             //기본 두더지의 색상(173,135,24)

    //두더지의 현재 상태 (Set은 MoleFSM 클래스 내부에서만)
    public MoleState MoleState { private set; get; }  //property  
    //두더지의 종류(moleType에 따라 두더지 색상 변경)
    public MoleType MoleType
    {
        set
        {
            moleType = value;
            switch (moleType)
            {
                case MoleType.Normal:
                    meshRenderer.material.color = defaultColor;
                    break;
                    case MoleType.Red:
                    meshRenderer.material.color = Color.red;
                    break;
                case MoleType.Blue:
                    meshRenderer.material.color = Color.blue;
                    break;
            }
        }
        get => moleType;
    }
    private void Awake()
    {
        movement3D = GetComponent<Movement3D>();
        meshRenderer = GetComponent<MeshRenderer>();

        defaultColor = meshRenderer.material.color; //두더지의 최초 색상 저장

        ChangeState(MoleState.UnderGround);//처음 두더지의 상태를 설정 UnderGround(땅속에 대기)
    }
    public void ChangeState(MoleState newState)
    {
        //열거형 변수를 ToString() 메소드를 이용해 문자열로 변환하면
        //"UnderGround"와 같이 열거형 요소 이름 반환

        //이전에 재생중이던 상태 종료 
        StopCoroutine(MoleState.ToString());
        //상태 변경 
        MoleState = newState;
        //새로운 상태 재생
        StartCoroutine(MoleState.ToString());
    }
    /*<summary>
      두더지가 바닥에서 대기하는 상태로 최초에 바닥 위치로 두더지 위치 설정
     </summary>*/
    private IEnumerator UnderGround()
    {
        //이동방향을 : (0,0,0) [정지]
        movement3D.MoveTo(Vector3.zero);
        //두더지의 y위치를 홀 밖으로 나와있는 limitMaxY 위치로 설정
        transform.position = new Vector3(transform.position.x ,limitMinY, transform.position.z);

        yield return null;
    }
    ///<summary>
    ///두더지가 홀 밖으로 나와있는 상태로 waitTimeOnGround동안 대기
    ///</summary>
    private IEnumerator OnGround()
    {
 
        //이동방향을 : (0,0,0) [정지]
        movement3D.MoveTo(Vector3.zero);
        //두더지의 y위치를 홀 밖으로 나와있는 limitMaxY 위치로 설정
        transform.position = new Vector3(transform.position.x , limitMaxY, transform.position.z);

        //waitTImeOnGround 시간 동안 대기
        yield return new WaitForSeconds(waitTimeOnGround);

        //두더지의 상태를 MoveDown으로 변경
        ChangeState(MoleState.MoveDown);
    }
    ///<summary>
    ///두더지가 홀 밖으로 나오는 상태 (maxYPoseOnGround 위치까지 위로 이동)
    ///</summary>
    private IEnumerator MoveUp()
    {
        //이동방향 : (0,1,0) [위]
        movement3D.MoveTo(Vector3.up);

        while(true)
        {
            //두더지의 y위치가 limitMaxY에 도달하면 상태 변경
            if(transform.position.y >= limitMaxY)
            {
                //OnGround 상태로 변경
                ChangeState(MoleState.OnGround);
            }
            yield return null;
        }
    }
    ///<summary>
    ///두더지가 홀로 들어가는 상태 (minYPosUnderGround 위치까지 아래로 이동)
    /// </summary>
   private IEnumerator MoveDown()
    {
        //이동방향 : (0,-1,0) [아래]
        movement3D.MoveTo(Vector3.down);
        while(true)
        {
            //두더지의 y위치가 limitMinY에 도달하면 반복문 중지
            if(transform.position.y <= limitMinY)
            {
                //UnderGround 상태로 변경
                ChangeState(MoleState.UnderGround);
            }
            yield return null;
        }
    }
}
