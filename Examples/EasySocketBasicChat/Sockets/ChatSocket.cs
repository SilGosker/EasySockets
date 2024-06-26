﻿using EasySockets;

namespace EasySocketBasicChat.Sockets;

public class ChatSocket : EasySocket
{
    public override Task OnMessage(string message)
    {
        return Broadcast(message);
    }
}