using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HiveShard.Workers.Shard;
using Newtonsoft.Json;

namespace Xcepto.HiveShard.Builders;

public static class AggregateExpectation
{
    public static AggregateExpectationBuilder<T, T> For<T>()
    {
        return new AggregateExpectationBuilder<T, T>(x => x);
    }
}

public class AggregateExpectationBuilder<TRoot, TCurrent>
{
    private readonly Func<IEnumerable<TRoot>, IEnumerable<TCurrent>> _transformation;

    public AggregateExpectationBuilder(Func<IEnumerable<TRoot>, IEnumerable<TCurrent>> transformation)
    {
        _transformation = transformation;
    }

    public AggregateExpectationBuilder<TRoot, TCurrent> Where(Expression<Func<TCurrent, bool>> filter)
    {
        var compiled = filter.Compile();

        return new AggregateExpectationBuilder<TRoot, TCurrent>(
            items => _transformation(items).Where(compiled));
    }

    public AggregateExpectationBuilder<TRoot, TNext> Drill<TNext>(Expression<Func<TCurrent, TNext>> selector)
    {
        var compiled = selector.Compile();

        return new AggregateExpectationBuilder<TRoot, TNext>(
            items => _transformation(items).Select(compiled));
    }

    public AggregateExpectation<TRoot> All(IExpectation<TCurrent> expectation)
    {
        return new AggregateExpectation<TRoot>(items =>
        {
            var transformation = _transformation(items).ToArray();
            foreach (var current in transformation)
            {
                if (!expectation.Evaluate(current))
                    return AggregateExpectationResult.Fail(transformation.Cast<object>(), "All are " + expectation.Describe());
            }

            return AggregateExpectationResult.Pass();
        });
    }

    public AggregateExpectation<TRoot> Any(IExpectation<TCurrent> expectation)
    {
        return new AggregateExpectation<TRoot>(items =>
        {
            TCurrent? lastFailure = default;
            var transformation = _transformation(items).ToArray();
            foreach (var current in transformation)
            {
                if (expectation.Evaluate(current))
                    return AggregateExpectationResult.Pass();
                lastFailure = current;
            }
            return AggregateExpectationResult.Fail(transformation.Cast<object>(), "Any are " + expectation.Describe());
        });
    }

    public AggregateExpectation<TRoot> None(IExpectation<TCurrent> expectation)
    {
        return new AggregateExpectation<TRoot>(items =>
        {
            var transformation = _transformation(items).ToArray();
            foreach (var current in transformation)
            {
                if (expectation.Evaluate(current))
                    return AggregateExpectationResult.Fail(transformation.Cast<object>(), "None are " + expectation.Describe());
            }
            return AggregateExpectationResult.Pass();
        });
    }
}

public class AggregateExpectationResult
{
    public bool Passed { get; }
    public IEnumerable<object> All { get; }
    public string? Description { get; }

    private AggregateExpectationResult(bool passed, IEnumerable<object> all, string? description)
    {
        All = all;
        Description = description;
        Passed = passed;
    }
    public static AggregateExpectationResult Fail(IEnumerable<object> all, string describe)
    {
        return new AggregateExpectationResult(false, all, describe);
    }
    
    public static AggregateExpectationResult Pass()
    {
        return new AggregateExpectationResult(true, [], null);
    }
}

public class AggregateExpectation<TRoot>
{
    private readonly Func<IEnumerable<TRoot>, AggregateExpectationResult> _evaluation;

    public AggregateExpectation(Func<IEnumerable<TRoot>, AggregateExpectationResult> evaluation)
    {
        _evaluation = evaluation;
    }

    public bool Evaluate(IEnumerable<TRoot> items)
    {
        var aggregateExpectationResult = _evaluation(items);
        if (!aggregateExpectationResult.Passed)
            throw new Exception($"Expected: {aggregateExpectationResult.Description}\nBut was: {JsonConvert.SerializeObject(aggregateExpectationResult.All)}");
        return true;
    }
}

public interface IExpectation<in T>: IExpectation
{
    bool Evaluate(T value);
}

public interface IExpectation
{
    string Describe();
}

public sealed class EqualTo<T> : IExpectation<T>
{
    public T Expected { get; }

    public EqualTo(T expected)
    {
        Expected = expected;
    }

    public bool Evaluate(T value)
    {
        return Equals(value, Expected);
    }

    public string Describe()
    {
        return $"equal to {Expected}";
    }
}

public sealed class GreaterThan<T>
    : IExpectation<T>
    where T : IComparable<T>
{
    public T Expected { get; }

    public GreaterThan(T expected)
    {
        Expected = expected;
    }

    public bool Evaluate(T value)
    {
        return value.CompareTo(Expected) > 0;
    }

    public string Describe()
    {
        return $"greater than {Expected}";
    }
}

public sealed class LessThan<T>
    : IExpectation<T>
    where T : IComparable<T>
{
    private readonly T _expected;

    public LessThan(T expected)
    {
        _expected = expected;
    }

    public bool Evaluate(T value)
    {
        return value.CompareTo(_expected) < 0;
    }

    public string Describe()
    {
        return $"less than {_expected}";
    }
}

public static class Are
{
    public static EqualTo<T> EqualTo<T>(T expected) => new(expected);

    public static GreaterThan<T> GreaterThan<T>(T expected)
        where T : IComparable<T> => new(expected); 
    
    public static LessThan<T> LessThan<T>(T expected)
        where T : IComparable<T> => new(expected); 
}


public class test
{
    public test()
    {
        var aggregateExpectation = AggregateExpectation
            .For<ShardWorker>()
            .Where(x => x.ManagedShards.Any())
            .Drill(x => x.GetType())
            .Drill(x =>x.FullName!.Length)
            .All(Are.GreaterThan(5));

        aggregateExpectation.Evaluate([]);
    }
}