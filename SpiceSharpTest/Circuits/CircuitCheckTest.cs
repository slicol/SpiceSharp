﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest.Circuits
{
    [TestClass]
    public class CircuitCheckTest
    {
        [TestMethod]
        [ExpectedException(typeof(CircuitException))]
        public void BadGroundNodeName()
        {
            // Verifies that CircuitException is thrown during Check when circuit has a ground node called "GND"
            var ckt = CreateCircuit("gnd");
            ckt.Validate();
        }

        [TestMethod]
        public void CorrectGroundNodeName()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "gnd"
            var ckt = CreateCircuit("GND");
            ckt.Validate();
        }

        [TestMethod]
        public void CorrectSecondGroundNodeName()
        {
            // Verifies that CircuitException is not thrown during Check when circuit has a ground node called "0"
            var ckt = CreateCircuit("0");
            ckt.Validate();
        }

        /// <summary>
        /// Creates a circuit with a resistor and a voltage source which is connected to IN
        /// node and a ground node with name <paramref name="groundNodeName"/>
        /// </summary>
        /// <param name="groundNodeName"></param>
        /// <returns></returns>
        private static Circuit CreateCircuit(string groundNodeName)
        {
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "IN", groundNodeName, 1.0),
                new Resistor("R1", "IN", groundNodeName, 1.0e3));
            return ckt;
        }
    }
}