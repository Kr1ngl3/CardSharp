using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using Avalonia.Media;
using System.Security.Cryptography;

namespace CardSharp.Controls;
public class CardStackPanel : StackPanel
{
    /// <summary>
    /// Defines the Rotation property.
    /// </summary>
    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<StackPanel, double>(nameof(Rotation));

    /// <summary>
    /// Defines the Curve property.
    /// </summary>
    public static readonly StyledProperty<double> CurveProperty =
        AvaloniaProperty.Register<StackPanel, double>(nameof(Curve));
    
    
    /// <summary>
    /// Gets or sets how muc heach card is rotated depending on index from middle
    /// </summary>
    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    /// <summary>
    /// Gets or sets how much each card is moved up depending on index from edges. unit is in parts of card height so inverse
    /// </summary>
    public double Curve
    {
        get => GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    public CardStackPanel()
    {
        Orientation = Orientation.Horizontal;
        Rotation = 2.5;
        Curve = 50;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        bool fHorizontal = (Orientation == Orientation.Horizontal);
        Rect rcChild = new Rect(finalSize);
        double previousChildSize = 0.0;
        var spacing = 200 * Math.Max(-Math.Log10(children.Count) * 2.0 / 3.0, -.8);
        var rotation = Rotation;
        var curve = Curve;

        //
        // Arrange and Position Children.
        //
        for (int i = 0, count = children.Count; i < count; ++i)
        {
            var child = children[i];
            //child.RenderTransform = new RotateTransform()
            //{
            //    Angle = Math.Min((i - (count - 1.0) / 2.0) * rotation, 45)
            //};
            //child.Margin = new Thickness(0, -(-Math.Abs(i - (count - 1.0) / 2.0) - (count - 1.0) / 2.0) * rcChild.Height / curve / 2, 0, (-Math.Abs(i - (count - 1.0) / 2.0) - (count - 1.0) / 2.0) * rcChild.Height / curve / 2);

            if (!child.IsVisible)
            {
                continue;
            }

            if (fHorizontal)
            {
                rcChild = rcChild.WithX(rcChild.X + previousChildSize);
                previousChildSize = child.DesiredSize.Width;
                rcChild = rcChild.WithWidth(previousChildSize);
                rcChild = rcChild.WithHeight(Math.Max(finalSize.Height, child.DesiredSize.Height));
                previousChildSize += spacing;
            }
            else
            {
                rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize = child.DesiredSize.Height;
                rcChild = rcChild.WithHeight(previousChildSize);
                rcChild = rcChild.WithWidth(Math.Max(finalSize.Width, child.DesiredSize.Width));
                previousChildSize += spacing;
            }
            child.Arrange(rcChild);
        }

        RaiseEvent(new RoutedEventArgs(Orientation == Orientation.Horizontal ? HorizontalSnapPointsChangedEvent : VerticalSnapPointsChangedEvent));

        return finalSize;
    }


}
