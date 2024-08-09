using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using CardSharp.Models;
using CardSharp.ViewModels;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class Table : Canvas
{
    // used when dragging cards
    private Point _startPoint;
    private List<Point> _homePoints = new List<Point>();
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
    private List<Hand> _hands = new List<Hand>();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.CardMoved += Vm_CardMoved;

            IEnumerable<CardViewModel> deck = vm.CreateDeck(4);
            CardStack cardStack = new CardStack();
            _cardStacks.Add(cardStack);
            Children.Add(cardStack);
            SetCanvasPosition(cardStack, new Point((Width - App.SCardSize.Width) / 2, (Height - App.SCardSize.Height) / 2));
            foreach (CardViewModel cardVM in deck)
            {
                Card card = new Card(cardVM);
                _cardContainers.Add(cardVM, card);
                _cardHashTable.Add(cardVM.Hash, card);
            }
            cardStack.CardStackChanged += OnCardStackChanged;
            cardStack.AddCards(deck);

            _hands.Add(MakeHand(Hand.HandTypes.MainHand, 10, 0));
            SetCanvasPosition(_hands[0], new Point((Width - App.SCardSize.Width * 10 )/ 2, Height));
            _hands.AddRange(MakeHands(vm.PlayerCount));
        }
        base.OnAttachedToVisualTree(e);
    }

    private Hand MakeHand(Hand.HandTypes handType, int width, int angle)
    {
        Hand hand = new Hand(handType, width, angle);
        _cardStacks.Add(hand);
        Children.Add(hand);
        hand.CardStackChanged += OnCardStackChanged;
        return hand;
    }

    private IEnumerable<Hand> MakeHands(int playerCount)
    {
        List<Hand> list = new List<Hand>();
        if (playerCount % 2 == 0)
        {
            Hand hand = MakeHand(Hand.HandTypes.OtherHand, 3, 0);
            SetCanvasPosition(hand, new Point((Width - App.SCardSize.Width * 3) / 2, 0));
            list.Add(hand);
        }
        if (playerCount % 2 != 0)
        {
            Hand hand = MakeHand(Hand.HandTypes.OtherHand, 3, 0);
            Hand hand2 = MakeHand(Hand.HandTypes.OtherHand, 3, 0);
            SetCanvasPosition(hand, new Point((Width - App.SCardSize.Width * 9) / 2, 0));
            SetCanvasPosition(hand2, new Point((Width + App.SCardSize.Width * 3) / 2, 0));
            list.Add(hand);
            list.Add(hand2);
        }
        if (playerCount == 6)
        {
            Hand hand = MakeHand(Hand.HandTypes.OtherHand, 3, 0);
            Hand hand2 = MakeHand(Hand.HandTypes.OtherHand, 3, 0);
            SetCanvasPosition(hand, new Point((Width - App.SCardSize.Width * 11) / 2, 0));
            SetCanvasPosition(hand2, new Point((Width + App.SCardSize.Width * 5) / 2, 0));
            list.Add(hand);
            list.Add(hand2);
        }
        if (playerCount >= 4)
        {
            Hand hand = MakeHand(Hand.HandTypes.OtherHand, 3, 90);
            Hand hand2 = MakeHand(Hand.HandTypes.OtherHand, 3, -90);
            SetCanvasPosition(hand, new Point(0, (Height - App.SCardSize.Width * 3) / 2));
            SetCanvasPosition(hand2, new Point(Width - App.SCardSize.Height, (Height - App.SCardSize.Width * 3) / 2));
            list.Add(hand);
            list.Add(hand2);
        }
        return list.OrderBy(hand => GetCanvasPosition(hand).X);
    }

    private async void Vm_CardMoved((byte[] cardHashes, Point? point, int player, bool isCardStack) data)
    {
        List<Card> cards = new List<Card>(data.cardHashes.Select(hash => _cardHashTable[hash]));


        // cancel current drag if one of the dragging cards is moved from server
        foreach (Card card in cards)
        {
            if (_draggington.Contains(card))
                CancelDragging();
            if (_selectedCards.Contains(card))
                Deselect();
            card.Classes.Add("fromServer");
        }
        Point point;
        if (data.point is not null)
            point = data.point.Value;
        else
            point = _hands[data.player].Bounds.TopLeft;


        if (data.isCardStack)
        {
            List<Border> items = [_cardStacks.Find(stack => stack.ContainsCard(cards[0].ViewModel))!];
            items.AddRange(cards);
            foreach (Border item in items)
                SetCanvasPosition(item, point);
            // wait for animation
            await Task.Delay(350);
            MoveCardStack(point, items);
        }
        else
        {
            foreach (Card card in cards)
            {
                SetCanvasPosition(card, point);
                RemoveCardFromCardStack(card);
            }
            // wait  for animation
            await Task.Delay(350);
            MoveCards(point, cards);
        }
        foreach (Card card in cards)
            card.Classes.Remove("fromServer");
    }

    public void AddSelectedCards(IEnumerable<CardViewModel> cardVMs)
    {
        Deselect();
        foreach (CardViewModel cardVM in cardVMs)
            cardVM.IsSelected = true;
        foreach (Card card in cardVMs.Select(cardVM => _cardContainers[cardVM]))
            _selectedCards.Add(card);
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
            if (cardStack is Hand { IsRotated: true })
                SetCanvasPosition(card, GetCanvasPosition(cardStack) + new Point(0, counter * e.CardOffset));
            else
                SetCanvasPosition(card, GetCanvasPosition(cardStack) + new Point(counter * e.CardOffset, 0));
            counter++;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        CancelDragging();
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Point mousePos = e.GetPosition(this);


        if (!_hasMoved)
        {
            if (_doubleClickedOn is not null)
            {
                if (_selectedCards.Contains(_doubleClickedOn))
                {
                    _doubleClickedOn.ViewModel.IsSelected = false;
                    _selectedCards.Remove(_doubleClickedOn);
                }
                else
                {
                    _doubleClickedOn.ViewModel.IsSelected = true;
                    _selectedCards.Add(_doubleClickedOn);
                }
            }
            ResetDrag();
            return;
        }

        if (_draggington[0] is CardStack)
        {
            List<Border> newList = _draggington.ConvertAll(border => border);
            List<Point> newHomePoints = _homePoints.ConvertAll(point => point);
            ResetDrag();
            Deselect();
            MoveCardStack(mousePos, newList, newHomePoints, true);
        }
        else
        {
            List<Card> newList = _draggington.ConvertAll(card => (Card)card);
            List<Point> newHomePoints = _homePoints.ConvertAll(point => point);
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
            SetCanvasPosition(_draggington[dragIndex], _homePoints[dragIndex] + e.GetPosition(this) - _startPoint);
            if (_draggington[0] is CardStack)
                continue;

            RemoveCardFromCardStack((Card)_draggington[dragIndex]);
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

        bool onHand = false;

        foreach (Visual? visual in this.GetVisualsAt(mousePos)
                     .OrderByDescending(visual => visual.ZIndex))
        {
            if (isAltClick && visual is Hand { HandType: Hand.HandTypes.MainHand })
                onHand = true;

            if (isAltClick && visual is CardStack cardStack)
            {
                AddItemsToBeDragged([cardStack]);
                AddItemsToBeDragged(cardStack.GetCardsToShow().Reverse().Select(cardViewModel => _cardContainers[cardViewModel]));
            }
            if (visual is not Card { DataContext: CardViewModel cvm } card)
                continue;

            if (isAltClick && onHand)
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
                _homePoints.Add(GetCanvasPosition(item));
            }
        }
    }

    // TO-DO change to clip to cardstack size
    private void SetCanvasPosition(AvaloniaObject? control, Point newPoint)
    {
        if (control is null)
            return;

        SetLeft(control, Math.Max(Math.Min(newPoint.X, Width - App.SCardSize.Width), 0));
        SetTop(control, Math.Max(Math.Min(newPoint.Y, Height - App.SCardSize.Height), 0));
    }

    private Point GetCanvasPosition(AvaloniaObject? control)
    {
        if (control is null)
            return new Point();

        return new Point(GetLeft(control), GetTop(control));
    }

    private void RemoveCardFromCardStack(Card card)
    {
        CardStackBase? cardStackBase = _cardStacks.Find(stack => stack.ContainsCard(card.ViewModel));

        if (cardStackBase is null)
            return;

        cardStackBase.RemoveCard(card.ViewModel);

        // can only be true if cardstackbase is an empty cardstack, since hand has no IsEmpty method
        if (cardStackBase is not CardStack cardStack)
            return;

        if (cardStack.IsEmpty())
        {
            cardStack.CardStackChanged -= OnCardStackChanged;
            _cardStacks.Remove(cardStack);
            Children.Remove(cardStack);
        }
    }

    void CancelDragging()
    {
        if (!_isDragging)
            return;
        if (_draggington[0] is CardStack)
            MoveCardStackBack(_draggington, _homePoints);
        else
            for (int i = 0; i < _draggington.Count; i++)
                MoveCardBack(_homePoints[i], (Card)_draggington[i]);
        Deselect();
        ResetDrag();
    }

    private void ResetDrag()
    {
        _hasMoved = false;
        _isDragging = false;
        _startPoint = new Point();
        _draggington = new List<Border>();
        _homePoints = new List<Point>();
        _doubleClickedOn = null;
    }

    private void Deselect()
    {
        foreach (Card card in _selectedCards)
            card.ViewModel.IsSelected = false;
        _selectedCards = new List<Card>();
    }

    private void MoveCardStack(Point point, List<Border> movedStack, List<Point>? homePoints = null, bool canFail = false)
    {
        Card? cardHover = null;

        CardStack cardStack = (CardStack)movedStack[0];
        movedStack.Reverse();
        foreach (Visual? visual in this.GetVisualsAt(point + new Point(1, 1))
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
            if (visual is Hand { HandType: Hand.HandTypes.MainHand } hand && cardHover is not null)
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

        if (!canFail || homePoints is null)
            return;
        foreach (CardStackBase otherCardStack in _cardStacks)
        {
            if (otherCardStack.Equals(cardStack))
                continue;

            Rect bounds = otherCardStack.Bounds;
            if (otherCardStack is Hand { IsRotated: true })
                bounds = new Rect(bounds.X, bounds.Y, bounds.Height, bounds.Width);
            if (bounds.Intersects(cardStack.Bounds))
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

    private void MoveCards(Point point, List<Card> cards, List<Point>? homepoints = null, bool canFail = false)
    {
        Card? cardHover = null;


        foreach (Visual? visual in this.GetVisualsAt(point + new Point(1, 1))
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
            if (visual is Hand { HandType: Hand.HandTypes.MainHand } hand && cardHover is not null)
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
        if (canFail && homepoints is not null)
        {
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

            if (draggedCard is not null)
                foreach (CardStackBase cardStack in _cardStacks)
                {
                    Rect bounds = cardStack.Bounds;
                    if (cardStack is Hand { IsRotated: true })
                        bounds = new Rect(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    if (bounds.Intersects(draggedCard.Bounds))
                    {
                        for (int i = 0; i < cards.Count; i++)
                            MoveCardBack(homepoints[i], cards[i]);
                        return;
                    }
                }
        }

        MoveCardsToNewStack(GetCanvasPosition(draggedCard ?? cards[0]), cards);
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

