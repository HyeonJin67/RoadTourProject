using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//고래 움직임
public class WhaleController : MonoBehaviour
{

    [SerializeField]
    Transform[] Point;
    public int index;
    int nextIndex;

    private void Start()
    {
        MoveRandom();
    }

    public void MoveRandom()
    {
        nextIndex = Random.Range(0, Point.Length);  

        transform.DOMove(Point[nextIndex].position, 10f).From(transform.position).SetEase(Ease.Linear).OnComplete(() => MoveRandom());
        transform.DOLookAt(Point[nextIndex].position, 7f);
    }
}
