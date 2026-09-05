using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.NetCoreServer.Session;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LuciAgent.Server.Core;

public class AppSession : WsSession
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AppSession(AppServer server) : base(server)
    {

    }

    /// <summary>
    /// Handle the received data from the client, 
    /// this method is called when data is received from the client.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);
    }

    /// <summary>
    /// Handle the received HTTP request from the client, 
    /// this method is called when an HTTP request is received from the client.
    /// </summary>
    /// <param name="request"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnReceivedRequest(RequestModel request)
    {
        RouteHandler.Route(request, this);
    }

    /// <summary>
    /// Handle the received WebSocket data from the client, 
    /// this method is called when WebSocket data is received from the client.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        base.OnWsReceived(buffer, offset, size);
    }

    /// <summary>
    /// Handle the connection of the client, 
    /// this method is called when the client connects.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnConnected()
    {
        base.OnConnected();
        Info<char>($"[{SessionInfo.Id}]: connected", this);
    }

    /// <summary>
    /// Handle the disconnection of the client, 
    /// this method is called when the client disconnects.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Info<char>($"[{SessionInfo.Id}]: disconnected", this);
    }

    /// <summary>
    /// Handle errors that occur during communication with the client, 
    /// this method is called when an error occurs.
    /// </summary>
    /// <param name="error"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnError(SocketError error)
    {
        base.OnError(error);
        Error<char>($"[{SessionInfo.Id}]: error: {error}", this);
    }
}