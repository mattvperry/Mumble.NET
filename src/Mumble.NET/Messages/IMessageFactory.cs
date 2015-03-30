//-----------------------------------------------------------------------
// <copyright file="IMessageFactory.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Messages
{
    using System;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Interface for the protobuf message factory
    /// </summary>
    internal interface IMessageFactory
    {
        /// <summary>
        /// Create a weak builder given the protobuf message type
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <returns>Weak message builder</returns>
        IBuilder CreateBuilder(MessageType type);

        /// <summary>
        /// Deserialize a byte string into a message of the given type
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <param name="byteString">Serialized message byte string</param>
        /// <returns>Deserialized message of given type</returns>
        IMessage Deserialize(MessageType type, ByteString byteString);

        /// <summary>
        /// Get the message type of a given message
        /// </summary>
        /// <param name="message">Message to get type of</param>
        /// <returns>Message type</returns>
        MessageType GetMessageType(IMessage message);
    }
}
