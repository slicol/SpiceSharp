﻿using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Interface for a behaviour
    /// </summary>
    public interface ICircuitObjectBehavior
    {
        /// <summary>
        /// Setup the circuit object behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        void Setup(ICircuitObject component, Circuit ckt);

        /// <summary>
        /// Execute the circuit object behaviour
        /// </summary>
        /// <param name="ckt"></param>
        void Execute(Circuit ckt);

        /// <summary>
        /// Unsetup the object behaviour
        /// </summary>
        void Unsetup();
    }
}