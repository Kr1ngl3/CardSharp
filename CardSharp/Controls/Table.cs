using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    private List<Point> _homePoint = new List<Point>();
    private bool _isDragging;
    private List<Card> _draggington = new List<Card>();

    private List<CardStack> _cardStacks = new List<CardStack>();

    public Table()
    {
        Children.Add(new Card()
        {
            DataContext = new CardViewModel()
        });
        Children.Add(new Card()
        {
            DataContext = new CardViewModel()
        });
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
            Trace.WriteLine("GetDeck of cards");

        base.OnAttachedToVisualTree(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        PointerPoint pointer = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
            return;
        Point mousePos = pointer.Position;
        MoveCard(mousePos);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        Point mousePos = e.GetCurrentPoint(this).Position;

        for (int i = 0; i < _draggington.Count; i++)
            SetCanvasPosition(_draggington[i], _homePoint[i] + mousePos - _dragStart);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        PointerPoint pointer = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        if (pointer.Properties.IsRightButtonPressed)
            return;
        Point mousePos = pointer.Position;
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(mousePos)
                     .OrderBy(x => x.ZIndex))
        {
            if (visual is not Card { DataContext: CardViewModel cvm} card) 
                continue;

            for (int i = 0; i < _cardStacks.Count; i++)
            {
                CardStack cardStack = _cardStacks[i];
                if (cardStack.ContainsCard(card))
                {
                    cardStack.RemoveCard(card);
                    if (cardStack.IsEmpty())
                    {
                        _cardStacks.Remove(cardStack);
                        Children.Remove(cardStack);
                    }
                    break;
                }
            }
            _dragStart = e.GetCurrentPoint(this).Position;
            _draggington.Add(card);
            _isDragging = true;
            _homePoint.Add(card.Bounds.TopLeft);

            if (e.KeyModifiers == KeyModifiers.Control)
                continue;
            break;            
        }
    }

    private void SetCanvasPosition(AvaloniaObject? control, Vector newVector)
    {
        if (control is null) 
            return;

        SetLeft(control, Math.Max(Math.Min(newVector.X, Width - 200), 0));
        SetTop(control, Math.Max(Math.Min(newVector.Y, Height - 300), 0));
    }

    private Vector GetCanvasPosition(AvaloniaObject? control)
    {
        if (control is null) 
            return new Vector();

        return new Vector(GetLeft(control), GetTop(control));
    }

    private void ResetDrag()
    {
        _isDragging = false;
        _dragStart = new Point();
        _draggington = new List<Card>();
        _homePoint = new List<Point>();
    }

    private void MoveCard(Point point, bool canFail = true)
    {
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(point)
                        .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack cardStack)
            {
                cardStack.AddCards(_draggington);
                foreach (Card card in _draggington)
                    SetCanvasPosition(card, GetCanvasPosition(cardStack));

                ResetDrag();
                return;
            }
        }


        if (canFail)
        {
            foreach (Control control in Children)
            {
                if (control is Card card && !_draggington.Contains(card))
                {
                    if (card.Bounds.Intersects(_draggington[0].Bounds))
                    {
                        MoveCard((Point)this.TranslatePoint(_homePoint[0], TopLevel.GetTopLevel(this)!)!, false);
                        return;
                    }
                }
            }
        }
        else
        {
            foreach (Card card in _draggington)
                SetCanvasPosition(card, _homePoint[0]);
        }


        CardStack newCardStack = new CardStack(_draggington);
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        SetCanvasPosition(newCardStack, GetCanvasPosition(_draggington[0]));
        ResetDrag();
    }
}

