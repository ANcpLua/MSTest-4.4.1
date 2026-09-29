using System.Collections;

namespace MSTest.Assertions.Counting;

/// <summary>
/// A non-generic collection that knows its count and records every enumeration.
/// </summary>
internal sealed class Shelf(int books) : ICollection
{
    public int Enumerations { get; private set; }

    public int Count => books;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void CopyTo(Array array, int index) => throw new NotSupportedException();

    public IEnumerator GetEnumerator()
    {
        Enumerations++;
        return Enumerable.Range(1, books).GetEnumerator();
    }
}
