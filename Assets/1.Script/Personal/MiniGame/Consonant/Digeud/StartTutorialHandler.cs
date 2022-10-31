using DG.Tweening;
using UnityEngine;


public class StartTutorialHandler : MonoBehaviour
{
    [SerializeField]
    Transform TPoint;

    public void ReadyGame()
    {
        transform.GetComponent<SpriteRenderer>().enabled = true;
        transform.DOMove(TPoint.position, 1f).SetLoops(-1, LoopType.Restart);
        transform.GetComponent<SpriteRenderer>().DOFade(1, 1f).From(0.5f).SetLoops(-1, LoopType.Restart);
    }

    public void StartGame()
    {
        transform.DOComplete();
        transform.DOKill();
        transform.GetComponent<SpriteRenderer>().enabled = false;
    }
}
