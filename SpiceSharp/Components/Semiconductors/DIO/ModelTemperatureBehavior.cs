﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="DiodeModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : BaseTemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Conductance
        /// </summary>
        [ParameterName("cond"), ParameterInfo("Ohmic conductance")]
        public double Conductance { get; internal set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double VtNominal { get; protected set; }
        public double Xfc { get; protected set; }
        public double F2 { get; internal set; }
        public double F3 { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>("entity");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            if (!_mbp.NominalTemperature.Given)
            {
                _mbp.NominalTemperature.RawValue = simulation.RealState.NominalTemperature;
            }
            VtNominal = Circuit.KOverQ * _mbp.NominalTemperature;

            // limit grading coeff to max of 0.9
            if (_mbp.GradingCoefficient > 0.9)
            {
                _mbp.GradingCoefficient.RawValue = 0.9;
                CircuitWarning.Warning(this, "{0}: grading coefficient too large, limited to 0.9".FormatString(Name));
            }

            // limit activation energy to min of 0.1
            if (_mbp.ActivationEnergy < 0.1)
            {
                _mbp.ActivationEnergy.RawValue = 0.1;
                CircuitWarning.Warning(this, "{0}: activation energy too small, limited to 0.1".FormatString(Name));
            }

            // limit depletion cap coeff to max of .95
            if (_mbp.DepletionCapCoefficient > 0.95)
            {
                _mbp.DepletionCapCoefficient.RawValue = 0.95;
                CircuitWarning.Warning(this, "{0}: coefficient Fc too large, limited to 0.95".FormatString(Name));
            }

            if (_mbp.Resistance > 0)
                Conductance = 1 / _mbp.Resistance;
            else
                Conductance = 0;
            Xfc = Math.Log(1 - _mbp.DepletionCapCoefficient);

            F2 = Math.Exp((1 + _mbp.GradingCoefficient) * Xfc);
            F3 = 1 - _mbp.DepletionCapCoefficient * (1 + _mbp.GradingCoefficient);
        }
    }
}
