﻿using System;
using SpiceSharp.Diagnostics;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Frequency-domain analysis (AC analysis)
    /// </summary>
    public class AC : FrequencySimulation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        public AC(string name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="frequencySweep">Frequency sweep</param>
        public AC(Identifier name, Sweep<double> frequencySweep) : base(name, frequencySweep)
        {
        }

        /// <summary>
        /// Execute
        /// </summary>
        protected override void Execute()
        {
            // Execute base behavior
            base.Execute();

            var circuit = Circuit;

            var state = RealState;
            var cstate = ComplexState;
            var baseconfig = BaseConfiguration;
            var freqconfig = FrequencyConfiguration;
            
            // Calculate the operating point
            cstate.Laplace = 0.0;
            state.Domain = RealState.DomainType.Frequency;
            state.Gmin = baseconfig.Gmin;
            Op(baseconfig.DCMaxIterations);

            // Load all in order to calculate the AC info for all devices
            foreach (var behavior in LoadBehaviors)
                behavior.Load(this);
            foreach (var behavior in FrequencyBehaviors)
                behavior.InitializeParameters(this);

            // Export operating point if requested
            var exportargs = new ExportDataEventArgs(RealState, ComplexState);
            if (freqconfig.KeepOpInfo)
                Export(exportargs);

            // Sweep the frequency
            foreach (double freq in FrequencySweep.Points)
            {
                // Calculate the current frequency
                cstate.Laplace = new Complex(0.0, 2.0 * Math.PI * freq);

                // Solve
                ACIterate(circuit);

                // Export the timepoint
                Export(exportargs);
            }
        }
    }
}