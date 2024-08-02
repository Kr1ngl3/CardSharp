using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
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
        var absCur = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        var absCurPos = absCur.Position;

        foreach (var visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(absCurPos)
                     .OrderByDescending(x => x.ZIndex))
        {
            if (visual is CardControl)
            {
                _dragStart = e.GetCurrentPoint(this).Position;
                _draggington = visual;
                _isDragging = true;
                _homePoint = new Point(visual.Bounds.Position.X, visual.Bounds.Position.Y);
            }
        }
    }

    private void SetCanvasPosition(AvaloniaObject? control, Vector newVector)
    {
        if (control is null) return;

        SetLeft(control, newVector.X);
        SetTop(control, newVector.Y);
    }
}
