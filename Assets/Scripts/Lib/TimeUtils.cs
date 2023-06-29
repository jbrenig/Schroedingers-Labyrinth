namespace Lib
{
    public class TimeUtils
    {
        public static string TimeToUnitsString(double timeInSeconds, bool allowScientificNotation = false)
        {
            if (timeInSeconds == 0)
            {
                return "0s";
            }
            if (timeInSeconds >= 1)
            {
                return $"{timeInSeconds:F1}s";
            }

            if (timeInSeconds >= 1e-3)
            {
                return $"{timeInSeconds * 1e3:F1}ms";
            }

            if (timeInSeconds >= 1e-6)
            {
                return $"{timeInSeconds * 1e6:F1}µs";
            }

            if (timeInSeconds >= 1e-9)
            {
                return $"{timeInSeconds * 1e9:F1}ns";
            }

            if (timeInSeconds >= 1e-12)
            {
                return $"{timeInSeconds * 1e12:F1}ps";
            }

            if (timeInSeconds >= 1e-15)
            {
                return $"{timeInSeconds * 1e15:F1}fs";
            }

            if (allowScientificNotation)
            {
                return $"{timeInSeconds:e2}s";
            }
            else
            {
                return "0s";
            }
        }
    }
}