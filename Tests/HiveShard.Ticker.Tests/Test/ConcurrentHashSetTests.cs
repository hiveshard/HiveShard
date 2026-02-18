using HiveShard.Ticker.Util;

namespace HiveShard.Ticker.Tests.Test;

[TestFixture]
public class ConcurrentHashSetTests
{
    private ConcurrentHashSet<int> _set;

    [SetUp]
    public void SetUp()
    {
        _set = new ConcurrentHashSet<int>();
    }

    [Test]
    public void Add_NewItem_ReturnsTrue()
    {
        Assert.That(_set.Add(1), Is.True);
        Assert.That(_set.Contains(1), Is.True);
    }

    [Test]
    public void Add_DuplicateItem_ReturnsFalse()
    {
        _set.Add(1);
        Assert.That(_set.Add(1), Is.False);
        Assert.That(_set.Count, Is.EqualTo(1));
    }

    [Test]
    public void Remove_ExistingItem_ReturnsTrue()
    {
        _set.Add(1);
        Assert.That(_set.Remove(1), Is.True);
        Assert.That(_set.Contains(1), Is.False);
    }

    [Test]
    public void Remove_NonExistingItem_ReturnsFalse()
    {
        Assert.That(_set.Remove(42), Is.False);
    }

    [Test]
    public void Contains_NonExistingItem_ReturnsFalse()
    {
        Assert.That(_set.Contains(99), Is.False);
    }

    [Test]
    public void Count_EmptySet_IsZero()
    {
        Assert.That(_set.Count, Is.EqualTo(0));
    }

    [Test]
    public void Concurrent_Add_SameItem_OnlyOneSucceeds()
    {
        var results = ParallelEnumerable.Range(0, 100)
            .Select(_ => _set.Add(1))
            .ToArray();

        Assert.That(results.Count(r => r), Is.EqualTo(1));
        Assert.That(_set.Count, Is.EqualTo(1));
    }

    [Test]
    public void Concurrent_Add_MultipleItems_AllPresent()
    {
        Parallel.For(0, 1000, i => _set.Add(i));

        Assert.That(_set.Count, Is.EqualTo(1000));
        Assert.That(Enumerable.Range(0, 1000).All(i => _set.Contains(i)), Is.True);
    }

    [Test]
    public void Concurrent_Add_And_Remove_NoCorruption()
    {
        Parallel.For(0, 1000, i =>
        {
            _set.Add(i);
            _set.Remove(i);
        });

        Assert.That(_set.Count, Is.EqualTo(0));
    }

    [Test]
    public void Concurrent_Remove_SameItem_OnlyOneSucceeds()
    {
        _set.Add(1);

        var results = ParallelEnumerable.Range(0, 50)
            .Select(_ => _set.Remove(1))
            .ToArray();

        Assert.That(results.Count(r => r), Is.EqualTo(1));
        Assert.That(_set.Count, Is.EqualTo(0));
    }
}
