﻿using EasySockets.DataModels;
using EasySockets.Interfaces;
using EasySockets.Middleware;

namespace EasySockets.Builder;

/// <summary>
///     The class to further configure a specific EasySocket.
/// </summary>
public class EasySocketBuilder
{
    /// <summary>
    ///     Adds a simpleSocket type with configuration to the available websocket endpoints.
    /// </summary>
    /// <typeparam name="TEasySocket">
    ///     <para>
    ///         The simpleSocket type.
    ///     </para>
    ///     <para>
    ///         Although the type is only required to implement the <see cref="IEasySocket" /> interface, it is not
    ///         recommended to use that as
    ///         the type parameter. <br />
    ///         It is recommended to inherit from the <see cref="EasySocket" />, <see cref="EventSocket{TEvent}" /> or
    ///         <see cref="EventSocket" /> class
    ///         and use that as the type parameter, as these contain logic the middleware expects it to have (like accepting
    ///         and receiving messages through websockets).
    ///     </para>
    /// </typeparam>
    /// <param name="url">The endpoint that is made available for clients websocket requests.</param>
    /// <param name="configure">
    ///     An <see cref="Action{EasySocketOptions}" /> to configure the given options of the specific
    ///     simpleSocket.
    /// </param>
    /// <returns>A <see cref="EasySocketBuilder" /> that can further configure the simple socket behaviors.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public EasySocketBuilder AddEasySocket<TEasySocket>(string url, Action<EasySocketOptions>? configure = null)
        where TEasySocket : IEasySocket
    {
        return AddEasySocket(url, typeof(TEasySocket), configure);
    }

    private EasySocketBuilder AddEasySocket(string url, Type simpleSocketType,
        Action<EasySocketOptions>? configure)
    {
        if (url == null)
            throw new ArgumentNullException(nameof(url));
        var options = new EasySocketOptions();
        configure?.Invoke(options);
        if (options == null)
            throw new ArgumentException($"The {nameof(configure)} method cannot make the options null",
                nameof(configure));
        EasySocketInstanceFactory.AddType(url, EasySocketTypeContainer.Create(simpleSocketType, options));
        return this;
    }
}