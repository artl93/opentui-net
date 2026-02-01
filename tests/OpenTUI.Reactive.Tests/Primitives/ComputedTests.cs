using FluentAssertions;
using OpenTUI.Reactive.Primitives;

namespace OpenTUI.Reactive.Tests.Primitives;

public class ComputedTests
{
    [Fact]
    public void Constructor_ComputesInitialValue()
    {
        var computed = new Computed<int>(() => 42);
        computed.Value.Should().Be(42);
    }

    [Fact]
    public void Value_IsCached()
    {
        var computeCount = 0;
        var computed = new Computed<int>(() =>
        {
            computeCount++;
            return 42;
        });

        _ = computed.Value;
        _ = computed.Value;
        _ = computed.Value;

        computeCount.Should().Be(1);
    }

    [Fact]
    public void DependsOn_RecomputesWhenStateChanges()
    {
        var state = new State<int>(10);
        var computed = new Computed<int>(() => state.Value * 2)
            .DependsOn(state);

        computed.Value.Should().Be(20);

        state.Set(15);

        computed.Value.Should().Be(30);
    }

    [Fact]
    public void DependsOn_MultipleDependencies()
    {
        var a = new State<int>(2);
        var b = new State<int>(3);
        var computed = new Computed<int>(() => a.Value + b.Value)
            .DependsOn(a)
            .DependsOn(b);

        computed.Value.Should().Be(5);

        a.Set(10);
        computed.Value.Should().Be(13);

        b.Set(20);
        computed.Value.Should().Be(30);
    }

    [Fact]
    public void Changed_RaisedWhenRecomputed()
    {
        var state = new State<int>(1);
        var computed = new Computed<int>(() => state.Value * 10)
            .DependsOn(state);

        int? received = null;
        computed.Changed += (_, v) => received = v;

        state.Set(5);

        received.Should().Be(50);
    }

    [Fact]
    public void Subscribe_ReceivesUpdates()
    {
        var state = new State<int>(1);
        var computed = new Computed<int>(() => state.Value)
            .DependsOn(state);

        var values = new List<int>();
        computed.Subscribe(v => values.Add(v));

        state.Set(2);
        state.Set(3);

        values.Should().Equal(2, 3);
    }

    [Fact]
    public void Dispose_StopsUpdating()
    {
        var state = new State<int>(1);
        var computed = new Computed<int>(() => state.Value)
            .DependsOn(state);

        var values = new List<int>();
        computed.Subscribe(v => values.Add(v));

        state.Set(2);
        computed.Dispose();
        state.Set(3);

        values.Should().Equal(2);
    }

    [Fact]
    public void ImplicitConversion_ReturnsValue()
    {
        var computed = new Computed<int>(() => 99);
        int value = computed;
        value.Should().Be(99);
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        var computed = new Computed<string>(() => "hello");
        computed.ToString().Should().Be("hello");
    }

    [Fact]
    public void Factory_CreatesComputed()
    {
        var computed = Computed.Create(() => 123);
        computed.Value.Should().Be(123);
    }

    [Fact]
    public void ChainedComputed_Works()
    {
        var state = new State<int>(5);
        var doubled = new Computed<int>(() => state.Value * 2)
            .DependsOn(state);
        var quadrupled = new Computed<int>(() => doubled.Value * 2)
            .DependsOn(state); // Note: depends on state, not doubled

        quadrupled.Value.Should().Be(20);

        state.Set(10);
        doubled.Value.Should().Be(20);
        quadrupled.Value.Should().Be(40);
    }
}
