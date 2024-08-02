using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using CardSharp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class Table : Canvas
{
    private Point _dragStart;
    private Point _homePoint;
    private bool _isDragging;
    private Visual? _draggington;

    public Table()
    {
        Children.Add(new Card()
        {
            DataContext = new CardViewModel()
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
            Trace.WriteLine("HIHI");

        base.OnAttachedToVisualTree(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isDragging = false;
        _dragStart = new Point();
        _draggington = null;
        _homePoint = new Point();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        var position = e.GetCurrentPoint(this).Position;

        var delta = position - _dragStart;

        SetCanvasPosition(_draggington, _homePoint + delta);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        foreach (var visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(e.GetCurrentPoint(TopLevel.GetTopLevel(this)).Position)
                     .OrderBy(x => x.ZIndex))
        {
            if (visual is not Border { DataContext: CardViewModel cvm} border) 
                continue;
            Card card = (Children.FirstOrDefault(x => x.DataContext == cvm) as Card)!;
            _dragStart = e.GetCurrentPoint(this).Position;
            _draggington = card;
            _isDragging = true;
            _homePoint = new Point(card.Bounds.Position.X, card.Bounds.Position.Y);
            
        }
    }

    private void SetCanvasPosition(AvaloniaObject? control, Vector newVector)
    {
        if (control is null) return;

        SetLeft(control, newVector.X);
        SetTop(control, newVector.Y);
    }
}
