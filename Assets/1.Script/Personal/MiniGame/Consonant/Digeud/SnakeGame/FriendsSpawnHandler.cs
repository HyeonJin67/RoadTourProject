using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 뱀게임 목표물 스포너
public class FriendsSpawnHandler : MonoBehaviour
{
    #region 변수
    [SerializeField]
    GameObject[] Friends;
    [SerializeField]
    GameObject Particle;
    public Vector3 center;
    public Vector3 size;

    Quaternion min;
    #endregion
    private void Start()
    {
        FriendSpawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            FriendSpawn();
        }
    }
    // 오브젝트 스폰
    public void FriendSpawn()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2) , Random.Range(-size.y / 2, size.y / 2) , Random.Range(-size.z / 2, size.z / 2));

        int index = Random.Range(0, Friends.Length);

        GameObject friend = Instantiate(Friends[index], pos, Quaternion.identity);
        friend.transform.LookAt(center);
        var par = Instantiate(Particle, pos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawCube(center, size);
    }
}
