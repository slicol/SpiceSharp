﻿using System;
using System.Numerics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Capacitor"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary paramters and behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode;
        protected Element<Complex> PosPosPtr { get; private set; }
        protected Element<Complex> NegNegPtr { get; private set; }
        protected Element<Complex> PosNegPtr { get; private set; }
        protected Element<Complex> NegPosPtr { get; private set; }

        [PropertyName("v"), PropertyInfo("Capacitor voltage")]
        public Complex GetVoltage(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Capacitor current")]
        public Complex GetCurrent(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            Complex conductance = state.Laplace * bp.Capacitance.Value;
            return (state.Solution[posNode] - state.Solution[negNode]) * conductance;
        }
        [PropertyName("p"), PropertyInfo("Capacitor power")]
        public Complex GetPower(ComplexState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            Complex conductance = state.Laplace * bp.Capacitance.Value;
            Complex voltage = state.Solution[posNode] - state.Solution[negNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>("entity");
        }
        
        /// <summary>
        /// Connect behavior
        /// </summary>
        /// <param name="pins"></param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public override void GetMatrixPointers(Matrix<Complex> matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));


            PosPosPtr = matrix.GetElement(posNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
            NegPosPtr = matrix.GetElement(negNode, posNode);
            PosNegPtr = matrix.GetElement(posNode, negNode);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;
            var val = state.Laplace * bp.Capacitance.Value;

            // Load the matrix
            PosPosPtr.Add(val);
            NegNegPtr.Add(val);
            PosNegPtr.Sub(val);
            NegPosPtr.Sub(val);
        }
    }
}