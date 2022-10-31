using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 뽑기 상품 배치 및 관리 컨트롤러
public class ClawProductController : MonoBehaviour
{
    #region 변수
    [SerializeField]
    Transform basket;
    [Header("뽑기 목록")]
    [SerializeField]
    List<GameObject> Products = new List<GameObject>();

    //상품 갯수
    [SerializeField]
    int ProductNum;
    [SerializeField]
    Transform magenticRoot;

    [SerializeField]
    ClawController GameBox;

    [Header("뽑기 상품 배치 위치 조정")]
    [SerializeField]
    Vector3 center;
    [SerializeField]
    Vector3 size;
    #endregion

    #region

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ProductSetting();
        }
    }
    // 상품 배치
    public void ProductSetting()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 2, size.z / 2));

        int index = Random.Range(0, Products.Count);

        GameObject product = Instantiate(Products[index], pos, Quaternion.identity, basket);
        product.transform.LookAt(center);


        product.name = "생성" +  ProductNum.ToString();
        product.GetComponent<ClawProductHandler>().PrefabNumber = index;
        ProductNum++;
    }

    // 떨어진 상품을 리셋하여 새로운 상품으로 생성하기
    public void ResetDropProducts(Transform Target, int num)
    {
        Target.gameObject.GetComponent<Collider>().enabled = false;
        Target.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        GameObject go = Instantiate(Products[num], Target.transform.position, Target.transform.rotation, basket) as GameObject;
        go.GetComponent<ClawProductHandler>().PrefabNumber = num;
        go.name = Target.name;

        Destroy(Target.gameObject);
    }

    // 바닥에 충돌하였을 경우 자석을 올리기
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.root == magenticRoot)
        {
            GameBox.Collide = true;
        }
    }
    #endregion

    //화면에 그림 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(center, size);
    }

    
}
