//-----------------------------------------------------------------------
// <copyright file="UnexpectedMessageException.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Exceptions
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception class thrown when an unexpected message type is read from the server connection
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "No need to serialize")]
    public class UnexpectedMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedMessageException"/> class.
        /// </summary>
        /// <param name="expectedType">Expected type</param>
        /// <param name="actualType">Actual type</param>
        public UnexpectedMessageException(string expectedType, string actualType)
            : base(string.Format(CultureInfo.InvariantCulture, "Expected message of type, {0}, was type {1}", expectedType, actualType))
        { 
        }
    }
}
