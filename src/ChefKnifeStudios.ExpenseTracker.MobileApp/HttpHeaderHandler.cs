using ChefKnifeStudios.ExpenseTracker.Shared;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp;

public class HttpHeaderHandler : DelegatingHandler
{
    private readonly IAppSessionService _sessionService;
    private readonly ILogger<HttpHeaderHandler> _logger;

    public HttpHeaderHandler(IAppSessionService sessionService, ILogger<HttpHeaderHandler> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current session and add AppId header
            var session = await _sessionService.GetSessionAsync(cancellationToken);

            // Remove existing header to avoid duplicates
            if (request.Headers.Contains(Constants.AppIdHeader))
            {
                request.Headers.Remove(Constants.AppIdHeader);
            }
            request.Headers.Add(Constants.AppIdHeader, session.AppId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add AppId header to request");
            // Continue with request even if we can't add the header
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
