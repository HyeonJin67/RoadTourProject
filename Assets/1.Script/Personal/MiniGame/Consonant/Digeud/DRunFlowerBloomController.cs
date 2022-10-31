using System.Linq;
using UnityEngine;


public class DRunFlowerBloomController : MonoBehaviour
{
    enum Constant
    {
        ㄱ, ㄴ, ㄷ, ㄹ, ㅁ, ㅂ, ㅅ, ㅇ, ㅈ, ㅊ, ㅋ, ㅌ, ㅍ, ㅎ
    }

    #region 변수    
    int[] indexArray;

    [SerializeField]
    GameObject dotoriPre;

    [Header("프리팹 바구니 / 프리팹")]
    [SerializeField]
    Transform pot;
    [SerializeField]
    GameObject flowerPrefab;

    [Header("도토리 대상 / 갯수")]
    [SerializeField]
    Constant targetNum;
    [SerializeField]
    int dCount;
    #endregion


    private void Awake()
    {
        indexArray = new int[transform.childCount];
        var arr = System.Linq.Enumerable.Range(0, transform.childCount);
        indexArray = arr.OrderBy(x => Random.value).ToArray();
        FlowerInit();
    }


    public void FlowerInit()
    {
        for (int i = 0; i < indexArray.Length; i++)
        {
            BloomingFlower(i);
        }
    }

    public void BloomingFlower(int i)
    {
        var flower = Instantiate(flowerPrefab, pot);
        if (i < dCount)
        {
            flower.GetComponent<DRunObjectController>().SettingParam((int)targetNum);
            var dotori = Instantiate(dotoriPre, flower.transform);

        }
        else
        {
            flower.GetComponent<DRunObjectController>().SettingParam(RandomNumberPicker.GetRandomNumberByNum((int)targetNum, 14));
        }
        flower.transform.position = transform.GetChild(indexArray[i]).position + new Vector3(0, 0, -0.15f);
        flower.name = "Flower" + indexArray[i];
        flower.GetComponent<DRunObjectController>().SetPointINdex(i);
    }

    #region 파라미터 함수
    public int GetDotoriCount()
    {
        return dCount;
    }
    public int GetTargetNum()
    {
        return (int)targetNum;
    }

    public int[] GetIndexArray()
    {
        return indexArray;
    }
    #endregion

}