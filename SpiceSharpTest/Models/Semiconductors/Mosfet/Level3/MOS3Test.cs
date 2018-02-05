﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    /// <summary>
    /// Model part of the FDC604P (ONSemi)
    /// M1 2 1 4x 4x DMOS L = 1u W = 1u
    /// .MODEL DMOS PMOS(VTO= -0.7 KP= 3.8E+1 THETA = .25 VMAX= 3.5E5 LEVEL= 3)
    /// </summary>
    [TestClass]
    public class MOS3Test : Framework
    {
        /// <summary>
        /// Create a MOS3 transistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="d">Drain</param>
        /// <param name="g">Gate</param>
        /// <param name="s">Source</param>
        /// <param name="b">Bulk</param>
        /// <param name="modelname">Model name</param>
        /// <param name="nmos">True for NMOS, false for PMOS</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        protected Mosfet3 CreateMOS3(Identifier name, Identifier d, Identifier g, Identifier s, Identifier b,
            Identifier modelname, bool nmos, string modelparams)
        {
            // Create model
            Mosfet3Model model = new Mosfet3Model(modelname, nmos);
            ApplyParameters(model, modelparams);

            // Create transistor
            Mosfet3 mos = new Mosfet3(name);
            mos.Connect(d, g, s, b);
            mos.SetModel(model);
            return mos;
        }

        [TestMethod]
        public void MOS3_DC()
        {
            /*
             * MOS3 driven by voltage sources
             * The current should match the references. References are simulation results by Spice 3f5.
             */
            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "0", "g", 0),
                new VoltageSource("V2", "0", "d", 0),
                CreateMOS3("M1", "d", "g", "0", "0",
                    "DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5")
                );
            ckt.Objects["M1"].ParameterSets.SetProperty("w", 1e-6);
            ckt.Objects["M1"].ParameterSets.SetProperty("l", 1e-6);

            // Create simulation
            DC dc = new DC("dc", new SweepConfiguration[] {
                new SweepConfiguration("V2", 0, 1.8, 0.3),
                new SweepConfiguration("V1", 0, 1.8, 0.3)
            });

            // Create exports
            Export<double>[] exports = { new RealPropertyExport(dc, "V2", "i") };

            // Create references
            double[][] references = new double[1][];
            references[0] = new double[] { -1.262177448353619e-29, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, -4.159905034473416e-13, -4.159905034473416e-13, -4.159905034473416e-13, -7.010973787728751e-01, -3.391621129326464e+00, -5.921232876712748e+00, -8.164781906300902e+00, -8.319810068946831e-13, -8.319810068946831e-13, -8.319810068946831e-13, -7.010973787732908e-01, -3.928205688972319e+00, -8.750000000000831e+00, -1.323794712286243e+01, -1.247971510342025e-12, -1.247971510342025e-12, -1.247971510342025e-12, -7.010973787737074e-01, -3.928205688972735e+00, -9.117778872755419e+00, -1.555322338830710e+01, -1.663962013789366e-12, -1.663962013789366e-12, -1.663962013789366e-12, -7.010973787741221e-01, -3.928205688973152e+00, -9.117778872755839e+00, -1.577264391107695e+01, -2.079952517236708e-12, -2.079952517236708e-12, -2.079952517236708e-12, -7.010973787745387e-01, -3.928205688973566e+00, -9.117778872756254e+00, -1.577264391107736e+01, -2.495943020684050e-12, -2.495943020684050e-12, -2.495943020684050e-12, -7.010973787749553e-01, -3.928205688973984e+00, -9.117778872756670e+00, -1.577264391107778e+01 };

            // Run test
            AnalyzeDC(dc, ckt, exports, references);
        }

        [TestMethod]
        public void MOS3_AC()
        {
            /*
             * Common-source amplifier biased as a diode-connected transistor
             * Output voltage is expected to match the reference. Reference is simulated by Spice 3f5.
             */
            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("Vsupply", "vdd", "0", 1.8),
                new VoltageSource("Vin", "in", "0", 0.0),
                new Resistor("R1", "out", "0", 100e3),
                new Resistor("R2", "g", "out", 10e3),
                new Capacitor("C1", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "g", "vdd", "vdd",
                    "DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5")
                );
            ckt.Objects["Vin"].ParameterSets.SetProperty("acmag", 1.0);
            ckt.Objects["M1"].ParameterSets.SetProperty("w", 1e-6);
            ckt.Objects["M1"].ParameterSets.SetProperty("l", 1e-6);

            // Create simulation
            AC ac = new AC("ac", new SpiceSharp.Simulations.Sweeps.DecadeSweep(10, 10e9, 5));

            // Create exports
            Export<Complex>[] exports = { new ComplexVoltageExport(ac, "out") };

            // Create references
            double[] riref = { -1.448857719884684e-03, -6.260007108126745e-01, -3.639336573643311e-03, -9.921362299670315e-01, -9.141414195176988e-03, -1.572397969565666e+00, -2.296102101289896e-02, -2.491955503154010e+00, -5.766807563860867e-02, -3.948976475444068e+00, -1.448089774772143e-01, -6.256689086075871e+00, -3.634495125691141e-01, -9.908163806423953e+00, -9.110929278146982e-01, -1.567154314632532e+01, -2.276965858631552e+00, -2.471186973228724e+01, -5.647598782046568e+00, -3.867345058089153e+01, -1.375199410745549e+01, -5.941755334708798e+01, -3.207763017324329e+01, -8.744829839823841e+01, -6.832438160737344e+01, -1.175235216537343e+02, -1.241920646766530e+02, -1.347854256329389e+02, -1.841315808238452e+02, -1.260890489248231e+02, -2.279253114275959e+02, -9.847854969308602e+01, -2.517636926413673e+02, -6.863445460602944e+01, -2.627019332545188e+02, -4.518688001997734e+01, -2.673256909888647e+02, -2.901280942253893e+01, -2.692120585123498e+02, -1.843501927536817e+01, -2.699704646985724e+02, -1.166447888431620e+01, -2.702735821604205e+02, -7.368052045702465e+00, -2.703944449090613e+02, -4.651005490826149e+00, -2.704425913243499e+02, -2.935108605838326e+00, -2.704617635295234e+02, -1.852059618528605e+00, -2.704693968784006e+02, -1.168603599754638e+00, -2.704724358892379e+02, -7.373473088379656e-01, -2.704736457602495e+02, -4.652367810210625e-01, -2.704741274215870e+02, -2.935450866537507e-01, -2.704743191748967e+02, -1.852145596684913e-01, -2.704743955133399e+02, -1.168625197106691e-01, -2.704744259042335e+02, -7.373527339090884e-02, -2.704744380030681e+02, -4.652381437434735e-02, -2.704744428197012e+02, -2.935454289547597e-02, -2.704744447372374e+02, -1.852146456506789e-02, -2.704744455006224e+02, -1.168625413084242e-02, -2.704744458045314e+02, -7.373527881602024e-03, -2.704744459255197e+02, -4.652381573707377e-03, -2.704744459736860e+02, -2.935454323777736e-03, -2.704744459928614e+02, -1.852146465105010e-03, -2.704744460004952e+02, -1.168625415244017e-03, -2.704744460035343e+02, -7.373527887027131e-04, -2.704744460047443e+02, -4.652381575070101e-04, -2.704744460052259e+02, -2.935454324120037e-04, -2.704744460054176e+02, -1.852146465190992e-04, -2.704744460054940e+02, -1.168625415265614e-04 };
            Complex[][] references = new Complex[1][];
            references[0] = new Complex[riref.Length / 2];
            for (int i = 0; i < riref.Length; i += 2)
            {
                references[0][i / 2] = new Complex(riref[i], riref[i + 1]);
            }

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
        }

        [TestMethod]
        public void MOS3_Transient()
        {
            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1.8, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new VoltageSource("Vsupply", "vdd", "0", 1.8),
                new Resistor("R1", "out", "0", 100e3),
                CreateMOS3("M1", "out", "in", "vdd", "vdd",
                    "DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5")
                );
            ckt.Objects["M1"].ParameterSets.SetProperty("w", 1e-6);
            ckt.Objects["M1"].ParameterSets.SetProperty("l", 1e-6);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            Export<double>[] exports = { new GenericExport<double>(tran, () => tran.Method.Time), new RealVoltageExport(tran, "out") };

            // Create references
            double[][] references = new double[2][];
            references[0] = new double[] { 0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06, 1.001000000000000e-06, 1.001080000000000e-06, 1.001240000000000e-06, 1.001560000000000e-06, 1.002200000000000e-06, 1.003480000000000e-06, 1.006040000000000e-06, 1.011160000000001e-06, 1.021400000000002e-06, 1.041880000000003e-06, 1.082840000000006e-06, 1.164760000000012e-06, 1.328600000000025e-06, 1.528600000000025e-06, 1.728600000000025e-06, 1.928600000000025e-06, 2.128600000000025e-06, 2.328600000000025e-06, 2.528600000000024e-06, 2.728600000000024e-06, 2.928600000000024e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06, 3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 4.000999999999999e-06, 4.200999999999999e-06, 4.400999999999999e-06, 4.600999999999999e-06, 4.800999999999999e-06, 5.000999999999998e-06, 5.200999999999998e-06, 5.400999999999998e-06, 5.600999999999998e-06, 5.800999999999997e-06, 6.000999999999997e-06, 6.200999999999997e-06, 6.400999999999997e-06, 6.600999999999997e-06, 6.800999999999996e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06, 7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06, 7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06, 7.082839999999971e-06, 7.164759999999943e-06, 7.328599999999885e-06, 7.528599999999885e-06, 7.728599999999886e-06, 7.928599999999885e-06, 8.128599999999885e-06, 8.328599999999885e-06, 8.528599999999885e-06, 8.728599999999885e-06, 8.928599999999884e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06, 9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06, 9.800999999999996e-06, 9.999999999999999e-06 };
            references[1] = new double[] { 1.799999451299451e+00, 1.799999453635131e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999389971110e+00, 1.799999183568271e+00, 7.448095922434256e-02, -7.448046003580762e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447238e+00, 1.800000994575628e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999389971109e+00, 1.799999183568270e+00, 7.448095922456173e-02, -7.448046003602679e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447239e+00, 1.800000994575627e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00 };

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
        }

        /*
        [TestMethod]
        public void TestMOS3_Transient()
        {
            double[] reft = new double[]
            {
                0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06, 1.001000000000000e-06, 1.001080000000000e-06, 1.001240000000000e-06, 1.001560000000000e-06, 1.002200000000000e-06, 1.003480000000000e-06, 1.006040000000000e-06, 1.011160000000001e-06, 1.021400000000002e-06, 1.041880000000003e-06, 1.082840000000006e-06, 1.164760000000012e-06, 1.328600000000025e-06, 1.528600000000025e-06, 1.728600000000025e-06, 1.928600000000025e-06, 2.128600000000025e-06, 2.328600000000025e-06, 2.528600000000024e-06, 2.728600000000024e-06, 2.928600000000024e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06, 3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 4.000999999999999e-06, 4.200999999999999e-06, 4.400999999999999e-06, 4.600999999999999e-06, 4.800999999999999e-06, 5.000999999999998e-06, 5.200999999999998e-06, 5.400999999999998e-06, 5.600999999999998e-06, 5.800999999999997e-06, 6.000999999999997e-06, 6.200999999999997e-06, 6.400999999999997e-06, 6.600999999999997e-06, 6.800999999999996e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06, 7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06, 7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06, 7.082839999999971e-06, 7.164759999999943e-06, 7.328599999999885e-06, 7.528599999999885e-06, 7.728599999999886e-06, 7.928599999999885e-06, 8.128599999999885e-06, 8.328599999999885e-06, 8.528599999999885e-06, 8.728599999999885e-06, 8.928599999999884e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06, 9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06, 9.800999999999996e-06, 9.999999999999999e-06
            };
            double[] refv = new double[]
            {
                1.799999451299451e+00, 1.799999453635131e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999389971110e+00, 1.799999183568271e+00, 7.448095922434256e-02, -7.448046003580762e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447238e+00, 1.800000994575628e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999389971109e+00, 1.799999183568270e+00, 7.448095922456173e-02, -7.448046003602679e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447239e+00, 1.800000994575627e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00
            };

            var netlist = Run(
                "M1 out in vdd vdd DMOS L = 1u W = 1u",
                ".MODEL DMOS pmos(LEVEL = 3 VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5)",
                "V1 in 0 PULSE(0 1.8 1u 1n 0.5u 2u 6u)",
                "Vsupply vdd 0 1.8",
                "R1 out 0 100k",
                ".SAVE V(out)",
                ".tran 1n 10u"
            );
            TestTransient(netlist, reft, refv);
        }

        [TestMethod]
        public void TestMOS3_Noise()
        {
            // Provided by Spice 3f5
            double[] reference_in = new double[]
            {
                3.922768707208749e-08, 2.291519864765784e-08, 1.407017079182780e-08, 9.105419001244266e-09, 6.194775275256878e-09, 4.400494594546067e-09, 3.234724357194375e-09, 2.438766987662312e-09, 1.871693394323449e-09, 1.453917118933676e-09, 1.138437555727376e-09, 8.960486326477243e-10, 7.076247097689131e-10, 5.600153714457724e-10, 4.437981051977524e-10, 3.520011751241839e-10, 2.793437593104617e-10, 2.217599693356640e-10, 1.760847169296443e-10, 1.398362406154857e-10, 1.110594243473126e-10, 8.820939208512007e-11, 7.006307873970219e-11, 5.565101079844748e-11, 4.420413131174884e-11, 3.511206946242394e-11, 2.789024750161414e-11, 2.215388045908861e-11, 1.759738733044696e-11, 1.397806885343117e-11, 1.110315836866977e-11, 8.819544003760386e-12, 7.005608748958646e-12, 5.564750821227481e-12, 4.420237720013241e-12, 3.511119166439130e-12, 2.788980890105120e-12, 2.215366197887284e-12, 1.759727917185403e-12, 1.397801598669977e-12, 1.110313321355928e-12, 8.819532737389402e-13, 7.005604443464823e-13, 5.564750004447928e-13, 4.420238651739997e-13, 3.511120974499506e-13, 2.788983137375605e-13, 2.215368665286093e-13, 1.759730494910868e-13, 1.397804231690478e-13, 1.110315982090053e-13, 8.819559483630688e-14, 7.005631259322864e-14, 5.564776855198143e-14, 4.420265519978456e-14, 3.511147851503304e-14, 2.789010018772769e-14, 2.215395548885336e-14, 1.759757379613881e-14, 1.397831116946759e-14, 1.110342867623672e-14, 8.819828340357141e-15, 7.005900116746275e-15, 5.565045712970983e-15, 4.420534377926492e-15, 3.511416709539196e-15, 2.789278876852723e-15, 2.215664406987391e-15, 1.760026237727024e-15, 1.398099975065467e-15, 1.110611725745173e-15, 8.822516921586183e-16, 7.008588697982366e-16, 5.567734294210622e-16, 4.423222959167916e-16, 3.514105290781517e-16, 2.791967458095498e-16, 2.218352988230396e-16, 1.762714818970144e-16, 1.400788556308646e-16, 1.113300306988383e-16, 8.849402734018427e-17, 7.035474510414699e-17, 5.594620106642994e-17, 4.450108771600306e-17, 3.540991103213923e-17, 2.818853270527910e-17, 2.245238800662809e-17, 1.789600631402562e-17, 1.427674368741065e-17, 1.140186119420801e-17
            };
            double[] reference_out = new double[]
            {
                1.539536799052918e-08, 1.425345435411005e-08, 1.387057594440403e-08, 1.422632996835039e-08, 1.533963544458775e-08, 1.726972771446756e-08, 2.011928701284424e-08, 2.403985484211667e-08, 2.923980093621382e-08, 3.599521393602984e-08, 4.466419381666479e-08, 5.570510421760067e-08, 6.969935295791849e-08, 8.737911233612782e-08, 1.096598686737091e-07, 1.376764079306097e-07, 1.728180388884790e-07, 2.167531130268350e-07, 2.714217539395848e-07, 3.389553269773518e-07, 4.214470619168126e-07, 5.204498170488096e-07, 6.360328975134899e-07, 7.652633336600888e-07, 9.002602032091956e-07, 1.026717692562145e-06, 1.124827913760457e-06, 1.174519607691990e-06, 1.163957750016862e-06, 1.095752686376782e-06, 9.855214927538404e-07, 8.541143022481549e-07, 7.198049873512028e-07, 5.946327641875588e-07, 4.845636355140244e-07, 3.912952737214274e-07, 3.141085332390589e-07, 2.511836994017152e-07, 2.003727806630066e-07, 1.595909797180171e-07, 1.269836933484485e-07, 1.009753304509658e-07, 8.026208279931426e-08, 6.378180050949626e-08, 5.067741158629848e-08, 4.026138502566187e-08, 3.198421061708310e-08, 2.540769734184064e-08, 2.018292494198937e-08, 1.603230850568468e-08, 1.273514004532339e-08, 1.011599746207860e-08, 8.035484136167294e-09, 6.382846382763908e-09, 5.070095882912550e-09, 4.027334244157314e-09, 3.199035798462045e-09, 2.541093235211037e-09, 2.018470017819231e-09, 1.603335207809911e-09, 1.273581690332554e-09, 1.011649052308116e-09, 8.035885079041564e-10, 6.383201156746202e-10, 5.070427517480938e-10, 4.027654281492569e-10, 3.199350023391544e-10, 2.541404547028331e-10, 2.018779869617789e-10, 1.603644327863574e-10, 1.273890443643801e-10, 1.011957621812022e-10, 8.038969852857269e-11, 6.386285468853768e-11, 5.073511598184553e-11, 4.030738246218394e-11, 3.202433929990102e-11, 2.544488424493813e-11, 2.021863732481874e-11, 1.606728183409455e-11, 1.276974295521784e-11, 1.015041471851632e-11, 8.069808344039258e-12, 6.417123955417489e-12, 5.104350082433485e-12, 4.061576729307079e-12, 3.233272412497215e-12, 2.575326906709407e-12, 2.052702214551339e-12, 1.637566665405663e-12, 1.307812777481265e-12
            };

            var netlist = Run(
                "M1 out g vdd vdd DMOS L = 1u W = 1u",
                ".MODEL DMOS pmos(LEVEL = 3 VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24)",
                "Vsupply vdd 0 1.8",
                "V1 in 0 DC 0 AC 1 0",
                "R1 out 0 100k",
                "R2 g out 10k",
                "Cin in g 1u",
                ".NOISE v(out) V1 dec 10 10 10g");
            TestNoise(netlist, reference_in, reference_out);
        }
        */
    }
}