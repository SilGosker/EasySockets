using EasySockets.Authentication;
using EasySockets.Builder;
using EasySockets.Events;
using EasySockets.Helpers;
using EasySockets.Services.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasySockets.Services;

internal sealed class EasySocketAuthenticationService
{
    private readonly EasySocketGlobalOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EasySocketAuthenticationService(IServiceScopeFactory serviceScopeFactory,
        IOptions<EasySocketGlobalOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    internal async Task<EasySocketAuthenticationResult> GetAuthenticationResultAsync(
        EasySocketTypeCache easySocketTypeCache, HttpContext context)
    {
        var roomId = _options.GetDefaultRoomId(context);
        var clientId = _options.GetDefaultClientId(context);

        var scope = context.RequestServices;

        EasySocketAuthenticationResult authenticationResult = new(
            easySocketTypeCache.Options.Authenticators.Count == 0,
            roomId,
            clientId);

        foreach (var authenticatorType in easySocketTypeCache.Options.Authenticators)
        {
            var authenticator = scope.GetService(authenticatorType) ??
                                ActivatorUtilities.CreateInstance(scope, authenticatorType);

            authenticationResult = authenticator switch
            {
                IEasySocketAsyncAuthenticator asyncAuthenticator => await asyncAuthenticator
                    .AuthenticateAsync(authenticationResult, context)
                    .ConfigureAwait(false),
                IEasySocketAuthenticator syncAuthenticator => syncAuthenticator.Authenticate(authenticationResult,
                    context),
                _ => authenticationResult
            };

            if (authenticationResult.IsAuthenticated != true) return authenticationResult;
        }

        ThrowHelper.ThrowIfInvalidRoomId(authenticationResult.RoomId, roomId);
        ThrowHelper.ThrowIfInvalidClientId(authenticationResult.ClientId, clientId);

        authenticationResult.RoomId ??= roomId;
        authenticationResult.ClientId ??= clientId;

        return authenticationResult;
    }

    internal async Task<IEasySocket?> GetInstanceAsync(EasySocketTypeCache cache, WebSocketManager websockets,
        string roomId, string clientId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var ws = websockets.WebSocketRequestedProtocols.Count > 0
            ? await websockets.AcceptWebSocketAsync(websockets.WebSocketRequestedProtocols[0]).ConfigureAwait(false)
            : await websockets.AcceptWebSocketAsync().ConfigureAwait(false);

        if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, cache.EasySocketType) is not IEasySocket easySocket)
            return null;

        easySocket.WebSocket = ws;
        easySocket.Logger = scope.ServiceProvider.GetRequiredService<ILogger<EasySocket>>();
        easySocket.Options = cache.Options;
        ((IInternalEasySocket)easySocket).RoomId = roomId;
        ((IInternalEasySocket)easySocket).ClientId = clientId;

        if (easySocket is IInternalEventSocket eventSocket)
            eventSocket.Events = ((EventSocketTypeCache)cache).EventInfos;

        return easySocket;
    }
}