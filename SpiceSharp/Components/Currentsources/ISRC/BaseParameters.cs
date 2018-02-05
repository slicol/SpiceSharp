﻿using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("waveform"), PropertyInfo("The waveform object for this source")]
        public Waveform Waveform { get; set; } = null;
        [PropertyName("dc"), PropertyInfo("D.C. source value")]
        public Parameter DCValue { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">DC value</param>
        public BaseParameters(double dc)
        {
            DCValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="waveform">Waveform</param>
        public BaseParameters(Waveform waveform)
        {
            Waveform = waveform;
        }
    }
}