//-----------------------------------------------------------------------
// <copyright file="MessageReceivedEventArgs.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Class containing event data about a received Mumble Message
    /// </summary>
    /// <typeparam name="T">Type of message</typeparam>
    internal class MessageReceivedEventArgs<T> : EventArgs where T : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedEventArgs{T}"/> class.
        /// </summary>
        /// <param name="message">Received message</param>
        public MessageReceivedEventArgs(T message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets received message
        /// </summary>
        public T Message { get; private set; }
    }
}
