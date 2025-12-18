using System.Collections.Generic;
using System.Linq;

namespace HdrManager.Extension
{
    /// <summary>
    /// Provides extension methods for working with enumerable sequences.
    /// </summary>
    /// <remarks>This class contains utility methods that extend the functionality of types implementing <see
    /// cref="IEnumerable{T}"/>. These methods are intended to simplify common operations and improve code readability
    /// when working with collections.</remarks>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the specified sequence, or an empty sequence if the input is null.
        /// </summary>
        /// <remarks>This method is useful for avoiding null reference exceptions when enumerating
        /// potentially null sequences. The returned sequence is never null.</remarks>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="enumerable">The sequence to return, or null to return an empty sequence.</param>
        /// <returns>The original sequence if it is not null; otherwise, an empty sequence of type T.</returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }
    }
}
