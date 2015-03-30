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
        /// <summary>
        /// Mapping of message types to concrete types
        /// </summary>
        private static Dictionary<MessageType, Type> types = new Dictionary<MessageType, Type>
        {
            { MessageType.Version, typeof(Version) },
            { MessageType.UDPTunnel, typeof(UDPTunnel) },
            { MessageType.Authenticate, typeof(Authenticate) },
            { MessageType.Ping, typeof(Ping) },
            { MessageType.Reject, typeof(Reject) },
            { MessageType.ServerSync, typeof(ServerSync) },
            { MessageType.ChannelRemove, typeof(ChannelRemove) },
            { MessageType.ChannelState, typeof(ChannelState) },
            { MessageType.UserRemove, typeof(UserRemove) },
            { MessageType.UserState, typeof(UserState) },
            { MessageType.BanList, typeof(BanList) },
            { MessageType.TextMessage, typeof(TextMessage) },
            { MessageType.PermissionDenied, typeof(PermissionDenied) },
            { MessageType.ACL, typeof(ACL) },
            { MessageType.QueryUsers, typeof(QueryUsers) },
            { MessageType.CryptSetup, typeof(CryptSetup) },
            { MessageType.ContextActionModify, typeof(ContextActionModify) },
            { MessageType.ContextAction, typeof(ContextAction) },
            { MessageType.UserList, typeof(UserList) },
            { MessageType.VoiceTarget, typeof(VoiceTarget) },
            { MessageType.PermissionQuery, typeof(PermissionQuery) },
            { MessageType.CodecVersion, typeof(CodecVersion) },
            { MessageType.UserStats, typeof(UserStats) },
            { MessageType.RequestBlob, typeof(RequestBlob) },
            { MessageType.ServerConfig, typeof(ServerConfig) },
            { MessageType.SuggestConfig, typeof(SuggestConfig) },
        };

        /// <inheritdoc />
        public IBuilder CreateBuilder(MessageType type)
        {
            return MessageUtil.GetDefaultMessage(types[type]).WeakCreateBuilderForType();
        }

        /// <inheritdoc />
        public IMessage Deserialize(MessageType type, ByteString byteString)
        {
            return this.CreateBuilder(type).WeakMergeFrom(byteString).WeakBuild();
        }

        /// <inheritdoc />
        public MessageType GetMessageType(IMessage message)
        {
            return types.Where(kv => kv.Value == message.GetType()).First().Key;
        }
    }
}
