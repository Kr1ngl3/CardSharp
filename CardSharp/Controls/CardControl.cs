using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using CardSharp.Models;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Controls;
public class CardControl : Border
{
    public CardControl()
    {
        Card card = new Card();
        CornerRadius = new CornerRadius(5);
        Width = 2 * 100;
        Height = 3 * 100;
        Background = Brushes.White;


        BorderBrush = Brushes.Black;
        BorderThickness = new Thickness(2);
        Padding = new Thickness(2);

        double width = Width + (BorderThickness - Padding).Right + (BorderThickness - Padding).Left;
        double height = Height + (BorderThickness - Padding).Top + (BorderThickness - Padding).Bottom;


        StackPanel panel = new StackPanel();
        Child = panel;
        panel.Orientation = Avalonia.Layout.Orientation.Horizontal;

        StackPanel topLeftPanel = new StackPanel();
        Grid grid = new Grid();
        StackPanel botRightPanel = new StackPanel();
        panel.Children.Add(topLeftPanel);
        panel.Children.Add(grid);
        panel.Children.Add(botRightPanel);

        grid.Width = width * 8 / 10;
        topLeftPanel.Width = width * 1.5 / 20;
        botRightPanel.Width = width * 1.5 / 20;



        botRightPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
        botRightPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;


        topLeftPanel.Children.Add(new TextBlock()
        {
            Foreground = Brushes.Black,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Text = Card.SRanksToSymbol[card.Rank]
        });
        topLeftPanel.Children.Add(new Image()
        {
            Stretch = Stretch.Uniform,
            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
        });

        botRightPanel.Children.Add(new Image()
        {
            RenderTransform = new RotateTransform()
            {
                Angle = 180
            },
            Stretch = Stretch.Uniform,
            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
        });
        botRightPanel.Children.Add(new TextBlock()
        {
            RenderTransform = new RotateTransform()
            {
                Angle = 180
            },
            Foreground = Brushes.Black,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Text = Card.SRanksToSymbol[card.Rank]
        });


        grid.ColumnDefinitions.Add([new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)]);

        StackPanel[] columns = [
            new StackPanel(){
                Margin = new Thickness(width / 40, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            },
            new StackPanel(){
                Margin = new Thickness(width / 40, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            },
            new StackPanel(){
                Margin = new Thickness(width / 40, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }];

        grid.Children.AddRange(columns);
        for (int i = 0; i < 3; i++)
            columns[i].SetValue(Grid.ColumnProperty, i);

        switch (card.Rank)
        {
            case Card.Ranks.Ace:
            case Card.Ranks.Two:
            case Card.Ranks.Three:
                for (int i = 0; i < (int)card.Rank + 1; i++)
                {
                    columns[1].Children.Add(new Image()
                    {
                        Margin = new Thickness(0, width / 10),
                        Stretch = Stretch.Uniform,
                        Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                    });

                }
                break;
            case Card.Ranks.Four:
            case Card.Ranks.Five:
                for (int i = 0; i < (int)card.Rank + 1; i++)
                {
                    if (i == 4)
                    {
                        columns[1].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, width / 10),
                            Stretch = Stretch.Uniform,
                            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                        });
                        continue;
                    }
                    columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                    {
                        Margin = i < 2 ? new Thickness(0, 0, 0, height / 5) : new Thickness(0, height / 5, 0, 0),
                        Stretch = Stretch.Uniform,
                        Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                    });

                }
                break;
            case Card.Ranks.Six:
            case Card.Ranks.Seven:
            case Card.Ranks.Eight:
                for (int i = 0; i < (int)card.Rank + 1; i++)
                {
                    if (i > 5)
                    {
                        columns[1].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, width / 10),
                            Stretch = Stretch.Uniform,
                            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                        });
                        continue;
                    }
                    columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                    {
                        Margin = new Thickness(0, width / 10),
                        Stretch = Stretch.Uniform,
                        Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                    });

                }
                break;
            case Card.Ranks.Nine:
            case Card.Ranks.Ten:
                for (int i = 0; i < (int)card.Rank + 1; i++)
                {
                    if (i > 7)
                    {
                        columns[1].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, width / 10),
                            Stretch = Stretch.Uniform,
                            Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                        });
                        continue;
                    }
                    columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                    {
                        Margin = new Thickness(0, width / 15),
                        Stretch = Stretch.Uniform,
                        Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                    });

                }
                break;
            default:
                columns[1].Children.Add(new Image()
                {
                    Stretch = Stretch.Uniform,
                    Source = new Bitmap(AssetLoader.Open(new Uri($"avares://CardSharp{card.Suit}"))),
                });
                break;
        }
    }
}
