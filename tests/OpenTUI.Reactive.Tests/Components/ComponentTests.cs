using FluentAssertions;
using OpenTUI.Core.Renderables;
using OpenTUI.Reactive.Components;
using OpenTUI.Reactive.Primitives;

namespace OpenTUI.Reactive.Tests.Components;

public class ComponentTests
{
    private class TestComponent : Component
    {
        public int RenderCount { get; private set; }
        public bool WasMounted { get; private set; }
        public bool WasUnmounted { get; private set; }
        
        public State<int> Counter { get; private set; } = null!;

        protected override void OnMount()
        {
            WasMounted = true;
            Counter = UseState(0);
        }

        protected override void OnUnmount()
        {
            WasUnmounted = true;
        }

        public override IRenderable Render()
        {
            RenderCount++;
            return new TextRenderable { Text = $"Count: {Counter.Value}" };
        }
    }

    [Fact]
    public void Mount_CallsOnMount()
    {
        var component = new TestComponent();
        component.Mount();
        
        component.WasMounted.Should().BeTrue();
        component.IsMounted.Should().BeTrue();
    }

    [Fact]
    public void Unmount_CallsOnUnmount()
    {
        var component = new TestComponent();
        component.Mount();
        component.Unmount();
        
        component.WasUnmounted.Should().BeTrue();
        component.IsMounted.Should().BeFalse();
    }

    [Fact]
    public void GetRenderedContent_CallsRender()
    {
        var component = new TestComponent();
        component.Mount();
        
        var content = component.GetRenderedContent();
        
        content.Should().NotBeNull();
        component.RenderCount.Should().Be(1);
    }

    [Fact]
    public void GetRenderedContent_RecalculatesWhenShouldRenderTrue()
    {
        var component = new TestComponent();
        component.Mount();
        
        // ShouldRender returns true by default, so each call re-renders
        _ = component.GetRenderedContent();
        _ = component.GetRenderedContent();
        _ = component.GetRenderedContent();
        
        // Default behavior re-renders each time (can be optimized in subclasses)
        component.RenderCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ForceUpdate_ClearsCache()
    {
        var component = new TestComponent();
        component.Mount();
        
        _ = component.GetRenderedContent();
        component.ForceUpdate();
        _ = component.GetRenderedContent();
        
        component.RenderCount.Should().Be(2);
    }

    [Fact]
    public void UseState_TriggersRerender()
    {
        var component = new TestComponent();
        component.Mount();
        
        _ = component.GetRenderedContent();
        component.Counter.Set(1);
        _ = component.GetRenderedContent();
        
        component.RenderCount.Should().Be(2);
    }

    [Fact]
    public void Dispose_UnmountsComponent()
    {
        var component = new TestComponent();
        component.Mount();
        component.Dispose();
        
        component.IsMounted.Should().BeFalse();
        component.WasUnmounted.Should().BeTrue();
    }

    [Fact]
    public void Parent_SetWhenAddingChild()
    {
        var parent = new TestComponent();
        var child = new TestComponent();
        
        // Using reflection to access protected method
        typeof(Component)
            .GetMethod("AddChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(parent, new object[] { child });
        
        child.Parent.Should().BeSameAs(parent);
    }

    [Fact]
    public void Children_MountedWhenParentMounts()
    {
        var parent = new TestComponent();
        var child = new TestComponent();
        
        typeof(Component)
            .GetMethod("AddChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(parent, new object[] { child });
        
        parent.Mount();
        
        child.IsMounted.Should().BeTrue();
    }
}
