using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class CardTemplate : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        // card width = 1/12 of screen
        // card height = 2/9 of screen
        // card is 2/3

        Card card = (data as Card)!;

        Border border = new Border();
        border.Width = 2*20;
        border.Height = 3*20;
        border.BorderBrush = Brushes.Black;
        border.BorderThickness = new Avalonia.Thickness(2);
        border.Background = Brushes.Blue;
        StackPanel panel = new StackPanel();
        border.Child = panel;

        panel.Children.Add(new TextBlock()
        {
            Text = card.Rank.ToString()
        });
        panel.Children.Add( new Image()
        {
            Stretch = Stretch.Uniform,
            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}")))
        });
        //panel.Children.Add(new TextBlock()
        //{
        //    RenderTransform = new RotateTransform()
        //    {
        //        Angle = 180
        //    },
        //    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        //    Text = card.Rank.ToString()
        //});

        return border;
    }

    public bool Match(object? data)
    {
        return data is Card;
    }
}
