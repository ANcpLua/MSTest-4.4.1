using System.Collections;

namespace MSTest.Assertions.Counting;

/// <summary>
/// A non-generic sequence without a count: its size is only known after walking it.
/// </summary>
internal sealed class Conveyor(int parcels) : IEnumerable
{
    public int Enumerations { get; private set; }

    public IEnumerator GetEnumerator()
    {
        Enumerations++;
        return Enumerable.Range(1, parcels).GetEnumerator();
    }
}
