namespace App.Api.Models;

public static class Fibonacci
{
    private static readonly int[] Sequence = [0, 1, 2, 3, 5, 8, 13];

    public static int Next(int current)
    {
        var idx = Array.IndexOf(Sequence, current);
        return idx >= 0 && idx < Sequence.Length - 1 ? Sequence[idx + 1] : current;
    }

    public static int Previous(int current)
    {
        var idx = Array.IndexOf(Sequence, current);
        return idx > 0 ? Sequence[idx - 1] : 0;
    }

    public static bool IsValid(int value) => Array.IndexOf(Sequence, value) >= 0;
}
