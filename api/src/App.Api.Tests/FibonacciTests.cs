using App.Api.Models;
using Xunit;

namespace App.Api.Tests;

public class FibonacciTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 5)]
    [InlineData(5, 8)]
    [InlineData(8, 13)]
    [InlineData(13, 13)] // tope
    public void Next_ReturnsCorrectValue(int current, int expected)
    {
        Assert.Equal(expected, Fibonacci.Next(current));
    }

    [Theory]
    [InlineData(0, 0)] // mínimo
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(5, 3)]
    [InlineData(8, 5)]
    [InlineData(13, 8)]
    public void Previous_ReturnsCorrectValue(int current, int expected)
    {
        Assert.Equal(expected, Fibonacci.Previous(current));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(8, true)]
    [InlineData(13, true)]
    [InlineData(4, false)]
    [InlineData(6, false)]
    [InlineData(14, false)]
    [InlineData(-1, false)]
    public void IsValid_ReturnsCorrectValue(int value, bool expected)
    {
        Assert.Equal(expected, Fibonacci.IsValid(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    public void NextThenPrevious_IsRoundTrip(int value)
    {
        Assert.Equal(value, Fibonacci.Previous(Fibonacci.Next(value)));
    }
}
