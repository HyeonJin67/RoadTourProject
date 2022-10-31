using System.Collections.Generic;
using UnityEngine;

//뱀 이동
public class SnakeMovement : MonoBehaviour
{
    #region 변수    
    //몸체 리스트
    [SerializeField]
    List<Transform> BodyParts = new List<Transform>();
    //거리, 속도, 회전속도
    [SerializeField]
    float mindistance, speed, rotationSpeed;
    // 무엇을 추가할 것인가
    [SerializeField]
    GameObject bodyPrefab;
    // 길이
    [SerializeField]
    int beginSize;
    // 시작점과 목표점
    [SerializeField]
    Transform StartPoint, Target;
    // 현재 파츠 이전파츠
    private float dis;
    private Transform curBodyParts;
    private Transform prevBodyParts;

    #endregion

    #region 함수

    private void Start()
    {
        StartLevel();
    }
    // 몇개로 시작할 것인가
    public void StartLevel()
    {
        for (int i = 0; i < beginSize; i++)
        {
            //AddBodyPart();
        }

        BodyParts[0].position = StartPoint.position;
    }    
    private void Update()
    {
        SnakeMove();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RemoveBodyPart();

        }
    }
    // 뱀의 움직임 구현
    public void SnakeMove()
    {
        float curSpeed = speed;

        /*
        BodyParts[0].Translate(BodyParts[0].forward * curSpeed * Time.smoothDeltaTime, Space.World);

        if(Input.GetAxis("Horizontal") != 0)
        {
            BodyParts[0].Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Input.GetAxis("Horizontal"));
        }
        */

        //snakeMove.ChangeEndValue(new Vector3(Target.position.x, BodyParts[0].position.y, Target.transform.position.z), .3f, true).Restart();
        //BodyParts[0].DOLookAt(Target.position, 1);


        for (int i = 1; i < BodyParts.Count; i++)
        {
            curBodyParts = BodyParts[i];
            prevBodyParts = BodyParts[i-1];

            dis = Vector3.Distance(prevBodyParts.position, curBodyParts.position);

            Vector3 newpos = prevBodyParts.position;

            newpos.y = BodyParts[0].position.y;

            float T = Time.deltaTime * dis / mindistance * curSpeed;
            if (T > 0.5f){T = 0.5f;}            

            curBodyParts.position = Vector3.Slerp(curBodyParts.position, newpos, T);
            curBodyParts.rotation = Quaternion.Slerp(curBodyParts.rotation, prevBodyParts.rotation, T);
        }
    }

    // 지정 프리팹으로 몸통 추가
    public void AddBodyPart()
    {
        Transform newpart = ((Instantiate(bodyPrefab, BodyParts[BodyParts.Count-1].position, BodyParts[BodyParts.Count - 1].rotation)) as GameObject).transform;
        newpart.SetParent(transform);

        BodyParts.Add(newpart);
    }
    // 특정 오브젝트로 몸통 추가
    public void AddBodyPart(GameObject obj)
    {
        Transform newpart = ((Instantiate(obj, BodyParts[BodyParts.Count - 1].position, BodyParts[BodyParts.Count - 1].rotation)) as GameObject).transform;
        newpart.SetParent(transform);

        BodyParts.Add(newpart);
    }
    // 몸통 제거
    public void RemoveBodyPart()
    {
        if (BodyParts.Count > 2)
        {
            Transform tt = BodyParts[BodyParts.Count - 1];
            BodyParts.RemoveAt(BodyParts.Count - 1);

            tt.SetParent(null);
            tt.position = new Vector3(Random.Range(-5,5), 1, Random.Range(-5, 5));
        }
    }

    #endregion
}
