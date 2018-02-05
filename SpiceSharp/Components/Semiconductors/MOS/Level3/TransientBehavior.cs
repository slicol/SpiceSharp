﻿using System;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode, drainNodePrime, sourceNodePrime;
        protected Element<double> DrainDrainPtr { get; private set; }
        protected Element<double> GateGatePtr { get; private set; }
        protected Element<double> SourceSourcePtr { get; private set; }
        protected Element<double> BulkBulkPtr { get; private set; }
        protected Element<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainDrainPrimePtr { get; private set; }
        protected Element<double> GateBulkPtr { get; private set; }
        protected Element<double> GateDrainPrimePtr { get; private set; }
        protected Element<double> GateSourcePrimePtr { get; private set; }
        protected Element<double> SourceSourcePrimePtr { get; private set; }
        protected Element<double> BulkDrainPrimePtr { get; private set; }
        protected Element<double> BulkSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeDrainPtr { get; private set; }
        protected Element<double> BulkGatePtr { get; private set; }
        protected Element<double> DrainPrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePtr { get; private set; }
        protected Element<double> DrainPrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// States
        /// </summary>
        StateHistory Vgs;
        StateHistory Vds;
        StateHistory Vbs;
        StateHistory Capgs;
        StateHistory Capgd;
        StateHistory Capgb;
        StateDerivative Qgs;
        StateDerivative Qgd;
        StateDerivative Qgb;
        StateDerivative Qbd;
        StateDerivative Qbs;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [PropertyName("cbd"), PropertyInfo("Bulk-Drain capacitance")]
        public double CapBD { get; protected set; }
        [PropertyName("cbs"), PropertyInfo("Bulk-Source capacitance")]
        public double CapBS { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

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
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            load = provider.GetBehavior<LoadBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
            drainNode = pins[0];
            gateNode = pins[1];
            sourceNode = pins[2];
            bulkNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix<double> matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            drainNodePrime = load.DrainNodePrime;
            sourceNodePrime = load.SourceNodePrime;

            // Get matrix elements
            DrainDrainPtr = matrix.GetElement(drainNode, drainNode);
            GateGatePtr = matrix.GetElement(gateNode, gateNode);
            SourceSourcePtr = matrix.GetElement(sourceNode, sourceNode);
            BulkBulkPtr = matrix.GetElement(bulkNode, bulkNode);
            DrainPrimeDrainPrimePtr = matrix.GetElement(drainNodePrime, drainNodePrime);
            SourcePrimeSourcePrimePtr = matrix.GetElement(sourceNodePrime, sourceNodePrime);
            DrainDrainPrimePtr = matrix.GetElement(drainNode, drainNodePrime);
            GateBulkPtr = matrix.GetElement(gateNode, bulkNode);
            GateDrainPrimePtr = matrix.GetElement(gateNode, drainNodePrime);
            GateSourcePrimePtr = matrix.GetElement(gateNode, sourceNodePrime);
            SourceSourcePrimePtr = matrix.GetElement(sourceNode, sourceNodePrime);
            BulkDrainPrimePtr = matrix.GetElement(bulkNode, drainNodePrime);
            BulkSourcePrimePtr = matrix.GetElement(bulkNode, sourceNodePrime);
            DrainPrimeSourcePrimePtr = matrix.GetElement(drainNodePrime, sourceNodePrime);
            DrainPrimeDrainPtr = matrix.GetElement(drainNodePrime, drainNode);
            BulkGatePtr = matrix.GetElement(bulkNode, gateNode);
            DrainPrimeGatePtr = matrix.GetElement(drainNodePrime, gateNode);
            SourcePrimeGatePtr = matrix.GetElement(sourceNodePrime, gateNode);
            SourcePrimeSourcePtr = matrix.GetElement(sourceNodePrime, sourceNode);
            DrainPrimeBulkPtr = matrix.GetElement(drainNodePrime, bulkNode);
            SourcePrimeBulkPtr = matrix.GetElement(sourceNodePrime, bulkNode);
            SourcePrimeDrainPrimePtr = matrix.GetElement(sourceNodePrime, drainNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            BulkBulkPtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateBulkPtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            BulkDrainPrimePtr = null;
            BulkSourcePrimePtr = null;
            DrainPrimeSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            BulkGatePtr = null;
            DrainPrimeGatePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            DrainPrimeBulkPtr = null;
            SourcePrimeBulkPtr = null;
            SourcePrimeDrainPrimePtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            Vgs = states.CreateHistory();
            Vds = states.CreateHistory();
            Vbs = states.CreateHistory();
            Capgs = states.CreateHistory();
            Capgd = states.CreateHistory();
            Capgb = states.CreateHistory();
            Qgs = states.CreateDerivative();
            Qgd = states.CreateDerivative();
            Qgb = states.CreateDerivative();
            Qbd = states.CreateDerivative();
            Qbs = states.CreateDerivative();
        }

        /// <summary>
        /// Gets DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double EffectiveLength,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw, capgs = 0.0, capgd = 0.0, capgb = 0.0;

            vbs = load.VoltageBS;
            vgs = load.VoltageGS;
            vds = load.VoltageDS;
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;
            von = mbp.MosfetType * load.Von;
            vdsat = mbp.MosfetType * load.SaturationVoltageDS;

            Vgs.Current = vgs;
            Vbs.Current = vbs;
            Vds.Current = vds;

            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            /* 
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            * 
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /* 
            * charge storage elements
            * 
            * .. bulk - drain and bulk - source depletion capacitances
            */
            if (vbs < temp.TempDepletionCap)
            {
                double arg = 1 - vbs / temp.TempBulkPotential, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == mbp.BulkJunctionSideGradingCoefficient)
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Qbs.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                Qbs.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));
                CapBS = temp.F2S + temp.F3S * vbs;
            }

            if (vbd < temp.TempDepletionCap)
            {
                double arg = 1 - vbd / temp.TempBulkPotential, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == .5 && mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Qbd.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBD = temp.CapBD * sarg + temp.CapBDSidewall * sargsw;
            }
            else
            {
                Qbd.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);
                CapBD = temp.F2D + vbd * temp.F3D;
            }
            /* CAPZEROBYPASS */

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non - constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
             */
            double icapgs, icapgd, icapgb;
            if (load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.TempPhi, OxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.TempPhi, OxideCap);
            }
            Capgs.Current = icapgs;
            Capgd.Current = icapgd;
            Capgb.Current = icapgb;

            /* TRANOP only */
            Qgs.Current = vgs * capgs;
            Qgd.Current = vgd * capgd;
            Qgb.Current = vgb * capgb;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var rstate = state;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd;

            vbs = load.VoltageBS;
            vbd = load.VoltageBD;
            vgs = load.VoltageGS;
            vds = load.VoltageDS;
            vgd = load.VoltageGS - load.VoltageDS;
            vgb = load.VoltageGS - load.VoltageBS;
            von = mbp.MosfetType * load.Von;
            vdsat = mbp.MosfetType * load.SaturationVoltageDS;

            Vds.Current = vds;
            Vbs.Current = vbs;
            Vgs.Current = vgs;

            double Gbd = 0.0;
            double Cbd = 0.0;
            double Cd = 0.0;
            double Gbs = 0.0;
            double Cbs = 0.0;

            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            /* 
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            * 
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /* 
            * charge storage elements
            * 
            * .. bulk - drain and bulk - source depletion capacitances
            */

            if (vbs < temp.TempDepletionCap)
            {
                double arg = 1 - vbs / temp.TempBulkPotential, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == mbp.BulkJunctionSideGradingCoefficient)
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Qbs.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                Qbs.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));
                CapBS = temp.F2S + temp.F3S * vbs;
            }

            if (vbd < temp.TempDepletionCap)
            {
                double arg = 1 - vbd / temp.TempBulkPotential, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == .5 && mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Qbd.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBD = temp.CapBD * sarg + temp.CapBDSidewall * sargsw;
            }
            else
            {
                Qbd.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);
                CapBD = temp.F2D + vbd * temp.F3D;
            }
            /* CAPZEROBYPASS */

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */

            /* 
            * calculate equivalent conductances and currents for
            * depletion capacitors
            */

            /* integrate the capacitors and save results */
            Qbd.Integrate();
            Gbd += Qbd.Jacobian(CapBD);
            Cbd += Qbd.Derivative;
            Cd -= Qbd.Derivative;
            Qbs.Integrate();
            Gbs += Qbs.Jacobian(CapBS);
            Cbs += Qbs.Derivative;

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non - constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
             */
            double icapgs, icapgd, icapgb;
            if (load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.TempPhi, OxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.TempPhi, OxideCap);
            }
            Capgs.Current = icapgs;
            Capgd.Current = icapgd;
            Capgb.Current = icapgb;

            vgs1 = Vgs[1];
            vgd1 = vgs1 - Vds[1];
            vgb1 = vgs1 - Vbs[1];
            capgs = (Capgs.Current + Capgs[1] + GateSourceOverlapCap);
            capgd = (Capgd.Current + Capgd[1] + GateDrainOverlapCap);
            capgb = (Capgb.Current + Capgb[1] + GateBulkOverlapCap);

            Qgs.Current = (vgs - vgs1) * capgs + Qgs[1];
            Qgd.Current = (vgd - vgd1) * capgd + Qgd[1];
            Qgb.Current = (vgb - vgb1) * capgb + Qgb[1];

            /* 
             * calculate equivalent conductances and currents for
             * meyer"s capacitors
             */
            Qgs.Integrate();
            gcgs = Qgs.Jacobian(capgs);
            ceqgs = Qgs.RhsCurrent(gcgs, vgs);
            Qgd.Integrate();
            gcgd = Qgd.Jacobian(capgd);
            ceqgd = Qgd.RhsCurrent(gcgd, vgd);
            Qgb.Integrate();
            gcgb = Qgb.Jacobian(capgb);
            ceqgb = Qgb.RhsCurrent(gcgb, vgb);

            /* 
             * load current vector
             */
            ceqbs = mbp.MosfetType * (Cbs - Gbs * vbs);
            ceqbd = mbp.MosfetType * (Cbd - Gbd * vbd);
            rstate.Rhs[gateNode] -= (mbp.MosfetType * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[bulkNode] -= (ceqbs + ceqbd - mbp.MosfetType * ceqgb);
            rstate.Rhs[drainNodePrime] += (ceqbd + mbp.MosfetType * ceqgd);
            rstate.Rhs[sourceNodePrime] += ceqbs + mbp.MosfetType * ceqgs;

            /* 
			 * load y matrix
			 */
            GateGatePtr.Add(gcgd + gcgs + gcgb);
            BulkBulkPtr.Add(Gbd + Gbs + gcgb);
            DrainPrimeDrainPrimePtr.Add(Gbd + gcgd);
            SourcePrimeSourcePrimePtr.Add(Gbs + gcgs);
            GateBulkPtr.Sub(gcgb);
            GateDrainPrimePtr.Sub(gcgd);
            GateSourcePrimePtr.Sub(gcgs);
            BulkGatePtr.Sub(gcgb);
            BulkDrainPrimePtr.Sub(Gbd);
            BulkSourcePrimePtr.Sub(Gbs);
            DrainPrimeGatePtr.Add(-gcgd);
            DrainPrimeBulkPtr.Add(-Gbd);
            SourcePrimeGatePtr.Add(-gcgs);
            SourcePrimeBulkPtr.Add(-Gbs);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            Qgs.LocalTruncationError(ref timestep);
            Qgd.LocalTruncationError(ref timestep);
            Qgb.LocalTruncationError(ref timestep);
        }
    }
}