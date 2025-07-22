using static System.Math;

namespace TGK;

public static class DoubleExtensions
{
    public static bool IsNotNanOrInfinity(this double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }
        
    public static bool IsAlmostEqualTo(this double value, double other, double tolerance = 1e-10)
    {
        if (tolerance < 1)
            return Abs(Round(other - value, (int)-Log10(tolerance))) <= tolerance;
        return Abs(other - value) <= tolerance;
    }

    public static bool IsGreaterThan(this double value, double other, double tolerance = 1e-10)
    {
        return value > other + tolerance;
    }          
        
    public static bool IsGreaterThanOrEqualTo(this double value, double other, double tolerance = 1e-10)
    {
        return Round(other - value, (int)-Log10(tolerance)) <= tolerance;
    }        
        
    public static bool IsLessThan(this double value, double other, double tolerance = 1e-10)
    {
        return value < other - tolerance;
    }        
        
    public static bool IsLessThanOrEqualTo(this double value, double other, double tolerance = 1e-10)
    {
        return Round(value - other, (int)-Log10(tolerance)) <= tolerance;
    }

    public static double Clamp(this double value, double min, double max)
    {
        return Max(min, Min(max, value));
    }

    /// <summary>
    /// Get the next greatest double value.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/15332024/200443
    /// </remarks>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double GetNext(this double value)
    {
        if (!value.IsNotNanOrInfinity()) throw new ArgumentException($"{nameof(value)} can not be NaN or infinite.");
            
        long bits = BitConverter.DoubleToInt64Bits(value);
        return value switch
        {
            > 0 => BitConverter.Int64BitsToDouble(bits + 1),
            < 0 => BitConverter.Int64BitsToDouble(bits - 1),
            _ => double.Epsilon
        };
    }
        
    /// <summary>
    /// Get the previous smallest double value.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/15332024/200443
    /// </remarks>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double GetPrevious(this double value)
    {
        if (!value.IsNotNanOrInfinity()) throw new ArgumentException($"{nameof(value)} can not be NaN or infinite.");
            
        long bits = BitConverter.DoubleToInt64Bits(value);
        return value switch
        {
            > 0 => BitConverter.Int64BitsToDouble(bits - 1),
            < 0 => BitConverter.Int64BitsToDouble(bits + 1),
            _ => -double.Epsilon
        };
    }
}