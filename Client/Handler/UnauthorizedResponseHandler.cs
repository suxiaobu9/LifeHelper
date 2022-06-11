using Microsoft.AspNetCore.Components;
using System.Net;

namespace LifeHelper.Client.Handler;

public class UnauthorizedResponseHandler : DelegatingHandler
{
    private readonly NavigationManager navigationManager;

    public UnauthorizedResponseHandler(NavigationManager navigationManager)
    {
        this.navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            navigationManager.NavigateTo("Logout");
        }

        return response;

    }
}
