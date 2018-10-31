using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary></summary>
    [PublicAPI]
    [Serializable]
    public sealed class SimulationFailedException : Exception
    {
        /// <summary>Constructor</summary>
        public SimulationFailedException()
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="message">String setting the message of the exception.</param>
        public SimulationFailedException(string message)
            : base(message)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="message">String setting the message of the exception.</param>
        /// <param name="inner">Sets a reference to the InnerException.</param>
        public SimulationFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming data context.</param>
        private SimulationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     This member supports the .NET Framework infrastructure and is not intended to
        ///     be used directly from your code.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming data context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
