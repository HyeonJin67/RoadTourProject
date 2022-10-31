using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HangeulUtil
{
    #region 상수
    private const int INITIAL_COUNT = 19;
    private const int MEDIAL_COUNT = 21;
    private const int FINAL_COUNT = 28;

    private const int HANGUL_UNICODE_START_INDEX = 0xac00;
    private const int HANGUL_UNICODE_END_INDEX = 0xD7A3;

    private const int INITIAL_START_INDEX = 0x1100;
    private const int MEDIAL_START_INDEX = 0x1161;
    private const int FINAL_START_INDEX = 0x11a7;
    #endregion


    #region 한글 변수
    // 입력된 글자가 한글이 맞는가?
    public static bool IsHangul(char source)
    {
        if (HANGUL_UNICODE_START_INDEX <= source && source <= HANGUL_UNICODE_END_INDEX)
        {
            return true;
        }

        return false;
    }
    // 한글 분리기
    public static char[] DivideHangul(char source)
    {
        char[] elementArray = null;

        if (IsHangul(source))
        {
            int index = source - HANGUL_UNICODE_START_INDEX;

            int initial = INITIAL_START_INDEX + index / (MEDIAL_COUNT * FINAL_COUNT);
            int medial = MEDIAL_START_INDEX + (index % (MEDIAL_COUNT * FINAL_COUNT)) / FINAL_COUNT;
            int final = FINAL_START_INDEX + index % FINAL_COUNT;

            if (final == 4519)
            {
                elementArray = new char[2];

                elementArray[0] = (char)initial;
                elementArray[1] = (char)medial;
            }
            else
            {
                elementArray = new char[3];

                elementArray[0] = (char)initial;
                elementArray[1] = (char)medial;
                elementArray[2] = (char)final;
            }
        }

        return elementArray;
    }
    public static (int, bool) DirAndPiece(char[] source)
    {
        int dir = -1;
        bool piece = false;

        dir = (int)source[1] switch
        {
            >= 4449 and <= 4456 => 0,
            4469 => 0,
            >= 4457 and <= 4468 => 1,
            _ => -1,
        };
        //switch ((int)source[1])
        //{
        //    case >= 4449 and <= 4456:
        //    //case 4449:
        //    //case 4450:
        //    //case 4451:
        //    //case 4452:
        //    //case 4453:
        //    //case 4454:
        //    //case 4455:
        //    //case 4456:
        //    case 4469:
        //        dir = 0;
        //        break;
        //    case >= 4457 and <= 4468:
        //        //case 4457:
        //        //case 4458:
        //        //case 4459:
        //        //case 4460:
        //        //case 4461:
        //        //case 4462:
        //        //case 4463:
        //        //case 4464:
        //        //case 4465:
        //        //case 4466:
        //        //case 4467:
        //        //case 4468:
        //        dir = 1;
        //        break;
        //    default:
        //        dir = -1;
        //        break;
        //}

        if (source.Length > 2) { piece = true; }

        return (dir, piece);
    }
    #endregion

}
