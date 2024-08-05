using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CardSharp.Converters;
public static class Converters
{

    public static GetCardArt SGetCardArt = new GetCardArt();

    public class GetCardArt : IMultiValueConverter
    {
        private Size _cardSize;
        private double _iconHeight;

        public GetCardArt()
        {
            _cardSize = new Size((double)Application.Current!.FindResource("CardWidth")!, (double)Application.Current!.FindResource("CardHeight")!);
            _iconHeight = _cardSize.Height / 6;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not Ranks rank)
                return null;
            if (values[1] is not ISolidColorBrush brush)
                return null;

            if (rank == Ranks.Joker)
            {
                DrawingImage drawingImage = (DrawingImage)Application.Current!.FindResource($"{brush.ToString()}Joker")!;
                return new Image()
                {
                    Source = drawingImage
                };
            }

            if (values[2] is not DrawingImage source)
                return null;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            StackPanel[] columns = [
                new StackPanel(){
                        Margin = new Thickness(_cardSize.Width / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    },
                    new StackPanel(){
                        Margin = new Thickness(_cardSize.Width / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    },
                    new StackPanel(){
                        Margin = new Thickness(_cardSize.Width / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }];

            grid.Children.AddRange(columns);
            for (int i = 0; i < 3; i++)
                columns[i].SetValue(Grid.ColumnProperty, i);

            switch (rank)
            {
                case Ranks.Ace:
                case Ranks.Two:
                case Ranks.Three:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        columns[1].Children.Add(new Image()
                        {
                            Height = _iconHeight,
                            Margin = new Thickness(0, _cardSize.Width / 10),
                            Source = source
                        });

                    }
                    break;
                case Ranks.Four:
                case Ranks.Five:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i == 4)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Height = _iconHeight,
                                Margin = new Thickness(0, _cardSize.Width / 10),
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Height = _iconHeight,
                            Margin = i < 2 ? new Thickness(0, 0, 0, _cardSize.Height / 5) : new Thickness(0, _cardSize.Height / 5, 0, 0),
                            Source = source
                        });

                    }
                    break;
                case Ranks.Six:
                case Ranks.Seven:
                case Ranks.Eight:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i > 5)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Height = _iconHeight,
                                Margin = new Thickness(0, _cardSize.Width / 10),
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Height = _iconHeight,
                            Margin = new Thickness(0, _cardSize.Width / 10),
                            Source = source
                        });

                    }
                    break;
                case Ranks.Nine:
                case Ranks.Ten:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i > 7)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Height = _iconHeight,
                                Margin = new Thickness(0, _cardSize.Width / 10),
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Height = _iconHeight,
                            Margin = new Thickness(0, _cardSize.Width / 15),
                            Source = source
                        });

                    }
                    break;
                default:
                    columns[1].Children.Add(new Image()
                    {
                        Height = _iconHeight,
                        Source = source
                    });
                    break;
            }
            return grid;
        }

        private void SetBrush(DrawingImage image, ISolidColorBrush brush)
        {
            DrawingGroup drawGroup = (image.Drawing as DrawingGroup)!;
            DrawingGroup? temp1 = (drawGroup.Children.FirstOrDefault() as DrawingGroup);
            if (temp1 is null)
            {
                foreach (var temp in drawGroup.Children)
                {
                    if (temp is GeometryDrawing drawing)
                        drawing.Brush = brush;
                }
            }
            else
            {
                foreach (var temp in temp1.Children)
                {
                    if (temp is GeometryDrawing drawing)
                        drawing.Brush = brush;
                }
            }
        }
    }
}