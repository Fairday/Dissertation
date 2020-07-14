using System;
using System.Collections.Generic;

namespace Dissertation.Algorithms.Helpers
{
    public static class ForEachEx
    {
        public static void ForEach<TFirst, TSecond, TThird>(IList<TFirst> firstCollection,
                                                            IList<TSecond> secondCollection,
                                                            IList<TThird> thirdCollection,
                                                            Action<TFirst, TSecond, TThird> toDo)
        {
            if (firstCollection.Count != secondCollection.Count && firstCollection.Count != thirdCollection.Count)
                throw new Exception("Длины коллекций должны быть одинаковы");

            for (int i = 0; i < firstCollection.Count; i++)
            {
                toDo(firstCollection[i], secondCollection[i], thirdCollection[i]);
            }
        }
    }
}
