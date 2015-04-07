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
    using Google.ProtocolBuffers;
    using Messages;

    /// <summary>
    /// Class representing a network connection to a Mumble server
    /// </summary>
    internal class Connection : IConnection
    {
        /// <summary>
        /// Write timeout in seconds
        /// </summary>
        private const ushort WriteTimeout = 30;

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
                await this.sslStream.AuthenticateAsClientAsync(this.host).ConfigureAwait(false);
                this.Connected = true;
            }
        }

        /// <inheritdoc />
        public async Task<IMessage> ReadMessageAsync(CancellationToken cancellationToken, int timeout = Timeout.Infinite)
        {
            var headerBytes = new byte[6];
            await this.WithTimeout(
                async (token) =>
                {
                    await this.sslStream.ReadAsync(headerBytes, 0, headerBytes.Length, token).ConfigureAwait(false);
                },
                cancellationToken,
                TimeSpan.FromSeconds(timeout),
                "Read timeout");

            var type = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(headerBytes, 0));
            var size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBytes, 2));

            var messageBytes = new byte[size];
            await this.WithTimeout(
                async (token) =>
                {
                    await this.sslStream.ReadAsync(messageBytes, 0, size, token).ConfigureAwait(false);
                },
                cancellationToken,
                TimeSpan.FromSeconds(timeout),
                "Read timeout");

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
        public async Task SendUDPPacketAsync(byte[] packet, CancellationToken cancellationToken)
        {
            await this.WriteMessageAsync(MessageType.UDPTunnel, packet, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            await this.WriteMessageAsync(this.messageFactory.GetMessageType(message), message.ToByteArray(), cancellationToken).ConfigureAwait(false);
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
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>Empty task</returns>
        private async Task WriteMessageAsync(MessageType type, byte[] messageBytes, CancellationToken cancellationToken)
        {
            var typeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)type));
            var sizeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageBytes.Length));

            await this.writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            await this.WithTimeout(
                async (token) =>
                {
                    await this.sslStream.WriteAsync(typeBytes, 0, typeBytes.Length, token).ConfigureAwait(false);
                    await this.sslStream.WriteAsync(sizeBytes, 0, sizeBytes.Length, token).ConfigureAwait(false);
                    await this.sslStream.WriteAsync(messageBytes, 0, messageBytes.Length, token).ConfigureAwait(false);
                },
                cancellationToken,
                TimeSpan.FromSeconds(WriteTimeout),
                "Write timeout");

            this.writeSemaphore.Release();
        }

        /// <summary>
        /// Add a timeout to a cancellation token, call an async method 
        /// and throw a TimeoutException if the cancellation token
        /// is cancelled because it reached the timeout.
        /// </summary>
        /// <param name="action">Async method to call</param>
        /// <param name="cancellationToken">Base cancellation token to link with</param>
        /// <param name="timeout">Time to wait for async method</param>
        /// <param name="errorMessage">Error message for exception</param>
        /// <returns>Empty task</returns>
        private async Task WithTimeout(Func<CancellationToken, Task> action, CancellationToken cancellationToken, TimeSpan timeout, string errorMessage)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(timeout);
                try
                {
                    await action(cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        throw new TimeoutException(errorMessage, ex);
                    }

                    throw;
                }
            }
        }
    }
}
