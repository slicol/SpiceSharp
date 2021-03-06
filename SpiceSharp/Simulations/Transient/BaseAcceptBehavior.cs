﻿using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for accepting a timepoint
    /// </summary>
    public abstract class BaseAcceptBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected BaseAcceptBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Accept(TimeSimulation simulation);
    }
}
