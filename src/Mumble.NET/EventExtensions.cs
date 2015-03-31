//-----------------------------------------------------------------------
// <copyright file="EventExtensions.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Extension methods to assist with raising events
    /// </summary>
    internal static class EventExtensions
    {
        /// <summary>
        /// Raise the given event in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="handler">Handler to raise</param>
        /// <param name="sender">Sender of event</param>
        /// <param name="argument">Received message</param>
        public static void RaiseEvent<T>(
            this EventHandler<MessageReceivedEventArgs<T>> handler,
            object sender,
            T argument)
            where T : IMessage
        {
            var e = new MessageReceivedEventArgs<T>(argument);
            var tempHandler = handler;
            if (tempHandler != null)
            {
                tempHandler(sender, e);
            }
        }
    }
}
