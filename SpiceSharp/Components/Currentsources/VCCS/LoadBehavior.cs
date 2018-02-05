﻿using SpiceSharp.Simulations;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosourceNode, contNegateNode;
        protected Element<double> PosControlPosPtr { get; private set; }
        protected Element<double> PosControlNegPtr { get; private set; }
        protected Element<double> NegControlPosPtr { get; private set; }
        protected Element<double> NegControlNegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public double GetVoltage(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));
            
            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("i"), PropertyName("c"), PropertyInfo("Current")]
        public double GetCurrent(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return (state.Solution[posNode] - state.Solution[negNode]) * bp.Coefficient;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(RealState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            double v = state.Solution[posNode] - state.Solution[negNode];
            return v * v * bp.Coefficient;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(string propertyName)
        {
            // Avoid reflection for common components
            switch (propertyName)
            {
                case "v": return GetVoltage;
                case "i":
                case "c": return GetCurrent;
                case "p": return GetPower;
                default: return null;
            }
        }

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
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
            contPosourceNode = pins[2];
            contNegateNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix<double> matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            PosControlPosPtr = matrix.GetElement(posNode, contPosourceNode);
            PosControlNegPtr = matrix.GetElement(posNode, contNegateNode);
            NegControlPosPtr = matrix.GetElement(negNode, contPosourceNode);
            NegControlNegPtr = matrix.GetElement(negNode, contNegateNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosControlPosPtr = null;
            PosControlNegPtr = null;
            NegControlPosPtr = null;
            NegControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosControlPosPtr.Add(bp.Coefficient);
            PosControlNegPtr.Sub(bp.Coefficient);
            NegControlPosPtr.Sub(bp.Coefficient);
            NegControlNegPtr.Add(bp.Coefficient);
        }
    }
}