using System;
using System.Collections.Generic;

public static class RandomUtils
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
            lock (SyncLock)
            {
                k = Random.Next(n + 1);
            }

            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static void Shuffle<T>(this HashSet<T> hashSet)
    {
        var list = new List<T>(hashSet);
        list.Shuffle();
        hashSet.Clear();
        foreach (var item in list)
        {
            hashSet.Add(item);
        }
    }
    
    public static T GetRandom<T>(this List<T> list)
    {
        lock (SyncLock)
        {
            var index = Random.Next(list.Count);
            return list[index];
        }
    }
    
    public static T GetRandomFrom<T>(params T[] items)
    {
        if (items == null || items.Length < 2)
            throw new ArgumentException("At least two items are required.", nameof(items));

        int index;
        lock (SyncLock)
        {
            index = Random.Next(items.Length);
        }
        
        return items[index];
    }

    public static T GetRandomFromRange<T>(T min, T max) where T : IConvertible
    {
        if (!typeof(T).IsPrimitive || typeof(T) == typeof(bool) || typeof(T) == typeof(char))
            throw new ArgumentException("T must be a numeric type.");

        double minValue = Convert.ToDouble(min);
        double maxValue = Convert.ToDouble(max);

        if (minValue > maxValue)
            throw new ArgumentException("min must be less than or equal to max.");

        double randomValue;

        lock (SyncLock)
        {
            randomValue = Random.NextDouble() * (maxValue - minValue) + minValue;
        }

        return (T)Convert.ChangeType(randomValue, typeof(T));
    }
    
    public static bool GetRandomBool()
    {
        lock (SyncLock)
        {
            return Random.Next(2) == 0;
        }
    }
}