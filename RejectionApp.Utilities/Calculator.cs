using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MathNet.Numerics;
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
            var result = 0.0;
            if (equals == _functionStr)
            {
                result = PerformFunctionRpn(_functionStrRpn, x);
                return result;
            }

            _functionStr = equals;
            _functionStrRpn = TranslateToRpn(equals);
            
            result = PerformFunctionRpn(_functionStrRpn, x);
            return result;
        }

        public static double PerformDensity(Models.Result myResult, double x)
        {
            foreach (var function in myResult.Functions)
            {
                if (x <= function.Less + Math.Pow(10, -myResult.Accuracy - 1) && x >= function.Great)
                {
                    var density = myResult.C * PerformFunction(function.Value, x);
                    return density;
                }
            }
            return 0.0;
        }

        public static double FindNormal(Models.Result myResult)
        {
            var integ = IntegrateDensity(myResult, myResult.A, myResult.B);
            return 1 / integ;
        }

        public static List<double> GenerateSampling(Models.Result myResult)
        {
            var sampling = new List<double>();
            var random = new Random();
            double M = PerformDensity(myResult, myResult.Maximum);

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

        public static double FindMaximum(Models.Result myResult)
        {
            var maximum = 0.0;
            var perfMaximum = 0.0;
            var accuracy = Math.Pow(10, -myResult.Accuracy);

            double a;
            double b;
            foreach (var function in myResult.Functions)
            {
                a = function.Great;
                b = function.Less;

                for (var x = a; x < b + accuracy/2; x += accuracy)
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

        public static List<int> CalculateFrequencies(Result myResult, List<double> sampling)
        {
            if (myResult.SampleSize == 0)
                return null;
            
            var frequency = new int[myResult.IntervalCount];

            foreach (var value in sampling)
            {
                int index = (int)((value - myResult.A) / (myResult.B - myResult.A) * myResult.IntervalCount);
                ++frequency[index];
            }

            return frequency.ToList();
        }
        
        public static List<double> СalculateProbabilities(Result myResult, DrawParam myParam)
        {
            var probability = new List<double>();

            double length = (myResult.B - myResult.A) / (double)myResult.IntervalCount;

            for (double x = myParam.xMinimum; x < myParam.xMaximum - length/2; x += length)
            {
                double Px = IntegrateDensity(myResult, x, x+length);
                probability.Add(Px);
            }

            return probability;
        }
        
        public static double CalculateX2(Result myResult, List<double> probability, List<int> frequency)
        {
            double x2Statistics;
            
            if (frequency == null || probability == null || frequency.Count == 0 || frequency.Count != probability.Count)
            {
                x2Statistics = 0;
                return x2Statistics;
            }

            double num1 = 0.0;

            for (int i = 0; i < myResult.IntervalCount; i++)
            {
                double num2 = (double)frequency[i] / myResult.SampleSize - probability[i];
                num1 += num2 * num2 / probability[i];
            }
            x2Statistics = num1 * myResult.SampleSize;
            
            return x2Statistics;
        }
        
        public static double SignificanceLevelXi2(Result myResult, double xi2, int intervalsCount)
        {
            // if (xi2 <= 0.0) return 1.0;
            //
            // int num1 = intervalsCount / 2;
            // double num2 = xi2 / 2.0;
            // double num3 = 1.0;
            // double num4 = num3;
            //
            // for (int index = 1; index < num1; ++index)
            // {
            //     num3 = num3 * num2 / (double)index;
            //     num4 += num3;
            // }
            //
            // return num4 * Math.Exp(-num2);
            double result = 0;
            var k = intervalsCount - 1;
            var x = xi2;
            if (intervalsCount % 2 != 0)
                result = 1 - SpecialFunctions.GammaLowerIncomplete(k/2.0, x/2)/SpecialFunctions.Gamma(k/2.0);
            else 
                result = 1 - MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(xx => Math.Pow(0.5, k / 2.0)*
                    Math.Pow(xx, k/2.0 - 1)*
                    Math.Exp(-xx/2)/SpecialFunctions.Gamma(k/2.0), 
                    0.0, x, (int)Math.Pow(10, myResult.Accuracy));

            return result;
        }
        
        public static double IntegrateDensity(Result myResult, double a, double b)
        {
            var n = Math.Pow(10, myResult.Accuracy);
            var result = 0.0;

            
            var h = (b - a) / n;
            double sum = 0;
            for (var i = 0; i < n; i++)
            {
                var x = a + h * (i + 0.5);
                sum += PerformDensity(myResult, x);
            }
            result = h * sum;
            
            return result;
        }
    }
}