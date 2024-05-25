using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RandomExtensions;

namespace CollectionExtensions
{
    public static class RandomCollectionExtensions
    {
        public static T RandomElement<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];

        public static T RandomElement<T>(this IList<T> list) => list[Random.Range(0, list.Count)];

        public static T RandomElement<T>(this IEnumerable<T> values) => values.ElementAt(Random.Range(0, values.Count()));

        public static T RandomElement<T>(this T[] arr, System.Random random) => arr[random.Range(0, arr.Length)];

        public static T RandomElement<T>(this IList<T> list, System.Random random) => list[random.Range(0, list.Count)];

        public static T RandomElement<T>(this IEnumerable<T> values, System.Random random) => values.ElementAt(random.Range(0, values.Count()));

        public static IEnumerable<T> RandomDistinctElements<T>(this IEnumerable<T> values, int n) => values.OrderBy(_ => Random.value).Take(n);

        public static IEnumerable<T> RandomDistinctElements<T>(this IEnumerable<T> values, int n, System.Random random) => values.OrderBy(_ => random.NextSingle()).Take(n);

        public static IEnumerable<T> RandomElements<T>(this T[] arr, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return arr.RandomElement();
            }
        }

        public static int[] RandomIndicesOf<T>(this T[] arr)
        {
            int[] idx = new int[arr.Length];
            for (int i = 0; i < idx.Length; i++)
                idx[i] = i;
            return idx.Shuffle();
        }

        public static int[] RandomIndicesOf<T>(this T[] arr, System.Random random)
        {
            int[] idx = new int[arr.Length];
            for (int i = 0; i < idx.Length; i++)
                idx[i] = i;
            return idx.Shuffle(random);
        }

        public static T[] Shuffle<T>(this T[] arr)
        {
            for (int i = arr.Length - 1; i >= 1; i--)
            {
                int j = Random.Range(0, i + 1);
                T swap = arr[i];
                arr[i] = arr[j];
                arr[j] = swap;
            }
            return arr;
        }

        public static T[] Shuffle<T>(this T[] arr, System.Random random)
        {
            for (int i = arr.Length - 1; i >= 1; i--)
            {
                int j = random.Range(0, i + 1);
                T swap = arr[i];
                arr[i] = arr[j];
                arr[j] = swap;
            }
            return arr;
        }

        public static T[] ShuffledCopy<T>(this T[] arr)
        {
            T[] newArr = (T[])arr.Clone();
            return newArr.Shuffle();
        }

        public static T[] ShuffledCopy<T>(this T[] arr, System.Random random)
        {
            T[] newArr = (T[])arr.Clone();
            return newArr.Shuffle();
        }

        public static string ToCommaSeparatedString(this IEnumerable<string> values)
        {
            var output = "";
            foreach (var value in values)
            {
                if (output == "")
                    output = value;
                else
                    output = $"{output}, {value}";
            }
            return output;
        }
    }

    public static class ArrayExtensions
    {
        public static void Resize<T>(this T[] arr, int n) => System.Array.Resize(ref arr, n);

    }
}
