using System;
using System.Collections.Generic;
using RejectionApp.Models;

namespace RejectionApp.Utilities
{
    public static class DrawPlots
    {
        public static List<Point> DrawGraph(DrawParam myParam, Result myResult)
        {
            if (myResult.Functions[0] == null)
                return null;

            var graph = new List<Point>();

            float x, y;
            var delta = ((float) myParam.xMaximum - myParam.xMinimum) / ((float) myParam.Width - 2 * myParam.Offset);
            for (x = myParam.xMinimum; x <= myParam.xMaximum; x += delta)
            {
                y = (float) Calculator.PerformDensity(myResult, x);
                if (!float.IsNaN(y))
                    graph.Add(new Point {X = myParam.ChangeX(x), Y = myParam.ChangeY(y)});
            }

            return graph;
        }

        public static List<Rect> DrawHistogram(DrawParam myParam, int intervalsCount, List<int> frequency, int sampleSize)
        {
            var histogram = new List<Rect>();
            
            var ixPrev = myParam.ChangeX(myParam.xMinimum);
            var delta = (myParam.xMaximum - myParam.xMinimum) / (float)(intervalsCount);
            var x = myParam.xMinimum + delta;
            var ix = 0.0f;
            for (int i = 0; i < intervalsCount; i++)
            {
                ix = myParam.ChangeX(x);
                float iyAround = myParam.ChangeY(frequency[i] / (float)sampleSize / delta);
                histogram.Add(new Rect(ixPrev, iyAround, ix - ixPrev, myParam.Height - myParam.Offset - iyAround));
                ixPrev = ix;
                x += delta;
            }

            return histogram;
        }
    }

    [Serializable]
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    [Serializable]
    public class Rect
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}