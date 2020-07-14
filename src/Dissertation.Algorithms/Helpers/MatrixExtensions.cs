using System.Collections.Generic;

namespace Dissertation.Algorithms.Helpers
{
    public static class MatrixExtensions
    {
        public static T[,] To2DArray<T>(this List<T>[] source)
        {
            var array = new T[source.Length, source[0].Count];

            for (int i = 0; i < source.Length; i++)
            {
                for (int j = 0; j < source[i].Count; j++)
                {
                    array[i, j] = source[i][j];
                }
            }

            return array;
        }
    }
}
