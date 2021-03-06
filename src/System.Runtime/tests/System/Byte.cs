// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

public static class ByteTests
{
    [Fact]
    public static void TestCtor_Empty()
    {
        var b = new byte();
        Assert.Equal(0, b);
    }

    [Fact]
    public static void TestCtor_Value()
    {
        byte b = 41;
        Assert.Equal(41, b);
    }

    [Fact]
    public static void TestMaxValue()
    {
        Assert.Equal(0xFF, byte.MaxValue);
    }

    [Fact]
    public static void TestMinValue()
    {
        Assert.Equal(0, byte.MinValue);
    }

    [Theory]
    [InlineData((byte)234, (byte)234, 0)]
    [InlineData((byte)234, byte.MinValue, 1)]
    [InlineData((byte)234, (byte)0, 1)]
    [InlineData((byte)234, (byte)123, 1)]
    [InlineData((byte)234, (byte)235, -1)]
    [InlineData((byte)234, byte.MaxValue, -1)]
    [InlineData((byte)234, null, 1)]
    public static void TestCompareTo(byte b, object value, int expected)
    {
        if (value is byte)
        {
            Assert.Equal(expected, Math.Sign(b.CompareTo((byte)value)));
        }
        IComparable comparable = b;
        Assert.Equal(expected, Math.Sign(comparable.CompareTo(value)));
    }

    [Fact]
    public static void TestCompareTo_Invalid()
    {
        IComparable comparable = (byte)234;
        Assert.Throws<ArgumentException>(null, () => comparable.CompareTo("a")); // Obj is not a byte
        Assert.Throws<ArgumentException>(null, () => comparable.CompareTo(234)); // Obj is not a byte
    }

    [Theory]
    [InlineData((byte)78, (byte)78, true)]
    [InlineData((byte)78, (byte)0, false)]
    [InlineData((byte)0, (byte)0, true)]
    [InlineData((byte)78, null, false)]
    [InlineData((byte)78, "78", false)]
    [InlineData((byte)78, 78, false)]
    public static void TestEquals(byte b, object obj, bool expected)
    {
        if (obj is byte)
        {
            byte b2 = (byte)obj;
            Assert.Equal(expected, b.Equals(b2));
            Assert.Equal(expected, b.GetHashCode().Equals(b2.GetHashCode()));
            Assert.Equal(b, b.GetHashCode());
        }
        Assert.Equal(expected, b.Equals(obj));
    }

    public static IEnumerable<object[]> ToString_TestData()
    {
        NumberFormatInfo emptyFormat = NumberFormatInfo.CurrentInfo;
        yield return new object[] { (byte)0, "G", emptyFormat, "0" };
        yield return new object[] { (byte)123, "G", emptyFormat, "123" };
        yield return new object[] { byte.MaxValue, "G", emptyFormat, "255" };

        yield return new object[] { (byte)0x24, "x", emptyFormat, "24" };
        yield return new object[] { (byte)24, "N", emptyFormat, string.Format("{0:N}", 24.00) };

        NumberFormatInfo customFormat = new NumberFormatInfo();
        customFormat.NegativeSign = "#";
        customFormat.NumberDecimalSeparator = "~";
        customFormat.NumberGroupSeparator = "*";
        yield return new object[] { (byte)24, "N", customFormat, "24~00" };
    }

    [Theory]
    [MemberData(nameof(ToString_TestData))]
    public static void TestToString(byte b, string format, IFormatProvider provider, string expected)
    {
        // Format is case insensitive
        string upperFormat = format.ToUpperInvariant();
        string lowerFormat = format.ToLowerInvariant();

        string upperExpected = expected.ToUpperInvariant();
        string lowerExpected = expected.ToLowerInvariant();

        bool isDefaultProvider = (provider == null || provider == NumberFormatInfo.CurrentInfo);
        if (string.IsNullOrEmpty(format) || format.ToUpperInvariant() == "G")
        {
            if (isDefaultProvider)
            {
                Assert.Equal(upperExpected, b.ToString());
                Assert.Equal(upperExpected, b.ToString((IFormatProvider)null));
            }
            Assert.Equal(upperExpected, b.ToString(provider));
        }
        if (isDefaultProvider)
        {
            Assert.Equal(upperExpected, b.ToString(upperFormat));
            Assert.Equal(lowerExpected, b.ToString(lowerFormat));
            Assert.Equal(upperExpected, b.ToString(upperFormat, null));
            Assert.Equal(lowerExpected, b.ToString(lowerFormat, null));
        }
        Assert.Equal(upperExpected, b.ToString(upperFormat, provider));
        Assert.Equal(lowerExpected, b.ToString(lowerFormat, provider));
    }

    [Fact]
    public static void TestToString_Invalid()
    {
        byte b = 123;
        Assert.Throws<FormatException>(() => b.ToString("Y")); // Invalid format
        Assert.Throws<FormatException>(() => b.ToString("Y", null)); // Invalid format
    }

    public static IEnumerable<object[]> Parse_Valid_TestData()
    {
        NumberStyles defaultStyle = NumberStyles.Integer;
        NumberFormatInfo emptyFormat = new NumberFormatInfo();

        NumberFormatInfo customFormat = new NumberFormatInfo();
        customFormat.CurrencySymbol = "$";

        yield return new object[] { "0", defaultStyle, null, (byte)0 };
        yield return new object[] { "123", defaultStyle, null, (byte)123 };
        yield return new object[] { "+123", defaultStyle, null, (byte)123 };
        yield return new object[] { "  123  ", defaultStyle, null, (byte)123 };
        yield return new object[] { "255", defaultStyle, null, (byte)255 };

        yield return new object[] { "12", NumberStyles.HexNumber, null, (byte)0x12 };
        yield return new object[] { "10", NumberStyles.AllowThousands, null, (byte)10 };

        yield return new object[] { "123", defaultStyle, emptyFormat, (byte)123 };

        yield return new object[] { "123", NumberStyles.Any, emptyFormat, (byte)123 };
        yield return new object[] { "12", NumberStyles.HexNumber, emptyFormat, (byte)0x12 };
        yield return new object[] { "ab", NumberStyles.HexNumber, emptyFormat, (byte)0xab };
        yield return new object[] { "AB", NumberStyles.HexNumber, null, (byte)0xab };
        yield return new object[] { "$100", NumberStyles.Currency, customFormat, (byte)100 };
    }

    [Theory]
    [MemberData(nameof(Parse_Valid_TestData))]
    public static void TestParse(string value, NumberStyles style, IFormatProvider provider, byte expected)
    {
        byte result;
        // If no style is specified, use the (String) or (String, IFormatProvider) overload
        if (style == NumberStyles.Integer)
        {
            Assert.True(byte.TryParse(value, out result));
            Assert.Equal(expected, result);

            Assert.Equal(expected, byte.Parse(value));

            // If a format provider is specified, but the style is the default, use the (String, IFormatProvider) overload
            if (provider != null)
            {
                Assert.Equal(expected, byte.Parse(value, provider));
            }
        }

        // If a format provider isn't specified, test the default one, using a new instance of NumberFormatInfo
        Assert.True(byte.TryParse(value, style, provider ?? new NumberFormatInfo(), out result));
        Assert.Equal(expected, result);

        // If a format provider isn't specified, test the default one, using the (String, NumberStyles) overload
        if (provider == null)
        {
            Assert.Equal(expected, byte.Parse(value, style));
        }
        Assert.Equal(expected, byte.Parse(value, style, provider ?? new NumberFormatInfo()));
    }

    public static IEnumerable<object[]> Parse_Invalid_TestData()
    {
        NumberStyles defaultStyle = NumberStyles.Integer;

        NumberFormatInfo customFormat = new NumberFormatInfo();
        customFormat.CurrencySymbol = "$";
        customFormat.NumberDecimalSeparator = ".";

        yield return new object[] { null, defaultStyle, null, typeof(ArgumentNullException) };
        yield return new object[] { "", defaultStyle, null, typeof(FormatException) };
        yield return new object[] { " \t \n \r ", defaultStyle, null, typeof(FormatException) };
        yield return new object[] { "Garbage", defaultStyle, null, typeof(FormatException) };

        yield return new object[] { "ab", defaultStyle, null, typeof(FormatException) }; // Hex value
        yield return new object[] { "1E23", defaultStyle, null, typeof(FormatException) }; // Exponent
        yield return new object[] { "(123)", defaultStyle, null, typeof(FormatException) }; // Parentheses
        yield return new object[] { 100.ToString("C0"), defaultStyle, null, typeof(FormatException) }; // Currency
        yield return new object[] { 1000.ToString("N0"), defaultStyle, null, typeof(FormatException) }; // Thousands
        yield return new object[] { 67.90.ToString("F2"), defaultStyle, null, typeof(FormatException) }; // Decimal
        yield return new object[] { "+-123", defaultStyle, null, typeof(FormatException) };
        yield return new object[] { "-+123", defaultStyle, null, typeof(FormatException) };
        yield return new object[] { "+abc", NumberStyles.HexNumber, null, typeof(FormatException) };
        yield return new object[] { "-abc", NumberStyles.HexNumber, null, typeof(FormatException) };

        yield return new object[] { "- 123", defaultStyle, null, typeof(FormatException) };
        yield return new object[] { "+ 123", defaultStyle, null, typeof(FormatException) };

        yield return new object[] { "ab", NumberStyles.None, null, typeof(FormatException) }; // Hex value
        yield return new object[] { "  123  ", NumberStyles.None, null, typeof(FormatException) }; // Trailing and leading whitespace

        yield return new object[] { "67.90", defaultStyle, customFormat, typeof(FormatException) }; // Decimal

        yield return new object[] { "-1", defaultStyle, null, typeof(OverflowException) }; // < min value
        yield return new object[] { "256", defaultStyle, null, typeof(OverflowException) }; // > max value
        yield return new object[] { "(123)", NumberStyles.AllowParentheses, null, typeof(OverflowException) }; // Parentheses = negative

        yield return new object[] { "2147483648", defaultStyle, null, typeof(OverflowException) }; // Internally, Parse pretends we are inputting an Int32, so this overflows
    }

    [Theory]
    [MemberData(nameof(Parse_Invalid_TestData))]
    public static void TestParse_Invalid(string value, NumberStyles style, IFormatProvider provider, Type exceptionType)
    {
        byte result;
        // If no style is specified, use the (String) or (String, IFormatProvider) overload
        if (style == NumberStyles.Integer)
        {
            Assert.False(byte.TryParse(value, out result));
            Assert.Equal(default(byte), result);

            Assert.Throws(exceptionType, () => byte.Parse(value));

            // If a format provider is specified, but the style is the default, use the (String, IFormatProvider) overload
            if (provider != null)
            {
                Assert.Throws(exceptionType, () => byte.Parse(value, provider));
            }
        }

        // If a format provider isn't specified, test the default one, using a new instance of NumberFormatInfo
        Assert.False(byte.TryParse(value, style, provider ?? new NumberFormatInfo(), out result));
        Assert.Equal(default(byte), result);

        // If a format provider isn't specified, test the default one, using the (String, NumberStyles) overload
        if (provider == null)
        {
            Assert.Throws(exceptionType, () => byte.Parse(value, style));
        }
        Assert.Throws(exceptionType, () => byte.Parse(value, style, provider ?? new NumberFormatInfo()));
    }

    [Theory]
    [InlineData(NumberStyles.HexNumber | NumberStyles.AllowParentheses)]
    [InlineData(unchecked((NumberStyles)0xFFFFFC00))]
    public static void TestTryParse_InvalidNumberStyle_ThrowsArgumentException(NumberStyles style)
    {
        byte result = 0;
        Assert.Throws<ArgumentException>(() => byte.TryParse("1", style, null, out result));
        Assert.Equal(default(byte), result);

        Assert.Throws<ArgumentException>(() => byte.Parse("1", style));
        Assert.Throws<ArgumentException>(() => byte.Parse("1", style, null));
    }
}
