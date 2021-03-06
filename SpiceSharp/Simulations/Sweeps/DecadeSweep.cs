﻿using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Logarithmic sweep (points per decade)
    /// </summary>
    public class DecadeSweep : Sweep<double>
    {
        /// <summary>
        /// Gets the points
        /// </summary>
        public override IEnumerable<double> Points
        {
            get
            {
                // Initialize
                Current = Initial;

                // Go through the points
                for (int i = 0; i < Count; i++)
                {
                    yield return Current;

                    // Go to the next point
                    Current = Current * _freqDelta;
                }
            }
        }

        /// <summary>
        /// Multiplication factor
        /// </summary>
        private readonly double _freqDelta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial value</param>
        /// <param name="final">Final value</param>
        /// <param name="steps">Steps per decade</param>
        public DecadeSweep(double initial, double final, int steps)
        {
            if (final * initial <= 0)
                throw new CircuitException("Invalid decade sweep from {0} to {1}".FormatString(initial, final));

            Initial = initial;
            Final = final;
            _freqDelta = Math.Exp(Math.Log(10.0) / steps);
            Count = (int)Math.Floor(Math.Log(final / initial) / Math.Log(_freqDelta) + 0.25) + 1;
        }
    }
}
