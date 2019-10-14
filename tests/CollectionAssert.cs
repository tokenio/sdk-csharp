using System.Collections.Generic;
using Xunit;

namespace Test {
    public static class CollectionAssert {
        public static void Equivalent<T>(IEnumerable<T> c1, IEnumerable<T> c2) {
            var d1 = ToDictionary(c1);
            var d2 = ToDictionary(c2);
            Assert.Equal(d1, d2);
        }

        private static IDictionary<T, int> ToDictionary<T>(IEnumerable<T> collection) {
            var dictionary = new Dictionary<T, int>();
            foreach (var item in collection) {
                if (dictionary.ContainsKey(item)) {
                    dictionary[item]++;
                } else {
                    dictionary[item] = 1;
                }
            }

            return dictionary;
        }
    }
}
