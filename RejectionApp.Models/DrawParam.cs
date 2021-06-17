using System;

namespace RejectionApp.Models
{
    [System.Serializable]
    public class DrawParam
    {
        public int Offset { get; set; } = 40;
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;

        public int xMinimum { get; set; } = 0;
        public int xMaximum { get; set; } = 10;
        public int yMinimum { get; set; } = 0;
        public int yMaximum { get; set; } = 1;

        public float ChangeX(float varX)
        {
            return (Width - 2 * Offset) * (varX - xMinimum) / (xMaximum - xMinimum) + Offset;
        }

        public float ChangeY(float varY)
        {
            return (Height - 2 * Offset) * (-varY + yMaximum) / (yMaximum - yMinimum) + Offset;
        }

        public void UpdateParam(Models.Result myResult)
        {
            xMinimum = myResult.A;
            xMaximum = myResult.B;
        }
    }
}