﻿using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;
using System;

namespace SpiceSharp.Components.VoltageControlledVoltagesourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("i"), PropertyInfo("Output current")]
        public double GetCurrent(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [PropertyName("v"), PropertyInfo("Output current")]
        public double GetVoltage(RealState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[posNode] - state.Solution[negNode];
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public double GetPower(RealState state)
        { 
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq] * (state.Solution[posNode] - state.Solution[negNode]);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, contPosourceNode, contNegateNode;
        public int BranchEq { get; private set; }
        protected Element<double> PosBranchPtr { get; private set; }
        protected Element<double> NegBranchPtr { get; private set; }
        protected Element<double> BranchPosPtr { get; private set; }
        protected Element<double> BranchNegPtr { get; private set; }
        protected Element<double> BranchControlPosPtr { get; private set; }
        protected Element<double> BranchControlNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create exports
        /// </summary>
        /// <param name="propertyName">Parameter</param>
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
        /// Setup behavior
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
        /// Connect
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
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            BranchEq = nodes.Create(Name.Grow("#branch"), Node.NodeType.Current).Index;
            PosBranchPtr = matrix.GetElement(posNode, BranchEq);
            NegBranchPtr = matrix.GetElement(negNode, BranchEq);
            BranchPosPtr = matrix.GetElement(BranchEq, posNode);
            BranchNegPtr = matrix.GetElement(BranchEq, negNode);
            BranchControlPosPtr = matrix.GetElement(BranchEq, contPosourceNode);
            BranchControlNegPtr = matrix.GetElement(BranchEq, contNegateNode);
        }
        
        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            PosBranchPtr = null;
            NegBranchPtr = null;
            BranchPosPtr = null;
            BranchNegPtr = null;
            BranchControlPosPtr = null;
            BranchControlNegPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Add(1.0);
            BranchPosPtr.Add(1.0);
            NegBranchPtr.Sub(1.0);
            BranchNegPtr.Sub(1.0);
            BranchControlPosPtr.Sub(bp.Coefficient);
            BranchControlNegPtr.Add(bp.Coefficient);
        }
    }
}