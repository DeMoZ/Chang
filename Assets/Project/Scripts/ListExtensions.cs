using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static readonly object SyncLock = new();
    private static readonly Random Random = new();

    public static void Shuffle<T>(this List<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            int k;
            lock (SyncLock) // Гарантируем потокобезопасность
            {
                k = Random.Next(n + 1);
            }

            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}