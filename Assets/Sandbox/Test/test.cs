using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    [SerializeField]
    private int searchRange = 0;

    [SerializeField]
    private Vector2 _offset = new Vector2(0, 0);

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GetDotsInRange(searchRange, _offset);
        }
    }

    private void GetDotsInRange(int range, Vector2 offset, bool hasOriginOffset = false, int originOffsetX = 0, int originOffsetY = 0)
    {
        Debug.Log("探索範囲" + range);
        if (range <= 1)
        {
            Debug.Log("探索終了 & 周囲のピクセル取得");
            return;
        }

        int i = 0;
        int nextSearchRange = 0;
        int tmpRange = 0;

        // TODO:S : ここもっと行数すくなくかけそう
        while (true)
        {
            var pow = (int)Mathf.Pow(3, i);
            nextSearchRange = tmpRange;
            tmpRange += pow;

            if (range > nextSearchRange && range <= tmpRange)
            {
                break;
            }
            i++;
        }

        // 評価点を決定
        int nextEvaluationBase = nextSearchRange * 2 + 1;
        Debug.Log("nextEvaluationBase : " + nextEvaluationBase);


        GetDotsInRange(nextSearchRange, _offset);
    }
}
