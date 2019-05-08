using System;
using System.Windows;

namespace MultimodSynth.UI
{
    /// <summary>
    /// Представляет собой вариант ручки, стандартное значение которой находится на середине.
    /// </summary>
    public class BipolarKnob : Knob
    {
        /// <summary>
        /// Инициализирует новый объект типа BipolarKnob.
        /// </summary>
        public BipolarKnob()
        {
            coloredArcFigure.StartPoint = new System.Windows.Point(20, 4);
        }

        /// <summary>
        /// Обрабатывает вращение элементов.
        /// </summary>
        /// <param name="normalizedValue"></param>
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
