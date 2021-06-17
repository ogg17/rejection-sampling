using System;
using System.Collections.Generic;

namespace RejectionApp.Models
{
    [System.Serializable]
    public class Result
    {
        public List<Function> Functions { get; set; } = new();
        public int Count { get; set; } = 1;
        public int SampleSize { get; set; } = 100;
        public int Accuracy { get; set; } = 1;
        public int IntervalCount { get; set; } = 20;
        public double C { get; set; } = 1;
        public double Maximum { get; set; }
        public int A { get; set; }
        public int B { get; set; }

        public void SetInterval()
        {
            var a = int.MaxValue;
            var b = int.MinValue;

            foreach (var function in Functions)
            {
                if (function.Great >= function.Less) throw new Exception("Invalid interval!");
                if (function.Great < a) a = (int)function.Great;
                if (function.Less > b) b = (int)function.Less;
            }
            
            if(Math.Abs(b-a) < 1) throw new Exception("Invalid interval!");
            
            A = a;
            B = b;
        }
    }

    [System.Serializable]
    public class Function
    {
        public string Value { get; set; }
        public double Great { get; set; }
        public double Less { get; set; }
    }
}