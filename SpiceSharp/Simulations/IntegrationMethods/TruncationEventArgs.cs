﻿using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for truncating the timestep
    /// </summary>
    public class TruncationEventArgs : EventArgs
    {
        /// <summary>
        /// The newly calculated delta
        /// This delta can only be made smaller using the setter.
        /// The getter will return min(2 * current delta, new delta)
        /// </summary>
        public double Delta
        {
            get => Math.Min(2.0 * CurrentDelta, _delta);
            set => _delta = Math.Min(value, _delta);
        }

        private double _delta;

        /// <summary>
        /// Gets the current timestep
        /// </summary>
        public double CurrentDelta { get; }
        
        /// <summary>
        /// Gets the simulation where the truncation event is called
        /// </summary>
        public TimeSimulation Simulation { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <param name="currentDelta">Initial timestep</param>
        public TruncationEventArgs(TimeSimulation simulation, double currentDelta)
        {
            Simulation = simulation;
            CurrentDelta = currentDelta;
            _delta = double.PositiveInfinity;
        }
    }
}
