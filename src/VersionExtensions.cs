//-----------------------------------------------------------------------
// <copyright file="VersionExtensions.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;

    /// <summary>
    /// Extension methods for encoding and decoding version objects into Mumble format
    /// </summary>
    internal static class VersionExtensions
    {
        /// <summary>
        /// Encode a version into mumble wire protocol version
        /// </summary>
        /// <param name="version">Version to encode</param>
        /// <returns>Unsigned integer representing the version</returns>
        public static uint EncodeVersion(this Version version)
        {
            return (uint)((version.Major << 16) | (version.Minor << 8) | (version.Build & 0xFF));
        }

        /// <summary>
        /// Decode a version number
        /// </summary>
        /// <param name="version">Encoding version</param>
        /// <returns>Decoded version object</returns>
        public static Version DecodeVersion(this uint version)
        {
            return new Version(
                (int)(version >> 16) & 0xFF,
                (int)(version >> 8) & 0xFF,
                (int)version & 0xFF);
        }
    }
}
