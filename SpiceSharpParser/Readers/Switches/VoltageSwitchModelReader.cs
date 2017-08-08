﻿using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switch models
    /// </summary>
    public class VoltageSwitchModelReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            VoltageSwitchModel model = new VoltageSwitchModel(name.ReadIdentifier());
            model.ReadParameters(parameters);
            netlist.Circuit.Components.Add(model);
            Generated = model;
            return true;
        }
    }
}
