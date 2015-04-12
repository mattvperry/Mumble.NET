//-----------------------------------------------------------------------
// <copyright file="IConnection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Interface representing a network connection to a Mumble server
    /// </summary>
    internal interface IConnection : IDisposable
    {
        /// <summary>
        /// Establish a SSL encrypted TCP connection to the server
        /// </summary>
        /// <returns>Empty task</returns>
        Task ConnectAsync();

        /// <summary>
        /// Read an incoming protobuf message from the socket
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <param name="timeout">Time in seconds to wait for message</param>
        /// <returns>Protobuf message from the socket</returns>
        Task<IMessage> ReadMessageAsync(CancellationToken cancellationToken, int timeout = Timeout.Infinite);

        /// <summary>
        /// Send a UDP packet message to the server. Used for audio data
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>Empty task</returns>
        Task SendUDPPacketAsync(byte[] packet, CancellationToken cancellationToken);

        /// <summary>
        /// Send a protobuf message to the server
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>Empty task</returns>
        Task SendMessageAsync(IMessage message, CancellationToken cancellationToken);
    }
}
