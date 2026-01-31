using FluentAssertions;
using OpenTUI.Reactive.Primitives;

namespace OpenTUI.Reactive.Tests.Primitives;

public class StateTests
{
    [Fact]
    public void Constructor_SetsInitialValue()
    {
        var state = new State<int>(42);
        state.Value.Should().Be(42);
    }

    [Fact]
    public void Get_ReturnsCurrentValue()
    {
        var state = new State<string>("hello");
        state.Get().Should().Be("hello");
    }

    [Fact]
    public void Set_UpdatesValue()
    {
        var state = new State<int>(1);
        state.Set(2);
        state.Value.Should().Be(2);
    }

    [Fact]
    public void Value_Setter_UpdatesValue()
    {
        var state = new State<int>(1);
        state.Value = 5;
        state.Value.Should().Be(5);
    }

    [Fact]
    public void Update_TransformsValue()
    {
        var state = new State<int>(10);
        state.Update(x => x * 2);
        state.Value.Should().Be(20);
    }

    [Fact]
    public void Set_RaisesChangedEvent()
    {
        var state = new State<int>(1);
        int? received = null;
        state.Changed += (_, v) => received = v;
        
        state.Set(42);
        
        received.Should().Be(42);
    }

    [Fact]
    public void Set_SameValue_DoesNotRaiseEvent()
    {
        var state = new State<int>(5);
        var eventCount = 0;
        state.Changed += (_, _) => eventCount++;
        
        state.Set(5);
        
        eventCount.Should().Be(0);
    }

    [Fact]
    public void Subscribe_ReceivesUpdates()
    {
        var state = new State<int>(0);
        var values = new List<int>();
        state.Subscribe(v => values.Add(v));
        
        state.Set(1);
        state.Set(2);
        state.Set(3);
        
        values.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Subscribe_Dispose_StopsReceivingUpdates()
    {
        var state = new State<int>(0);
        var values = new List<int>();
        var subscription = state.Subscribe(v => values.Add(v));
        
        state.Set(1);
        subscription.Dispose();
        state.Set(2);
        
        values.Should().Equal(1);
    }

    [Fact]
    public void ImplicitConversion_ReturnsValue()
    {
        var state = new State<int>(42);
        int value = state;
        value.Should().Be(42);
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        var state = new State<int>(123);
        state.ToString().Should().Be("123");
    }

    [Fact]
    public void ToString_NullValue_ReturnsNull()
    {
        var state = new State<string?>(null);
        state.ToString().Should().Be("null");
    }

    [Fact]
    public void CustomComparer_UsedForEquality()
    {
        var state = new State<string>("Hello");
        var eventCount = 0;
        state.Changed += (_, _) => eventCount++;
        
        // Same value should not trigger
        state.Set("Hello");
        eventCount.Should().Be(0);
        
        // Different value should trigger
        state.Set("World");
        eventCount.Should().Be(1);
    }

    [Fact]
    public void StateFactory_CreatesState()
    {
        var state = State.Create(100);
        state.Value.Should().Be(100);
    }

    [Fact]
    public void MultipleSubscribers_AllReceiveUpdates()
    {
        var state = new State<int>(0);
        var values1 = new List<int>();
        var values2 = new List<int>();
        
        state.Subscribe(v => values1.Add(v));
        state.Subscribe(v => values2.Add(v));
        
        state.Set(1);
        
        values1.Should().Equal(1);
        values2.Should().Equal(1);
    }
}
