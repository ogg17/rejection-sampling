using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RejectionApp.Models;

namespace RejectionApp.Utilities
{
    public static class Calculator
    {
        private static readonly List<string> Prefix = new() {"sin", "cos", "ln", "lg", "tg", "ctg", "sqrt", "abs"};
        private static readonly List<string> Binary = new() {"^", "*", "/", "\\", "+", "-"};
        private static readonly List<int> BinPriority = new() {0, 1, 1, 1, 2, 2};
        private static readonly List<string> Postfix = new() {"!"};
        private static readonly List<string> Parentheses = new() {"(", ")"};

        private static readonly string Separator =
            @"(sin)|(cos)|(ln)|(lg)|(tg)|(ctg)|(sqrt)|(abs)|(\^)|(\*)|(/)|(\+)|(-)|(!)|(\()|(\))|(\\)|\s+";

        private static List<string> _functionStrRpn;
        private static string _functionStr;

        private static List<string> TranslateToRpn(string source)
        {
            var stack = new Stack<string>();
            //List<string> input = Separate(source, separator.ToList());
            var input = Regex.Split(source, Separator)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            var output = new List<string>();

            foreach (var ch in input)
            {
                if (Prefix.Contains(ch) || ch == "(")
                {
                    stack.Push(ch);
                }
                else if (ch == ")")
                {
                    while (stack.Peek() != "(") output.Add(stack.Pop());
                    stack.Pop();
                }
                else if (Binary.Contains(ch))
                {
                    while (stack.Count > 0 && (Prefix.Contains(stack.Peek()) || Binary.Contains(stack.Peek()) &&
                        BinPriority[Binary.FindIndex(str => str == stack.Peek())] <= 
                        BinPriority[Binary.FindIndex(str => str == ch)]))
                    {
                        output.Add(stack.Pop());
                    }

                    stack.Push(ch);
                }
                else
                {
                    output.Add(ch);
                }
            }

            while (stack.Count > 0)
                output.Add(stack.Pop());

            return output;
        }

        private static double PerformFunctionRpn(List<string> rpn, double x)
        {
            var operations = Prefix.Union(Binary).Union(Postfix).ToList();
            var stack = new Stack<double>();

            foreach (var rpnChar in rpn)
            {
                if (rpnChar.ToLower() == "x")
                {
                    stack.Push(x);
                }
                else if (operations.Contains(rpnChar))
                {
                    if (Binary.Contains(rpnChar))
                    {
                        var num2 = stack.Pop();
                        var num1 = stack.Count > 0 ? stack.Pop() : 0;
                        double output;
                        switch (rpnChar)
                        {
                            case "+":
                                output = num1 + num2;
                                break;
                            case "-":
                                output = num1 - num2;
                                break;
                            case "*":
                                output = num1 * num2;
                                break;
                            case "/":
                                output = num1 / num2;
                                break;
                            case "\\":
                                output = num1 / num2;
                                break;
                            case "^":
                                output = Math.Pow(num1, num2);
                                break;
                            default:
                                output = 0;
                                break;
                        }

                        stack.Push(output);
                    }
                    else
                    {
                        var num = stack.Pop();
                        double output;
                        switch (rpnChar)
                        {
                            case "sin":
                                output = Math.Sin(num);
                                break;
                            case "cos":
                                output = Math.Cos(num);
                                break;
                            case "ln":
                                output = Math.Log(num);
                                break;
                            case "lg":
                                output = Math.Log10(num);
                                break;
                            case "tg":
                                output = Math.Tan(num);
                                break;
                            case "ctg":
                                output = 1 / Math.Tan(num);
                                break;
                            case "sqrt":
                                output = Math.Sqrt(num);
                                break;
                            case "abs":
                                output = Math.Abs(num);
                                break;
                            case "!":
                                output = Mathematics.Factorial((int) num);
                                break;
                            default:
                                output = 0;
                                break;
                        }

                        stack.Push(output);
                    }
                }
                else
                {
                    stack.Push(double.Parse(rpnChar));
                }
            }

            return stack.Pop();
        }

        public static double PerformFunction(string equals, double x)
        {
            if (equals == _functionStr)
                return PerformFunctionRpn(_functionStrRpn, x);

            _functionStr = equals;
            _functionStrRpn = TranslateToRpn(equals);

            return PerformFunctionRpn(_functionStrRpn, x);
        }

        public static double PerformDensity(Models.Result myResult, double x)
        {
            double density = float.NaN;
            
            for (int i = 0; i < myResult.Functions.Count; i++)
            {
                if (x <= myResult.Functions[i].Less && x >= myResult.Functions[i].Great)
                    density = myResult.C * PerformFunction(myResult.Functions[i].Value, x);
            }

            //if (density > 1.0) density = double.NaN;
            
            return density;
        }

        public static double FindNormal(Models.Result myResult, double accuracy)
        {
            var n = Math.Pow(10, accuracy);

            var result = 0.0;

            foreach (var function in myResult.Functions)
            {
                var h = (function.Less - function.Great) / n;
                double sum = (PerformDensity(myResult, function.Great) + PerformDensity(myResult, function.Less)) / 2;
                for (var i = 1; i < n; i++)
                {
                    var x = function.Great + h * i;
                    sum += PerformDensity(myResult, x);
                }
                result += h * sum;
            }

            double c = 1/result;
            
            return c;
        }

        public static List<double> GenerateSampling(Models.Result myResult, double accuracy, double maximum)
        {
            var sampling = new List<double>();
            var random = new Random();
            double M = PerformDensity(myResult, maximum);

            double eps;
            double n;

            for (int index = 0; index < myResult.SampleSize; ++index)
            {
                do
                {
                    eps = myResult.A + random.NextDouble() * (myResult.B - myResult.A);
                    n = random.NextDouble() * M;
                } while (n > PerformDensity(myResult, eps));
                sampling.Add(eps);
            }
            
            return sampling;
        }

        public static double FindMaximum(Models.Result myResult, double accuracy)
        {
            var maximum = 0.0;
            var perfMaximum = 0.0;
            accuracy = 1 / Math.Pow(10, accuracy);

            double a;
            double b;
            foreach (var function in myResult.Functions)
            {
                a = function.Great;
                b = function.Less;

                for (var x = a; x <= b; x += accuracy)
                {
                    var f = PerformFunction(function.Value, x);
                    if (f < 0)
                        throw new Exception("Density cannot be less than zero!");
                    if (f > perfMaximum)
                    {
                        maximum = x;
                        perfMaximum = f;
                    }
                }
            }

            // Console.WriteLine($"A:{myResult.A};B:{myResult.B}");
            //
            // a = maximum - accuracy;
            // a = a < myResult.A ? myResult.A : a;
            // b = maximum + accuracy;
            // b = b > myResult.B ? myResult.B : b;
            //
            // Console.WriteLine($"a:{a};b:{b}");
            //
            // var phi = (1 + Math.Sqrt(5)) / 2;
            // double x1, x2, y1, y2;
            //
            // while (Math.Abs(b-a) > accuracy / 10)
            // {
            //     x1=b - (b - a) / phi;
            //     x2=a + (b - a) / phi;
            //     y1 = PerformDensity(myResult, x1);
            //     y2 = PerformDensity(myResult, x2);
            //     if (y1 >= y2) a = x1;
            //     else b = x2;
            // }
            // maximum = a + b / 2;

            return maximum;
        }

        public static List<int> calculateFrequencies(Result myResult, DrawParam myParam, int intervalsCount, List<double> sampling)
        {
            if (myResult.SampleSize == 0)
                return null;
            else
            {
                var frequency = new int[intervalsCount];

                foreach (var value in sampling)
                {
                    int index = (int)((value - myResult.A) / (myResult.B - myResult.A) * intervalsCount);
                    ++frequency[index];
                }

                return frequency.ToList();
            }
        }
    }
}