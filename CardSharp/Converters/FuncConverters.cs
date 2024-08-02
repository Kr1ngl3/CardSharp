using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CardSharp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CardSharp.Converters;
public static class FuncConverters
{

    public static GetCardArt SGetCardArt = new GetCardArt();

    public class GetCardArt : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not Ranks rank)
                return null;
            if (values[1] is not Bitmap source)
                return null;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            StackPanel[] columns = [
                new StackPanel(){
                        Margin = new Thickness(200 / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    },
                    new StackPanel(){
                        Margin = new Thickness(200 / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    },
                    new StackPanel(){
                        Margin = new Thickness(200 / 40, 0),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }];

            grid.Children.AddRange(columns);
            for (int i = 0; i < 3; i++)
                columns[i].SetValue(Grid.ColumnProperty, i);

            switch (rank)
            {
                case Models.Ranks.Ace:
                case Models.Ranks.Two:
                case Models.Ranks.Three:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        columns[1].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, 200 / 10),
                            Stretch = Stretch.Uniform,
                            Source = source
                        });

                    }
                    break;
                case Models.Ranks.Four:
                case Models.Ranks.Five:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i == 4)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Margin = new Thickness(0, 200 / 10),
                                Stretch = Stretch.Uniform,
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Margin = i < 2 ? new Thickness(0, 0, 0, 300 / 5) : new Thickness(0, 300 / 5, 0, 0),
                            Stretch = Stretch.Uniform,
                            Source = source
                        });

                    }
                    break;
                case Models.Ranks.Six:
                case Models.Ranks.Seven:
                case Models.Ranks.Eight:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i > 5)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Margin = new Thickness(0, 200 / 10),
                                Stretch = Stretch.Uniform,
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, 200 / 10),
                            Stretch = Stretch.Uniform,
                            Source = source
                        });

                    }
                    break;
                case Models.Ranks.Nine:
                case Models.Ranks.Ten:
                    for (int i = 0; i < (int)rank + 1; i++)
                    {
                        if (i > 7)
                        {
                            columns[1].Children.Add(new Image()
                            {
                                Margin = new Thickness(0, 200 / 10),
                                Stretch = Stretch.Uniform,
                                Source = source
                            });
                            continue;
                        }
                        columns[i % 2 == 0 ? 0 : 2].Children.Add(new Image()
                        {
                            Margin = new Thickness(0, 200 / 15),
                            Stretch = Stretch.Uniform,
                            Source = source
                        });

                    }
                    break;
                default:
                    columns[1].Children.Add(new Image()
                    {
                        Stretch = Stretch.Uniform,
                        Source = source
                    });
                    break;
            }
            return grid;
        }
    }
}