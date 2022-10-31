using System.Collections.Generic;
using System.Linq;

public static class RandomNumberPicker
{
    // maxNum 까지의 숫자중 특정 숫자들을 제외한 랜덤값.
    public static int GetRandomNumberByArray(int[] exception, int maxNum)
    {
        var exclude = new HashSet<int>() { };
        foreach (int i in exception)
        {
            exclude.Add(i);
        }
        var range = Enumerable.Range(0, maxNum).Where(i => !exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(0, maxNum - exclude.Count);
        return range.ElementAt(index);
    }
    // maxNum 까지의 숫자중 특정 숫자 1개를 제외한 랜덤값.
    public static int GetRandomNumberByNum(int num, int maxNum)
    {
        var exclude = new HashSet<int>() { };
        exclude.Add(num);
        var range = Enumerable.Range(0, maxNum).Where(i => !exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(0, maxNum - exclude.Count);
        return range.ElementAt(index);
    }
}
