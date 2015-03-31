//-----------------------------------------------------------------------
// <copyright file="IConnection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
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
        /// <returns>Protobuf message from the socket</returns>
        Task<IMessage> ReadMessage();

        /// <summary>
        /// Send a UDP packet message to the server. Used for audio data
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <returns>Empty task</returns>
        Task SendUDPPacket(byte[] packet);

        /// <summary>
        /// Send a protobuf message to the server
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Empty task</returns>
        Task SendMessage(IMessage message);
    }
}
