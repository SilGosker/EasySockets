﻿using Microsoft.AspNetCore.Http;
using EasySockets.Services;

namespace EasySockets.Middleware;

internal sealed class SocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EasySocketService _easySocketService;
    private readonly EasySocketAuthenticator _easySocketAuthenticator;
    private readonly EasySocketTypeHolder _easySocketTypeHolder;

    public SocketMiddleware(RequestDelegate next, EasySocketService easySocketService, EasySocketAuthenticator easySocketAuthenticator, EasySocketTypeHolder easySocketTypeHolder)
    {
        _next = next;
        _easySocketService = easySocketService;
        _easySocketAuthenticator = easySocketAuthenticator;
        _easySocketTypeHolder = easySocketTypeHolder;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }

        if (!_easySocketTypeHolder.TryGetValue(context.Request.Path.ToString(), out var easySocketTypeCache))
        {
            await _next.Invoke(context);
            return;
        }

        var authenticationResult = await _easySocketAuthenticator.GetAuthenticationResultAsync(
            easySocketTypeCache,
            context);

        if (!authenticationResult.IsAuthenticated)
        {
            context.Response.StatusCode = 401;
            return;
        }

        var easySocket = await _easySocketAuthenticator.GetInstance(easySocketTypeCache, context.WebSockets, authenticationResult.RoomId!, authenticationResult.ClientId!);

        if (easySocket == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        await _easySocketService.AddSocket(easySocket);
        _easySocketService.RemoveSocket(easySocket);
    }
}