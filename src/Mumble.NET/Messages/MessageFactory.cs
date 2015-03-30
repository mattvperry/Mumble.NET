//-----------------------------------------------------------------------
// <copyright file="MessageFactory.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Factory class to create protobuf messages from their type enumeration
    /// </summary>
    internal class MessageFactory : IMessageFactory
    {
        /// <inheritdoc />
        public IBuilder CreateBuilder(MessageType type)
        {
            return MessageUtil.GetDefaultMessage(Messages.Types[type]).WeakCreateBuilderForType();
        }

        /// <inheritdoc />
        public IMessage Deserialize(MessageType type, ByteString byteString)
        {
            return this.CreateBuilder(type).WeakMergeFrom(byteString).WeakBuild();
        }

        /// <inheritdoc />
        public MessageType GetMessageType(IMessage message)
        {
            return Messages.Types.Where(kv => kv.Value == message.GetType()).First().Key;
        }
    }
}
