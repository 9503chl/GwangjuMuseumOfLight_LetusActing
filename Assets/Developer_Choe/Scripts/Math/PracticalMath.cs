using System;
using System.Numerics;

[Serializable]
public class PracticalMath
{
    /// <summary>
    /// �ִ� ����� ���ϱ�
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static long GetGCD(long first, long second)
    {
        while (second > 0)
        {
            long temp = second;
            second = first % second;
            first = temp;
        }
        return first;
    }

    public static int GetGCD(int a, int b)
    {
        return GetGCD(a, b);
    }

    /// <summary>
    /// �ּ� ����� ���ϱ�
    /// </summary>
    /// <returns></returns>
    public static long GetLCM(long first, long second)
    {
        long GCD = GetGCD(first, second);

        return first * second / GCD;
    }

    public static int GetLCM(int a, int b)
    {
        return GetLCM(a, b);
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="N"></param>
    /// <param name="R"></param>
    /// <returns></returns>
    public static BigInteger Permutation(int N, int R)
    {
        BigInteger Result = 1;

        for (int i = R, j = N; j >= 1; i--, j--)
        {
            Result *= i;
        }

        return Result;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="N"></param>
    /// <param name="R"></param>
    /// <returns></returns>
    public static BigInteger Combitnation(int N, int R)
    {
        BigInteger Result = 1;
        BigInteger Division = 1;

        for (int i = R, j = N; j >= 1; i--, j--)
        {
            Result *= i;
            Division *= j;
        }

        return Result / Division;
    }
}
