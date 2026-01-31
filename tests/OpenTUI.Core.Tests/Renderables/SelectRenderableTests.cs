using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class SelectRenderableTests
{
    [Fact]
    public void Constructor_Default_IsFocusable()
    {
        var select = new SelectRenderable();
        
        select.Focusable.Should().BeTrue();
        select.Items.Should().BeEmpty();
        select.SelectedIndex.Should().Be(-1);
    }

    [Fact]
    public void AddItem_AddsToList()
    {
        var select = new SelectRenderable();
        
        select.AddItem("Item 1");
        select.AddItem("Item 2");
        
        select.Items.Should().HaveCount(2);
        select.SelectedIndex.Should().Be(0); // First item selected
    }

    [Fact]
    public void AddItems_AddsMultiple()
    {
        var select = new SelectRenderable();
        
        select.AddItems(["One", "Two", "Three"]);
        
        select.Items.Should().HaveCount(3);
    }

    [Fact]
    public void SetItems_ReplacesExisting()
    {
        var select = new SelectRenderable();
        select.AddItem("Old");
        
        select.SetItems(["New 1", "New 2"]);
        
        select.Items.Should().HaveCount(2);
        select.Items[0].Should().Be("New 1");
    }

    [Fact]
    public void RemoveItem_RemovesFromList()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        
        select.RemoveItem("B").Should().BeTrue();
        
        select.Items.Should().HaveCount(2);
        select.Items.Should().NotContain("B");
    }

    [Fact]
    public void ClearItems_RemovesAll()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        
        select.ClearItems();
        
        select.Items.Should().BeEmpty();
        select.SelectedIndex.Should().Be(-1);
    }

    [Fact]
    public void SelectedItem_ReturnsCorrectItem()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 1;
        
        select.SelectedItem.Should().Be("B");
    }

    [Fact]
    public void SelectNext_MovesToNextItem()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 0;
        
        select.SelectNext();
        
        select.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void SelectNext_AtEnd_StaysAtEnd()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 2;
        
        select.SelectNext();
        
        select.SelectedIndex.Should().Be(2);
    }

    [Fact]
    public void SelectPrevious_MovesToPreviousItem()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 2;
        
        select.SelectPrevious();
        
        select.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void SelectPrevious_AtStart_StaysAtStart()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 0;
        
        select.SelectPrevious();
        
        select.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void SelectFirst_MovesToFirst()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 2;
        
        select.SelectFirst();
        
        select.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void SelectLast_MovesToLast()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 0;
        
        select.SelectLast();
        
        select.SelectedIndex.Should().Be(2);
    }

    [Fact]
    public void SelectionChanged_RaisedOnChange()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        string? selected = null;
        select.SelectionChanged += (_, s) => selected = s;
        
        select.SelectedIndex = 1;
        
        selected.Should().Be("B");
    }

    [Fact]
    public void Activate_RaisesItemActivated()
    {
        var select = new SelectRenderable();
        select.AddItems(["A", "B", "C"]);
        select.SelectedIndex = 1;
        string? activated = null;
        select.ItemActivated += (_, s) => activated = s;
        
        select.Activate();
        
        activated.Should().Be("B");
    }

    [Fact]
    public void Render_ShowsItems()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var select = new SelectRenderable();
        select.AddItems(["Item A", "Item B", "Item C"]);
        select.Layout.Width = 15;
        select.Layout.Height = 5;
        select.Layout.AlignSelf = AlignSelf.FlexStart;
        select.Focus();
        
        renderer.Root.Add(select);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // First item should have selection prefix
        buffer.GetCell(0, 0).Character.Should().Be("›");
        // Item text starts after prefix
        buffer.GetCell(0, 2).Character.Should().Be("I");
    }

    [Fact]
    public void Render_EmptyList_ShowsEmptyText()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var select = new SelectRenderable
        {
            EmptyText = "No items"
        };
        select.Layout.Width = 15;
        select.Layout.Height = 5;
        select.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(select);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("N");
    }
}
