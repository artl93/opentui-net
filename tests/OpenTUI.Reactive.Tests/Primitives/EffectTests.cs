using FluentAssertions;
using OpenTUI.Reactive.Primitives;

namespace OpenTUI.Reactive.Tests.Primitives;

public class EffectTests
{
    [Fact]
    public void Run_ExecutesEffect()
    {
        var executed = false;
        var effect = new Effect(() => executed = true);
        
        effect.Run();
        
        executed.Should().BeTrue();
    }

    [Fact]
    public void RunNow_ExecutesAndReturnsSelf()
    {
        var executed = false;
        var effect = new Effect(() => executed = true).RunNow();
        
        executed.Should().BeTrue();
        effect.Should().NotBeNull();
    }

    [Fact]
    public void DependsOn_RunsWhenStateChanges()
    {
        var state = new State<int>(0);
        var runCount = 0;
        var effect = new Effect(() => runCount++)
            .DependsOn(state);
        
        state.Set(1);
        state.Set(2);
        
        runCount.Should().Be(2);
    }

    [Fact]
    public void Cleanup_RunsBeforeNextEffect()
    {
        var state = new State<int>(0);
        var log = new List<string>();
        
        var effect = new Effect(
            () => log.Add("effect"),
            () => log.Add("cleanup")
        ).DependsOn(state);
        
        effect.Run(); // First run
        state.Set(1); // Triggers cleanup + effect
        
        log.Should().Equal("effect", "cleanup", "effect");
    }

    [Fact]
    public void Dispose_RunsCleanup()
    {
        var cleanedUp = false;
        var effect = new Effect(
            () => { },
            () => cleanedUp = true
        ).RunNow();
        
        effect.Dispose();
        
        cleanedUp.Should().BeTrue();
    }

    [Fact]
    public void Dispose_StopsRespondingToChanges()
    {
        var state = new State<int>(0);
        var runCount = 0;
        var effect = new Effect(() => runCount++)
            .DependsOn(state);
        
        state.Set(1);
        effect.Dispose();
        state.Set(2);
        
        runCount.Should().Be(1);
    }

    [Fact]
    public void EffectsFactory_RunsImmediately()
    {
        var executed = false;
        Effects.Run(() => executed = true);
        
        executed.Should().BeTrue();
    }

    [Fact]
    public void EffectsFactory_Create_DoesNotRunImmediately()
    {
        var executed = false;
        var effect = Effects.Create(() => executed = true);
        
        executed.Should().BeFalse();
        
        effect.Run();
        executed.Should().BeTrue();
    }

    [Fact]
    public void MultipleDependencies_Work()
    {
        var a = new State<int>(0);
        var b = new State<int>(0);
        var runCount = 0;
        
        var effect = new Effect(() => runCount++)
            .DependsOn(a)
            .DependsOn(b);
        
        a.Set(1);
        b.Set(1);
        
        runCount.Should().Be(2);
    }

    [Fact]
    public void Dispose_MultipleTimes_IsIdempotent()
    {
        var cleanupCount = 0;
        var effect = new Effect(
            () => { },
            () => cleanupCount++
        ).RunNow();
        
        effect.Dispose();
        effect.Dispose();
        effect.Dispose();
        
        cleanupCount.Should().Be(1);
    }

    [Fact]
    public void Run_AfterDispose_DoesNothing()
    {
        var runCount = 0;
        var effect = new Effect(() => runCount++);
        
        effect.Dispose();
        effect.Run();
        
        runCount.Should().Be(0);
    }
}
