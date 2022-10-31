using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 뽑기 상품 핸들러
public class ClawProductHandler : MonoBehaviour
{
    #region 변수
    ClawController clawController;
    ClawProductController product;

    Transform parent;

    public int PrefabNumber;
    [SerializeField]
    Rigidbody magnet;
    #endregion

    private void Start()
    {
        clawController = FindObjectOfType<ClawController>();
        clawController.MagnetPutDown += PutDownObject;

        product = FindObjectOfType<ClawProductController>();

        parent = transform.parent;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌 대상이 스테이지가 아니라면
        if (collision.gameObject.name != "Stage")
        {
            // 조인트가 있을 때
            if (GetComponent<Joint>())
            {                
                if (GetComponent<Joint>().connectedBody == null && collision.transform.parent != parent)
                {
                    transform.SetParent(collision.transform);
                    GetComponent<Joint>().connectedBody = collision.rigidbody;
                    clawController.AddBodyPart(gameObject);
                    gameObject.layer = 0;

                    GetComponent<Joint>().autoConfigureConnectedAnchor = false;
                    //자식 갯수로 판별시 꾸미기 어려움이 있음
                    //GetComponent<Joint>().connectedAnchor = collision.transform.childCount < 3 ? Vector3.down * 0.5f : Vector3.down * 2.1f;
                    
                    GetComponent<Joint>().connectedAnchor = !transform.parent.name.Contains("Magnet") ? Vector3.down * 0.5f : Vector3.down * 2.1f;
                    //자석 올리기 이벤트 호출

                    clawController.Collide = true;
                }
                else if (GetComponent<Joint>().connectedBody != collision.rigidbody)
                {
                    clawController.Collide = true;
                }
            }
            //GetComponent<Joint>(). = 50;
        }
        if (!GetComponent<Joint>())
        {
            if (GetComponent<Rigidbody>().velocity.magnitude < 1 && GetComponent<Rigidbody>().velocity.y >= 0)
            {
                if (collision.transform.GetComponent<ClawProductController>() != null)
                    product.ResetDropProducts(transform, PrefabNumber);
            }
        }
        if (transform.parent != parent)
        {
            clawController.Collide = true;
        }
    }

    // 잡혀있는 오브젝트 떨어트리기
    public void PutDownObject()
    {
        GetComponent<Collider>().isTrigger = true;
        if (transform.parent.GetComponent<Rigidbody>())
        {
            transform.SetParent(null);

            int r = Random.Range(10, 20);
            Vector3 RandomForce = new Vector3(Random.Range(-r, r), -2, Random.Range(-r, r));
            GetComponent<Rigidbody>().AddForce(RandomForce);


            GetComponent<Joint>().breakForce = 0;
            transform.parent = parent;
        }
    }


    // 오브젝트가 사라질 경우 이벤트 제외
    private void OnDestroy()
    {
        clawController.MagnetPutDown -= PutDownObject;
    }


    // 오브젝트가 바닥에 충돌할 경우 트리거를 끄기 
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<ClawProductController>() != null)
        {
            if(GetComponent<Joint>() != null){ print("붙어있음"); return;}
            GetComponent<Collider>().isTrigger = false;
        }
    }
}
