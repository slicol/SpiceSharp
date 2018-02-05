﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet1"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("off"), PropertyInfo("Device initially off")]
        public bool Off { get; set; }
        [PropertyName("icvbs"), PropertyInfo("Initial B-S voltage")]
        public Parameter InitialVoltageBS { get; } = new Parameter();
        [PropertyName("icvds"), PropertyInfo("Initial D-S voltage")]
        public Parameter InitialVoltageDS { get; } = new Parameter();
        [PropertyName("icvgs"), PropertyInfo("Initial G-S voltage")]
        public Parameter InitialVoltageGS { get; } = new Parameter();
        [PropertyName("temp"), PropertyInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter Temperature { get; } = new Parameter();
        [PropertyName("w"), PropertyInfo("Width")]
        public Parameter Width { get; } = new Parameter(1e-4);
        [PropertyName("l"), PropertyInfo("Length")]
        public Parameter Length { get; } = new Parameter(1e-4);
        [PropertyName("as"), PropertyInfo("Source area")]
        public Parameter SourceArea { get; } = new Parameter();
        [PropertyName("ad"), PropertyInfo("Drain area")]
        public Parameter DrainArea { get; } = new Parameter();
        [PropertyName("ps"), PropertyInfo("Source perimeter")]
        public Parameter SourcePerimeter { get; } = new Parameter();
        [PropertyName("pd"), PropertyInfo("Drain perimeter")]
        public Parameter DrainPerimeter { get; } = new Parameter();
        [PropertyName("nrs"), PropertyInfo("Source squares")]
        public Parameter SourceSquares { get; } = new Parameter(1);
        [PropertyName("nrd"), PropertyInfo("Drain squares")]
        public Parameter DrainSquares { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("ic"), PropertyInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (value.Length)
            {
                case 3: InitialVoltageBS.Set(value[2]); goto case 2;
                case 2: InitialVoltageGS.Set(value[1]); goto case 1;
                case 1: InitialVoltageDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}