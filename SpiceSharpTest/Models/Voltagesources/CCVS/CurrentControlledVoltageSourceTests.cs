﻿using System;
using System.Numerics;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    [TestFixture]
    public class CurrentControlledVoltageSourceTests : Framework
    {
        [Test]
        public void When_CCVSDC_Expect_Reference()
        {
            double transimpedance = 12.0;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );

            // Build simulation, exports and references
            DC dc = new DC("DC", "I1", -10, 10, 1e-3);
            Export<double>[] exports = { new RealVoltageExport(dc, "out") };
            Func<double, double>[] references = { sweep => transimpedance * sweep };
            AnalyzeDC(dc, ckt, exports, references);
        }

        [Test]
        public void When_CCVSSmallSignal_Expect_Reference()
        {
            double magnitude = 0.9;
            double transimpedance = 12.0;

            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new CurrentSource("I1", "0", "in", 0.0),
                new VoltageSource("V1", "in", "0", 0.0),
                new CurrentControlledVoltageSource("F1", "out", "0", "V1", transimpedance)
                );
            ckt.Objects["I1"].SetParameter("acmag", magnitude);

            // Build simulation, exports and references
            AC ac = new AC("AC", new DecadeSweep(1.0, 10e3, 4));
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };
            Func<double, Complex>[] references = { sweep => transimpedance * magnitude };
            AnalyzeAC(ac, ckt, exports, references);
        }
    }
}
