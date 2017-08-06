﻿using SpiceSharp.Components;
using SpiceSharp.Components.Waveforms;

namespace SpiceSharp.Parser.Readers.Waveforms
{
    /// <summary>
    /// A class that can read a sine wave
    /// </summary>
    public class SineReader : WaveformReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SineReader()
            : base("sin", new string[] { "vo", "va", "freq", "td", "theta" })
        {
        }

        /// <summary>
        /// Generate a new sine waveform
        /// </summary>
        /// <returns></returns>
        protected override Waveform Generate() => new Sine();
    }
}