﻿using System;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Element with a real value
    /// </summary>
    public sealed class RealElement : Element<double>
    {
        /// <summary>
        /// Gets the equivalent of 1.0
        /// </summary>
        public override double One => 1.0;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public override double Value { get; set; }

        /// <summary>
        /// Magnitude (sum of absolute values)
        /// </summary>
        public override double Magnitude => Math.Abs(Value);

        /// <summary>
        /// Constructor
        /// </summary>
        public RealElement()
        {
            Value = 0.0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public RealElement(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Add a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Add(double operand) => Value += operand;

        /// <summary>
        /// Subtract a value
        /// </summary>
        /// <param name="operand">Operand</param>
        public override void Sub(double operand) => Value -= operand;

        /// <summary>
        /// Negate the value
        /// </summary>
        public override void Negate() => Value = -Value;

        /// <summary>
        /// Store the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AssignMultiply(Element<double> first, Element<double> second)
        {
            if (first == null || second == null)
                Value = 0.0;
            else
                Value = first.Value * second.Value;
        }

        /// <summary>
        /// Add the result of the multiplication
        /// </summary>
        /// <param name="first">First factor</param>
        /// <param name="second">Second factor</param>
        public override void AddMultiply(Element<double> first, Element<double> second)
        {
            if (first == null || second == null)
                return;
            Value += first.Value * second.Value;
        }

        /// <summary>
        /// Subtract the result of the multiplication
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public override void SubtractMultiply(Element<double> first, Element<double> second)
        {
            if (first == null || second == null)
                return;
            Value -= first.Value * second.Value;
        }

        /// <summary>
        /// Multiply with a factor
        /// </summary>
        /// <param name="factor">Factor</param>
        public override void Multiply(Element<double> factor)
        {
            if (factor == null)
                Value = 0.0;
            else
                Value *= factor.Value;
        }

        /// <summary>
        /// Multiply with a scalar
        /// </summary>
        /// <param name="factor">Scalar factor</param>
        public override void Scalar(double factor) => Value *= factor;

        /// <summary>
        /// Assign reciprocal
        /// </summary>
        /// <param name="denominator">Denominator</param>
        public override void AssignReciprocal(Element<double> denominator)
        {
            if (denominator == null)
                Value = double.NaN;
            else
                Value = 1.0 / denominator.Value;
        }

        /// <summary>
        /// Check for identity multiplier
        /// </summary>
        /// <returns></returns>
        public override bool EqualsOne()
        {
            return Math.Abs(Value).Equals(1.0);
        }

        /// <summary>
        /// Check for 0
        /// </summary>
        /// <returns></returns>
        public override bool EqualsZero()
        {
            return Value.Equals(0);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0:g5}".FormatString(Value);
        }
    }
}