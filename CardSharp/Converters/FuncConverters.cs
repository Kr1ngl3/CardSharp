using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSharp.Converters;
public static class FuncConverters
{
    public static FuncValueConverter<double, GridLength> DoubleToGridLength { get; } =
        new FuncValueConverter<double, GridLength>(num => new GridLength(num));

    public static FuncValueConverter<double, string, double> Mult { get; } =
        new FuncValueConverter<double, string, double>((multiplicand, multiplier) => multiplicand * double.Parse(multiplier!, CultureInfo.InvariantCulture));
}