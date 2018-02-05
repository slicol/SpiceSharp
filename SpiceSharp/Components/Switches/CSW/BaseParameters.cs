﻿using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSwitchBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("on"), PropertyInfo("Initially closed")]
        public void SetZeroStateOn() { ZeroState = true; }
        [PropertyName("off"), PropertyInfo("Initially open")]
        public void SetZeroStateOff() { ZeroState = false; }

        /// <summary>
        /// The initial state
        /// </summary>
        public bool ZeroState { get; set; } = false;
    }
}