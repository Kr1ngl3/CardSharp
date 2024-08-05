using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using CardSharp.Models;
using CardSharp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static CardSharp.Controls.CardStack;

namespace CardSharp.Controls;
public class Table : Canvas
{
    private Point _startPoint;
    private List<Point> _homePoint = new List<Point>();
    private bool _isDragging;
    private List<Border> _draggington = new List<Border>();
    private bool _hasMoved;
    private Dictionary<CardViewModel, Card> _cardContainers = new Dictionary<CardViewModel, Card>();

    private List<CardStack> _cardStacks = new List<CardStack>();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            IEnumerable<CardViewModel> deck = vm.CreateDeck(4);
            CardStack cardStack = new CardStack(deck);
            _cardStacks.Add(cardStack);
            Children.Add(cardStack);
            foreach (CardViewModel cardVM in deck)
            {
                Card card = new Card(cardVM);
                _cardContainers.Add(cardVM, card);
            }
            cardStack.CardStackChanged += OnCardStackChanged;
        }
        base.OnAttachedToVisualTree(e);
    }

    private void OnCardStackChanged(CardStackChangedEventArgs e)
    {
        foreach (CardViewModel cardVM in e.CardsToNotShow ?? new List<CardViewModel>())
        {
            Card card = _cardContainers[cardVM];
            Children.Remove(card);
        }

        foreach (CardViewModel cardVM in e.CardsToShow)
        {
            Card card = _cardContainers[cardVM];
            CardStack cardStack = _cardStacks.Find(stack => stack.ContainsCard(cardVM))!;

            Children.Remove(card);
            Children.Add(card);
            SetCanvasPosition(card, GetCanvasPosition(cardStack));
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        PointerPoint pointer = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
            return;
        
        
        if (!_hasMoved)
        {
            if (_draggington[0] is CardStack)
                (_draggington[1].DataContext as CardViewModel)!.IsSelected = true;
            ResetDrag();
            return;
        }
        
        Point mousePos = pointer.Position;
        if (_draggington[0] is CardStack)
            MoveCardStack(mousePos);
        else
            MoveCard(mousePos);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        Point mousePos = e.GetCurrentPoint(this).Position;

        for (int dragIndex = 0; dragIndex < _draggington.Count; dragIndex++)
        {
            SetCanvasPosition(_draggington[dragIndex], _homePoint[dragIndex] + mousePos - _startPoint);
            if (_draggington[0] is not CardStack)
            {
                for (int stackIndex = 0; stackIndex < _cardStacks.Count; stackIndex++)
                {
                    CardStack cardStack = _cardStacks[stackIndex];
                    if (cardStack.ContainsCard((CardViewModel)_draggington[dragIndex].DataContext!))
                    {
                        cardStack.RemoveCard((CardViewModel)_draggington[dragIndex].DataContext!);
                        if (cardStack.IsEmpty())
                        {
                            cardStack.CardStackChanged -= OnCardStackChanged;
                            _cardStacks.Remove(cardStack);
                            Children.Remove(cardStack);
                        }
                        break;
                    }
                }
            }
        }

        // only run one time when cards are being moved
        if (!_hasMoved)
        {
            // re-add cards that are being moved to draw them over other cards
            _draggington.Reverse();
            foreach (Border item in _draggington)
            {
                Children.Remove(item);
                Children.Add(item);
            }
            _draggington.Reverse();
        }
        _hasMoved = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        PointerPoint pointer = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        if (pointer.Properties.IsRightButtonPressed)
            return;


        Point mousePos = pointer.Position;
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(mousePos)
                     .OrderByDescending(visual => visual.ZIndex))
        {
            if (e.KeyModifiers == KeyModifiers.Control || e.ClickCount == 2)
            {
                if (visual is CardStack cardStack)
                {
                    _draggington.Add(cardStack);
                    _homePoint.Add(cardStack.Bounds.TopLeft);
                }
            }
            if (visual is not Card { DataContext: CardViewModel cvm} card) 
                continue;

            _startPoint = e.GetCurrentPoint(this).Position;
            _draggington.Add(card);
            _isDragging = true;
            _homePoint.Add(card.Bounds.TopLeft);

            if (e.KeyModifiers == KeyModifiers.Control || e.ClickCount == 2)
                continue;
            break;          

        }
    }

    private void SetCanvasPosition(AvaloniaObject? control, Point newPoint)
    {
        if (control is null) 
            return;

        SetLeft(control, Math.Max(Math.Min(newPoint.X, Width - 200), 0));
        SetTop(control, Math.Max(Math.Min(newPoint.Y, Height - 300), 0));
    }

    private Point GetCanvasPosition(AvaloniaObject? control)
    {
        if (control is null) 
            return new Point();

        return new Point(GetLeft(control), GetTop(control));
    }

    private void ResetDrag()
    {
        _hasMoved = false;
        _isDragging = false;
        _startPoint = new Point();
        _draggington = new List<Border>();
        _homePoint = new List<Point>();
    }

    private void MoveCardStack(Point point, bool canFail = true)
    {
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(point)
                .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack cardStack && !cardStack.Equals(_draggington[0]))
            {
                CardStack movingStack = (CardStack)_draggington[0];
                Children.Remove(movingStack);
                _cardStacks.Remove(movingStack);
                _draggington.Remove(movingStack);

                cardStack.AddCards(movingStack);
                foreach (Card card in _draggington)
                    SetCanvasPosition(card, GetCanvasPosition(cardStack));

                ResetDrag();
                return;
            }
        }

        if (canFail)
        {
            foreach (CardStack cardStack in _cardStacks)
            {
                if (cardStack.Equals(_draggington[0]))
                    continue;
                if (cardStack.Bounds.Intersects(_draggington[0].Bounds))
                {
                    MoveCardStack((Point)this.TranslatePoint(_homePoint[0], TopLevel.GetTopLevel(this)!)!, false);
                    return;
                }
            }
        }
        else
        {
            foreach (Border item in _draggington)
                SetCanvasPosition(item, _homePoint[0]);
        }

        ResetDrag();
    }

    private void MoveCard(Point point, bool canFail = true)
    {
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(point)
                        .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack cardStack)
            {
                cardStack.AddCards(_draggington.Select(card => (CardViewModel)card.DataContext!));
                foreach (Card card in _draggington)
                    SetCanvasPosition(card, GetCanvasPosition(cardStack));

                ResetDrag();
                return;
            }
        }


        if (canFail)
        {
            foreach (CardStack cardStack in _cardStacks)
            {
                if (cardStack.Bounds.Intersects(_draggington[0].Bounds))
                {
                    MoveCard((Point)this.TranslatePoint(_homePoint[0], TopLevel.GetTopLevel(this)!)!, false);
                    return;
                }
            }
        }
        else
        {
            foreach (Card card in _draggington)
                SetCanvasPosition(card, _homePoint[0]);
        }


        CardStack newCardStack = new CardStack(_draggington.Select(card => (CardViewModel)card.DataContext!));
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        newCardStack.CardStackChanged += OnCardStackChanged;
        SetCanvasPosition(newCardStack, GetCanvasPosition(_draggington[0]));
        ResetDrag();
    }
}

