//-----------------------------------------------------------------------
// <copyright file="Connection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Google.ProtocolBuffers;
    using Messages;

    /// <summary>
    /// Class representing a network connection to a Mumble server
    /// </summary>
    internal class Connection : IConnection
    {
        /// <summary>
        /// Hostname of the server
        /// </summary>
        private readonly string host;

        /// <summary>
        /// Port on which the server is listening
        /// </summary>
        private readonly int port;

        /// <summary>
        /// Protobuf message factory
        /// </summary>
        private readonly IMessageFactory messageFactory;

        /// <summary>
        /// Semaphore used to lock the write method
        /// </summary>
        private readonly SemaphoreSlim writeSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// TCP client which connects to the server
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// SSL stream which encrypts the server connection
        /// </summary>
        private SslStream sslStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="host">Hostname of the server</param>
        /// <param name="port">Port on which the server is listening</param>
        public Connection(string host, int port)
            : this(host, port, new MessageFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="host">Hostname of the server</param>
        /// <param name="port">Port on which the server is listening</param>
        /// <param name="messageFactory">Protobuf message factory</param>
        public Connection(string host, int port, IMessageFactory messageFactory)
        {
            this.host = host;
            this.port = port;
            this.messageFactory = messageFactory;
            this.Connected = false;
        }

        /// <summary>
        /// Gets a value indicating whether the connection is established
        /// </summary>
        public bool Connected { get; private set; }

        /// <inheritdoc />
        public async Task ConnectAsync()
        {
            if (!this.Connected)
            {
                this.tcpClient = new TcpClient(this.host, this.port);
                this.sslStream = new SslStream(this.tcpClient.GetStream(), false, (sender, cert, chain, errors) => true);
                await this.sslStream.AuthenticateAsClientAsync(this.host);
                this.Connected = true;
            }
        }

        /// <inheritdoc />
        public async Task<IMessage> ReadMessage()
        {
            var headerBytes = new byte[6];
            await this.sslStream.ReadAsync(headerBytes, 0, headerBytes.Length);

            var type = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(headerBytes, 0));
            var size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBytes, 2));

            var messageBytes = new byte[size];
            await this.sslStream.ReadAsync(messageBytes, 0, size);

            if (type == (int)MessageType.UDPTunnel)
            {
                return new UDPTunnel.Builder
                {
                    Packet = ByteString.CopyFrom(messageBytes)
                }.Build();
            }
            else
            {
                return this.messageFactory.Deserialize((MessageType)type, ByteString.CopyFrom(messageBytes));
            }
        }

        /// <inheritdoc />
        public async Task SendUDPPacket(byte[] packet)
        {
            await this.WriteMessage(MessageType.UDPTunnel, packet);
        }

        /// <inheritdoc />
        public async Task SendMessage(IMessage message)
        {
            await this.WriteMessage(this.messageFactory.GetMessageType(message), message.ToByteArray());
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.sslStream.Dispose();
            this.writeSemaphore.Dispose();
        }

        /// <summary>
        /// Write a message to the SSL stream
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <param name="messageBytes">Byte array of serialized message</param>
        /// <returns>Empty task</returns>
        private async Task WriteMessage(MessageType type, byte[] messageBytes)
        {
            var typeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)type));
            var sizeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageBytes.Length));

            await this.writeSemaphore.WaitAsync();
            await this.sslStream.WriteAsync(typeBytes, 0, typeBytes.Length);
            await this.sslStream.WriteAsync(sizeBytes, 0, sizeBytes.Length); 
            await this.sslStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            this.writeSemaphore.Release();
        }
    }
}
