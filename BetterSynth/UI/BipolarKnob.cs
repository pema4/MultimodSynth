using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BetterSynth.UI
{
    public class BipolarKnob : Knob
    {
        public BipolarKnob() : base()
        {
            coloredArcFigure.StartPoint = new System.Windows.Point(20, 4);
        }

        protected override void Rotate(double normalizedValue)
        {
            var angle = Math.PI * (1.25 - 1.5 * normalizedValue);
            var newPoint = new Point
            {
                X = 20 + 16 * Math.Cos(angle),
                Y = 20 - 16 * Math.Sin(angle),
            };
            if (angle < Math.PI / 2)
                coloredArc.SweepDirection = System.Windows.Media.SweepDirection.Clockwise;
            else
                coloredArc.SweepDirection = System.Windows.Media.SweepDirection.Counterclockwise;
            coloredArc.Point = newPoint;
            rotateTransform.Angle = 270 * normalizedValue;
        }
    }
}
