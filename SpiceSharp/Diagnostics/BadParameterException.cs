﻿using System;
using System.Runtime.Serialization;

namespace SpiceSharp
{
    /// <summary>
    /// Exception for a bad parameter.
    /// </summary>
    [Serializable]
    public class BadParameterException : CircuitException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BadParameterException() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        public BadParameterException(string parameterName)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="innerException">Inner exception</param>
        public BadParameterException(string parameterName, Exception innerException)
            : base("Invalid parameter value for '{0}'".FormatString(parameterName), innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected BadParameterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
