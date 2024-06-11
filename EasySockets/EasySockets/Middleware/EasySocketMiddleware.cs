using EasySockets.Services;
using EasySockets.Services.Caching;
using Microsoft.AspNetCore.Http;

namespace EasySockets.Middleware;

internal sealed class SocketMiddleware
{
    private readonly EasySocketAuthenticationService _easySocketAuthenticator;
    private readonly EasySocketService _easySocketService;
    private readonly EasySocketTypeHolder _easySocketTypeHolder;
    private readonly RequestDelegate _next;

    public SocketMiddleware(RequestDelegate next, EasySocketService easySocketService,
        EasySocketAuthenticationService easySocketAuthenticator, EasySocketTypeHolder easySocketTypeHolder)
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
            await _next.Invoke(context).ConfigureAwait(false);
            return;
        }

        if (!_easySocketTypeHolder.TryGetValue(context.Request.Path, out var easySocketTypeCache))
        {
            await _next.Invoke(context).ConfigureAwait(false);
            return;
        }

        var authenticationResult = await _easySocketAuthenticator.GetAuthenticationResultAsync(
            easySocketTypeCache,
            context).ConfigureAwait(false);

        if (!authenticationResult.IsAuthenticated)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var easySocket = await _easySocketAuthenticator.GetInstanceAsync(easySocketTypeCache, context.WebSockets,
            authenticationResult.RoomId!, authenticationResult.ClientId!).ConfigureAwait(false);

        if (easySocket == null)
        {
            await _next.Invoke(context).ConfigureAwait(false);
            return;
        }

        await _easySocketService.AddSocket(easySocket).ConfigureAwait(false);
        _easySocketService.RemoveSocket(easySocket);
    }
}