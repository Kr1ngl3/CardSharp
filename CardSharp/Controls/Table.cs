﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CardSharp.Models;
using CardSharp.ViewModels;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
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
    private Card? _doubleClickedOn;

    private List<Card> _selectedCards = new List<Card>();

    private Dictionary<CardViewModel, Card> _cardContainers = new Dictionary<CardViewModel, Card>();
    private List<CardStack> _cardStacks = new List<CardStack>();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            IEnumerable<CardViewModel> deck = vm.CreateDeck(4);
            CardStack cardStack = new CardStack();
            _cardStacks.Add(cardStack);
            Children.Add(cardStack);
            foreach (CardViewModel cardVM in deck)
            {
                Card card = new Card(cardVM);
                _cardContainers.Add(cardVM, card);
            }
            cardStack.CardStackChanged += OnCardStackChanged;
            cardStack.AddCards(deck);
        }
        base.OnAttachedToVisualTree(e);
    }

    private void OnCardStackChanged(object? sender, CardStackChangedEventArgs e)
    {
        foreach (CardViewModel cardVM in e.CardsToNotShow ?? new List<CardViewModel>())
        {
            Card card = _cardContainers[cardVM];
            Children.Remove(card);
        }
        int counter = 0;
        foreach (CardViewModel cardVM in e.CardsToShow)
        {
            Card card = _cardContainers[cardVM];
            CardStack cardStack = _cardStacks.Find(stack => stack.ContainsCard(cardVM))!;

            Children.Remove(card);
            Children.Add(card);
            SetCanvasPosition(card, GetCanvasPosition(cardStack) - new Point(-counter * e.CardOffset, 0));
            counter++;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Point mousePos = e.GetCurrentPoint(TopLevel.GetTopLevel(this)).Position;

        if (!_hasMoved)
        {
            if (_doubleClickedOn is not null)
            {
                (_doubleClickedOn.DataContext as CardViewModel)!.IsSelected = true;
                _selectedCards.Add(_doubleClickedOn);
            }
            ResetDrag();
            return;
        }

        if (_draggington[0] is CardStack)
        {
            List<Border> newList = _draggington.ConvertAll(border => border);
            List<Point> newHomePoints = _homePoint.ConvertAll(point => point);
            ResetDrag();
            Deselect();
            MoveCardStack(mousePos, newList, newHomePoints);
        }
        else
        {
            List<Card> newList = _draggington.ConvertAll(card => (Card)card);
            List<Point> newHomePoints = _homePoint.ConvertAll(point => point);
            ResetDrag();
            Deselect();
            MoveCards(mousePos, newList, newHomePoints);
        }
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
        bool doubleClickFlag = false;

        PointerPoint pointer = e.GetCurrentPoint(TopLevel.GetTopLevel(this));
        if (pointer.Properties.IsRightButtonPressed)
            return;


        Point mousePos = pointer.Position;
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(mousePos)
                     .OrderByDescending(visual => visual.ZIndex))
        {
            if (e.KeyModifiers == KeyModifiers.Control || e.ClickCount == 2)
            {
                doubleClickFlag = true;
                if (visual is CardStack cardStack)
                {
                    _draggington.Add(cardStack);
                    _homePoint.Add(cardStack.Bounds.TopLeft);
                    _startPoint = e.GetCurrentPoint(this).Position;
                    _isDragging = true;
                    foreach (CardViewModel cardViewModel in cardStack.GetCardsToShow().Reverse())
                    {
                        Card cardStackCard = _cardContainers[cardViewModel];
                        _draggington.Add(cardStackCard);
                        _homePoint.Add(cardStackCard.Bounds.TopLeft);
                    }
                }
            }
            if (visual is not Card { DataContext: CardViewModel cvm } card)
                continue;

            if (doubleClickFlag)
            {
                _doubleClickedOn = card;
                break;
            }

            if (_selectedCards.Contains(card))
            {
                _startPoint = e.GetCurrentPoint(this).Position;
                _isDragging = true;
                foreach (Card selectedCard in _selectedCards)
                {
                    _draggington.Add(selectedCard);
                    _homePoint.Add(selectedCard.Bounds.TopLeft);
                }
                break;
            }

            _startPoint = e.GetCurrentPoint(this).Position;
            _isDragging = true;
            _draggington.Add(card);
            _homePoint.Add(card.Bounds.TopLeft);
            break;
        }
    }

    // TO-DO change to match cards (maybe)
    private void SetCanvasPosition(AvaloniaObject? control, Point newPoint)
    {
        if (control is null)
            return;

        SetLeft(control, Math.Max(Math.Min(newPoint.X, Width - (double)Application.Current!.FindResource("CardWidth")!), 0));
        SetTop(control, Math.Max(Math.Min(newPoint.Y, Height - (double)Application.Current!.FindResource("CardHeight")!), 0));
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
        _doubleClickedOn = null;
    }

    private void Deselect()
    {
        foreach (Card card in _selectedCards)
            (card.DataContext as CardViewModel)!.IsSelected = false;
        _selectedCards = new List<Card>();

    }

    private void MoveCardStack(Point screenPoint, List<Border> movedStack, List<Point> homePoints, bool canFail = true)
    {
        CardStack cardStack = (CardStack)movedStack[0];
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(screenPoint)
                .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack otherCardStack && !otherCardStack.Equals(cardStack))
            {
                Children.Remove(cardStack);
                _cardStacks.Remove(cardStack);
                _draggington.Remove(cardStack);

                otherCardStack.AddCards(cardStack);
                return;
            }
        }

        if (canFail)
        {
            foreach (CardStack otherCardStack in _cardStacks)
            {
                if (otherCardStack.Equals(cardStack))
                    continue;
                if (otherCardStack.Bounds.Intersects(cardStack.Bounds))
                {
                    MoveCardStack((Point)this.TranslatePoint(homePoints.First(), TopLevel.GetTopLevel(this)!)!, movedStack, homePoints, false);
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < movedStack.Count; i++)
                SetCanvasPosition(movedStack[i], homePoints[i]);
        }
    }

    private void MoveCards(Point screenPoint, List<Card> cards, List<Point> homepoints, bool canFail = true)
    {
        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(screenPoint)
                       .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack cardStack)
            {
                cardStack.AddCards(cards.Select(card => (CardViewModel)card.DataContext!));
                return;
            }
        }

        Card? movedCard = null;

        if (cards.Count == 1)
            movedCard = cards[0];
        else
        {
            foreach (Card card in cards)
            {
                if (movedCard is not null)
                    break;
                movedCard = card.Bounds.Contains((Point)TopLevel.GetTopLevel(this)!.TranslatePoint(screenPoint, this)!) ? card : null;
            }
        }

        if (movedCard is null)
            throw new Exception("moved cards but didn't move any cards??");


        if (canFail)
        {
            foreach (CardStack cardStack in _cardStacks)
            {
                if (cardStack.Bounds.Intersects(movedCard.Bounds))
                {
                    for (int i = 0; i < cards.Count; i++)
                        MoveCard(homepoints[i], cards[i]);
                    return;
                }
            }
        }

        MoveCardsToNewStack(movedCard.Bounds.TopLeft, cards);
    }

    private void MoveCardsToNewStack(Point canvasPoint, IEnumerable<Card> cards)
    {
        CardStack newCardStack = new CardStack();
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        newCardStack.CardStackChanged += OnCardStackChanged;
        SetCanvasPosition(newCardStack, canvasPoint);
        newCardStack.AddCards(cards.Select(card => (CardViewModel)card.DataContext!));
    }

    private void MoveCard(Point canvasPoint, Card card)
    {
        Point screenPoint = (Point)this.TranslatePoint(canvasPoint, TopLevel.GetTopLevel(this)!)!;

        foreach (Visual? visual in TopLevel.GetTopLevel(this)!.GetVisualsAt(screenPoint)
                        .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStack cardStack)
            {
                cardStack.AddCards([(CardViewModel)card.DataContext!]);
                return;
            }
        }

        CardStack newCardStack = new CardStack();
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        newCardStack.CardStackChanged += OnCardStackChanged;
        SetCanvasPosition(newCardStack, canvasPoint);
        newCardStack.AddCards([(CardViewModel)card.DataContext!]);
    }
}

