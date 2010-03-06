using System;
using System.Collections.Generic;

namespace EventSourcing
{
    internal static class Extensions
    {
        public static void ForEach<TItem>(this IEnumerable<TItem> enumerable, Action<TItem> action)
        {
            foreach(var item in enumerable)
            {
                action(item);
            }
        }
    }
}