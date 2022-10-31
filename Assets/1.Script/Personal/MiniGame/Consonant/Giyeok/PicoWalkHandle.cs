using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PicoWalkHandle : MonoBehaviour
{
    [Tooltip("피코가 이동할곳의 오브젝트를 넣어주세요")]
    [SerializeField]
    GameObject picoPosition; //피코가 이동할 곳
    private Animator animator;
    int[] clickArray = { 6, 7, 8, 9, 10 }; //사용자가 피코를 클릭할 때마다 바뀌게 될 애니메이션을 위한 인덱스 보관용 배열
    System.Random random = new System.Random();

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("PicoAction", 5);
        StartCoroutine(Speed_forPico());
    }
    //씬 시작시에 피코가 걸어서 씬에 천천히 들어오게끔 해주는 지연 함수
    IEnumerator Speed_forPico()
    {
        while (Vector3.Distance(transform.position, picoPosition.transform.position) > 0.2f) //둘사이의 거리가 있는 동안 //첨에 0으로 했다가 너무 느려서 10으로 바꿈
        {
            transform.position = Vector3.Lerp(transform.position, picoPosition.transform.position, Time.deltaTime * 0.7f);
            yield return new WaitForSeconds(Time.deltaTime);
            if (Vector3.Distance(transform.position, picoPosition.transform.position) <= 0.2f)
            {
                break;
            }
        }
        transform.position = picoPosition.transform.position;
        transform.Rotate(0, -70, 0);
        animator.SetInteger("PicoAction", 4); //피코애니메이션 중에 4번 두리번두리번켜기
        yield break;
    }
    //피코를 클릭할때마다 랜덤으로 애니메이션을 바꿔주는 함수
    private void OnMouseDown()
    {
        int[] shuffle = clickArray.OrderBy(x => random.Next()).ToArray();
        animator.SetInteger("PicoAction", shuffle[0]);
    }
}
