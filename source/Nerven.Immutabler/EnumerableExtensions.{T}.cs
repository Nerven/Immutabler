using System.Collections.Generic;
using System.Linq;

namespace Nerven.Immutabler
{
    public static class EnumerableExtensions
    {
        public static T OneOrDefault<T>(this IEnumerable<T> collection)
        {
            var _enumerator = collection.GetEnumerator();

            if (!_enumerator.MoveNext())
            {
                return default(T);
            }

            var _item = _enumerator.Current;

            if (_enumerator.MoveNext())
            {
                return default(T);
            }

            return _item;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, params T[] items)
        {
            return collection.Concat(items);
        }
    }
}
