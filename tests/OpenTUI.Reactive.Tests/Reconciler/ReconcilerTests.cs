using FluentAssertions;
using OpenTUI.Core.Renderables;
using Reconciler = OpenTUI.Reactive.Reconciler.Reconciler;

namespace OpenTUI.Reactive.Tests.Reconciler;

using OpenTUI.Reactive.Reconciler;

public class ReconcilerTests
{
    [Fact]
    public void Render_CreatesRenderable()
    {
        var reconciler = new Reconciler();
        var node = VNodes.Element<TextRenderable>(t => t.Text = "Hello");
        
        var result = reconciler.Render(node);
        
        result.Should().NotBeNull();
        result.Should().BeOfType<TextRenderable>();
        ((TextRenderable)result).Text.Should().Be("Hello");
    }

    [Fact]
    public void Render_SetsRoot()
    {
        var reconciler = new Reconciler();
        var node = VNodes.Element<TextRenderable>();
        
        reconciler.Render(node);
        
        reconciler.Root.Should().NotBeNull();
    }

    [Fact]
    public void Reconcile_ReturnsPatches()
    {
        var reconciler = new Reconciler();
        var oldNode = VNodes.Element<TextRenderable>(t => t.Text = "Old");
        var newNode = VNodes.Element<TextRenderable>(t => t.Text = "New");
        
        reconciler.Render(oldNode);
        var patches = reconciler.Reconcile(newNode);
        
        patches.Should().NotBeEmpty();
    }

    [Fact]
    public void Reconcile_UpdatesExistingElement()
    {
        var reconciler = new Reconciler();
        var oldNode = VNodes.Element<TextRenderable>(t => t.Text = "Old");
        var newNode = VNodes.Element<TextRenderable>(t => t.Text = "New");
        
        reconciler.Render(oldNode);
        reconciler.Reconcile(newNode);
        
        var text = reconciler.Root as TextRenderable;
        text!.Text.Should().Be("New");
    }

    [Fact]
    public void Reconcile_ReplacesRoot_WhenTypeChanges()
    {
        var reconciler = new Reconciler();
        var oldNode = VNodes.Element<TextRenderable>();
        var newNode = VNodes.Element<BoxRenderable>();
        
        reconciler.Render(oldNode);
        reconciler.Reconcile(newNode);
        
        reconciler.Root.Should().BeOfType<BoxRenderable>();
    }

    [Fact]
    public void Reconcile_Fragment_CreatesGroup()
    {
        var reconciler = new Reconciler();
        var node = VNodes.Fragment(
            VNodes.Element<TextRenderable>(t => t.Text = "One"),
            VNodes.Element<TextRenderable>(t => t.Text = "Two")
        );
        
        var result = reconciler.Render(node);
        
        result.Should().BeOfType<GroupRenderable>();
        result.Children.Should().HaveCount(2);
    }

    [Fact]
    public void VNodes_ElementFactory_Works()
    {
        var node = VNodes.Element<BoxRenderable>(b => 
        {
            b.Title = "Test Box";
            b.BorderStyle = BorderStyle.Double;
        });
        
        var renderable = node.CreateRenderable() as BoxRenderable;
        
        renderable!.Title.Should().Be("Test Box");
        renderable.BorderStyle.Should().Be(BorderStyle.Double);
    }

    [Fact]
    public void VNodes_ElementWithFactory_Works()
    {
        var node = VNodes.Element(() => new TextRenderable { Text = "Factory" });
        
        var renderable = node.CreateRenderable() as TextRenderable;
        
        renderable!.Text.Should().Be("Factory");
    }
}
