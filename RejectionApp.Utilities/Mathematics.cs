namespace RejectionApp.Utilities
{
    public class Mathematics
    {
        public static int Factorial(int x)
        {
            if (x == 0)
                return 1;

            return x * Factorial(x - 1);
        }
    }
}