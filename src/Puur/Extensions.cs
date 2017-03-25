using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puur
{
    using System.Collections.Concurrent;
    using System.Threading;

    public static class DictionaryExtensions
    {
        public static TDictionary Merge<TDictionary, TKey, TValue>(this TDictionary dic, IDictionary<TKey, TValue> other)
            where TDictionary : IDictionary<TKey, TValue>
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            if (other == null) throw new ArgumentNullException(nameof(other));

            foreach (var pair in other)
                dic[pair.Key] = pair.Value;

            return dic;
        }
    }

    public static class ConcurrentDictionaryExtensions
    {
        public static TValue LazyGetOrAdd<TKey, TValue>(
            this ConcurrentDictionary<TKey, Lazy<TValue>> dictionary,
            TKey key,
            Func<TKey, TValue> valueFactory) => dictionary
            .GetOrAdd(key, new Lazy<TValue>(() => valueFactory(key), LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;

        public static TValue LazyAddOrUpdate<TKey, TValue>(
            this ConcurrentDictionary<TKey, Lazy<TValue>> dictionary,
            TKey key,
            Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory) => dictionary
            .AddOrUpdate(
                key,
                new Lazy<TValue>(() => addValueFactory(key)),
                (currentKey, currentValue) => new Lazy<TValue>(() => updateValueFactory(currentKey, currentValue.Value), LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    public static class EnumerableTaskExtensions
    {
        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
    }

    public static class ObjectExtensions
    {
        public static T As<T>(this object obj) => (T)obj;
    }
}
