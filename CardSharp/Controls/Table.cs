using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CardSharp.Models;
using CardSharp.ViewModels;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class Table : Canvas
{
    // used when dragging cards
    private Point _startPoint;
    private List<Point> _homePoint = new List<Point>();
    private List<Border> _draggington = new List<Border>();
    private Card? _doubleClickedOn;
    private bool _isDragging;
    private bool _hasMoved;

    // used when selecting cards
    private List<Card> _selectedCards = new List<Card>();

    // used all throughout
    private Dictionary<CardViewModel, Card> _cardContainers = new Dictionary<CardViewModel, Card>();
    private Dictionary<byte, Card> _cardHashTable = new Dictionary<byte, Card>();
    private List<CardStackBase> _cardStacks = new List<CardStackBase>();
    private Hand _mainHand = null!;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.CardMoved += Vm_CardMoved;
            IEnumerable<CardViewModel> deck = vm.CreateDeck(4);
            CardStack cardStack = new CardStack();
            _cardStacks.Add(cardStack);
            Children.Add(cardStack);
            SetCanvasPosition(cardStack, new Point(920, 510));
            foreach (CardViewModel cardVM in deck)
            {
                Card card = new Card(cardVM);
                _cardContainers.Add(cardVM, card);
                _cardHashTable.Add(cardVM.Hash, card);
            }
            cardStack.CardStackChanged += OnCardStackChanged;
            cardStack.AddCards(deck);

            _mainHand = new Hand(Hand.HandTypes.MainHand, (double)Application.Current!.FindResource("CardWidth")! * 10);
            _cardStacks.Add(_mainHand);
            Children.Add(_mainHand);
            SetCanvasPosition(_mainHand, new Point((Width - (double)Application.Current!.FindResource("CardWidth")! * 10 )/ 2, Height));
            _mainHand.CardStackChanged += OnCardStackChanged;
        }
        base.OnAttachedToVisualTree(e);
    }

    private void Vm_CardMoved((byte hash, Point point) data)
    {
        Card card = _cardHashTable[data.hash];
        SetCanvasPosition(card, data.point);
        CardStackBase cardStack = _cardStacks.Find(stack => stack.ContainsCard(card.ViewModel))!;
        cardStack.RemoveCard(card.ViewModel);

        MoveCards(data.point, [card], [card.Bounds.TopLeft], false);
    }

    private void OnCardStackChanged(CardStackBase.CardStackChangedEventArgs e)
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
            CardStackBase cardStack = _cardStacks.Find(stack => stack.ContainsCard(cardVM))!;

            Children.Remove(card);
            Children.Add(card);
            SetCanvasPosition(card, GetCanvasPosition(cardStack) - new Point(-counter * e.CardOffset, 0));
            counter++;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Point mousePos = e.GetPosition(this);


        if (!_hasMoved)
        {
            if (_doubleClickedOn is not null)
            {
                _doubleClickedOn.ViewModel.IsSelected = true;
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
            MoveCardStack(mousePos, newList, newHomePoints, true);
        }
        else
        {
            List<Card> newList = _draggington.ConvertAll(card => (Card)card);
            List<Point> newHomePoints = _homePoint.ConvertAll(point => point);
            ResetDrag();
            Deselect();
            MoveCards(mousePos, newList, newHomePoints, true);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        for (int dragIndex = 0; dragIndex < _draggington.Count; dragIndex++)
        {
            SetCanvasPosition(_draggington[dragIndex], _homePoint[dragIndex] + e.GetPosition(this) - _startPoint);
            if (_draggington[0] is CardStack)
                continue;

            for (int stackIndex = 0; stackIndex < _cardStacks.Count; stackIndex++)
            {
                CardStackBase cardStack = _cardStacks[stackIndex];
                if (cardStack.ContainsCard((CardViewModel)_draggington[dragIndex].DataContext!))
                {
                    cardStack.RemoveCard((CardViewModel)_draggington[dragIndex].DataContext!);

                    if ((cardStack as CardStack)?.IsEmpty() ?? false)
                    {
                        cardStack.CardStackChanged -= OnCardStackChanged;
                        _cardStacks.Remove(cardStack);
                        Children.Remove(cardStack);
                    }
                    break;
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
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            return;

        bool isAltClick = e.KeyModifiers == KeyModifiers.Control || e.ClickCount == 2;

        Point mousePos = e.GetPosition(this);

        foreach (Visual? visual in this.GetVisualsAt(mousePos)
                     .OrderByDescending(visual => visual.ZIndex))
        {
            if (isAltClick && visual is CardStack cardStack)
            {
                AddItemsToBeDragged([cardStack]);
                AddItemsToBeDragged(cardStack.GetCardsToShow().Reverse().Select(cardViewModel => _cardContainers[cardViewModel]));
            }
            if (visual is not Card { DataContext: CardViewModel cvm } card)
                continue;

            if (isAltClick)
            {
                _doubleClickedOn = card;
                break;
            }

            if (_selectedCards.Contains(card))
            {
                AddItemsToBeDragged(_selectedCards);
                break;
            }

            AddItemsToBeDragged([card]);
            break;
        }

        void AddItemsToBeDragged(IEnumerable<Border> items)
        {
            _startPoint = mousePos;
            _isDragging = true;
            foreach (Border item in items)
            {
                _draggington.Add(item);
                _homePoint.Add(GetCanvasPosition(item));
            }
        }
    }

    // TO-DO change to clip to cardstack size
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
            card.ViewModel.IsSelected = false;
        _selectedCards = new List<Card>();
    }

    private void MoveCardStack(Point point, List<Border> movedStack, List<Point> homePoints, bool canFail)
    {
        Card? cardHover = null;

        CardStack cardStack = (CardStack)movedStack[0];
        movedStack.Reverse();
        foreach (Visual? visual in this.GetVisualsAt(point)
                .OrderBy(x => x.ZIndex))
        {
            if (visual is Card card && cardHover is null)
            {
                bool cardCheck = true;
                foreach (Border dragged in movedStack)
                    if (card.Equals(dragged))
                        cardCheck = false;

                if (cardCheck)
                    cardHover = card;
            }
            if (visual is Hand hand && cardHover is not null)
            {
                Children.Remove(cardStack);
                _cardStacks.Remove(cardStack);
                hand.AddCardsAt(cardHover.ViewModel, cardStack);
                return;
            }
            if (visual is CardStackBase otherCardStack && !otherCardStack.Equals(cardStack))
            {
                Children.Remove(cardStack);
                _cardStacks.Remove(cardStack);

                otherCardStack.AddCards(cardStack);
                return;
            }
        }

        if (!canFail)
            return;
        foreach (CardStackBase otherCardStack in _cardStacks)
        {
            if (otherCardStack.Equals(cardStack))
                continue;
            if (otherCardStack.Bounds.Intersects(cardStack.Bounds))
            {
                MoveCardStackBack(movedStack, homePoints);
                break;
            }
        }
    }

    private void MoveCardStackBack(List<Border> movedStack, List<Point> homePoints)
    {
        for (int i = 0; i < movedStack.Count; i++)
            SetCanvasPosition(movedStack[i], homePoints[i]);
    }

    private void MoveCards(Point point, List<Card> cards, List<Point> homepoints, bool canFail)
    {
        Card? cardHover = null;


        foreach (Visual? visual in this.GetVisualsAt(point)
                       .OrderBy(x => x.ZIndex))
        {
            if (visual is Card card && cardHover is null)
            {
                bool cardCheck = true;
                foreach (Card dragged in cards)
                    if (card.Equals(dragged))
                        cardCheck = false;

                if (cardCheck)
                    cardHover = card;
            }
            if (visual is Hand hand && cardHover is not null)
            {
                hand.AddCardsAt(cardHover.ViewModel, cards.Select(card => card.ViewModel));
                return;
            }
            if (visual is CardStackBase cardStack)
            {
                cardStack.AddCards(cards.Select(card => card.ViewModel));
                return;
            }
        }

        Card? draggedCard = null;
        if (cards.Count == 1)
            draggedCard = cards[0];
        else
        {
            foreach (Card card in cards)
            {
                if (draggedCard is not null)
                    break;
                draggedCard = card.Bounds.Contains(point) ? card : null;
            }
        }

        if (draggedCard is null)
            throw new Exception("moved cards but didn't move any cards??");


        if (canFail)
        {
            foreach (CardStackBase cardStack in _cardStacks)
            {
                if (cardStack.Bounds.Intersects(draggedCard.Bounds))
                {
                    for (int i = 0; i < cards.Count; i++)
                        MoveCardBack(homepoints[i], cards[i]);
                    return;
                }
            }
        }

        MoveCardsToNewStack(GetCanvasPosition(draggedCard), cards);
    }

    private void MoveCardsToNewStack(Point canvasPoint, IEnumerable<Card> cards)
    {
        CardStack newCardStack = new CardStack();
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        newCardStack.CardStackChanged += OnCardStackChanged;
        SetCanvasPosition(newCardStack, canvasPoint);
        newCardStack.AddCards(cards.Select(card => card.ViewModel));
    }

    private void MoveCardBack(Point point, Card card)
    {

        foreach (Visual? visual in this.GetVisualsAt(point + new Point(1, 1))
                        .OrderBy(x => x.ZIndex))
        {
            if (visual is CardStackBase cardStack)
            {
                cardStack.AddCards([card.ViewModel]);
                return;
            }
        }

        CardStack newCardStack = new CardStack();
        _cardStacks.Add(newCardStack);
        Children.Add(newCardStack);
        newCardStack.CardStackChanged += OnCardStackChanged;
        SetCanvasPosition(newCardStack, point);
        newCardStack.AddCards([card.ViewModel]);
    }
}

