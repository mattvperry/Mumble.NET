//-----------------------------------------------------------------------
// <copyright file="MessageType.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Messages
{
    /// <summary>
    /// Enumeration of all Mumble protobuf message types
    /// </summary>
    internal enum MessageType
    {
        /// <summary>
        /// Version message type
        /// </summary>
        Version = 0,

        /// <summary>
        /// UDP Tunnel message type 
        /// </summary>
        UDPTunnel = 1,

        /// <summary>
        /// Authenticate message type
        /// </summary>
        Authenticate = 2,

        /// <summary>
        /// Ping message type
        /// </summary>
        Ping = 3,

        /// <summary>
        /// Reject message type
        /// </summary>
        Reject = 4,

        /// <summary>
        /// Server Sync message type
        /// </summary>
        ServerSync = 5,

        /// <summary>
        /// Channel Remove message type
        /// </summary>
        ChannelRemove = 6,

        /// <summary>
        /// Channel State message type
        /// </summary>
        ChannelState = 7,

        /// <summary>
        /// User Remove message type
        /// </summary>
        UserRemove = 8,

        /// <summary>
        /// User State message type
        /// </summary>
        UserState = 9,

        /// <summary>
        /// Ban List message type
        /// </summary>
        BanList = 10,

        /// <summary>
        /// Text Message message type
        /// </summary>
        TextMessage = 11,

        /// <summary>
        /// Permission Denied message type
        /// </summary>
        PermissionDenied = 12,

        /// <summary>
        /// ACL message type
        /// </summary>
        ACL = 13,

        /// <summary>
        /// Query Users message type
        /// </summary>
        QueryUsers = 14,
        
        /// <summary>
        /// Crypt Setup message type
        /// </summary>
        CryptSetup = 15,

        /// <summary>
        /// Context Action Modify message type
        /// </summary>
        ContextActionModify = 16,

        /// <summary>
        /// Context Action message type
        /// </summary>
        ContextAction = 17,

        /// <summary>
        /// User List message type
        /// </summary>
        UserList = 18,

        /// <summary>
        /// Voice Target message type
        /// </summary>
        VoiceTarget = 19,

        /// <summary>
        /// Permission Query message type
        /// </summary>
        PermissionQuery = 20,

        /// <summary>
        /// Codec Version message type
        /// </summary>
        CodecVersion = 21,

        /// <summary>
        /// User Stats message type
        /// </summary>
        UserStats = 22,

        /// <summary>
        /// Request Blob message type
        /// </summary>
        RequestBlob = 23,

        /// <summary>
        /// Server Config message type
        /// </summary>
        ServerConfig = 24,

        /// <summary>
        /// Suggest Config message type
        /// </summary>
        SuggestConfig = 25
    }
}
