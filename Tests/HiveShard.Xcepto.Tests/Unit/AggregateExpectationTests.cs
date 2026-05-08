using HiveShard.Xcepto.Tests.Data;
using HiveShard.Xcepto.Tests.Shards;
using Xcepto.HiveShard.Builders;

namespace HiveShard.Xcepto.Tests.Unit;

[TestFixture]
public class AggregateExpectationTests
{
    readonly IEnumerable<Person> _people =
    [
        new(21),
        new(35),
        new(41),
        new(57),
    ];
    
    [Test]
    public void NoneAreEqualToSucceeds()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .None(Are.EqualTo(21));
        
        expectation.Evaluate(_people);
    }
    
    [Test]
    public void NoneAreEqualToFails()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .None(Are.EqualTo(41));
        
        Assert.That(() => expectation.Evaluate(_people), Throws.Exception);
    }
    
    [Test]
    public void AnyAreEqualToSucceeds()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .Any(Are.EqualTo(41));
        
        expectation.Evaluate(_people);
    }
    
    [Test]
    public void AnyAreGreaterThanSucceeds()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .Any(Are.GreaterThan(50));
        
        expectation.Evaluate(_people);
    }
    
    [Test]
    public void AnyAreLessThanFails()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .Any(Are.LessThan(40));
        Assert.That(() => expectation.Evaluate(_people), Throws.Exception);
    }
    
    [Test]
    public void AllAreGreaterThanSucceeds()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .All(Are.GreaterThan(40));
        expectation.Evaluate(_people);
    }
    
    [Test]
    public void AllAreGreaterThanFails()
    {
        var expectation = AggregateExpectation.For<Person>()
            .Where(x => x.Age > 40)
            .Drill(x => x.Age)
            .All(Are.GreaterThan(50));
        Assert.That(() => expectation.Evaluate(_people), Throws.Exception);
    }
}