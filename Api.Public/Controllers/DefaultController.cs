using System;
using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Svc.Accounts.Models.Api;
using Svc.Emailing.Models.Api;
using Svc.Locations.Models.Api;
using Svc.Places.Models.Api;

namespace Api.Public.Controllers;

/// <inheritdoc />
public class DefaultController : BaseController
{
    private readonly AccountsApi accountsApi;
    private readonly PlacesApi placesApi;
    private readonly LocationsApi locationsApi;
    private readonly EmailingApi emailingApi;

    /// <inheritdoc />
    public DefaultController(ILogger<DefaultController> logger, AccountsApi accountsApi, PlacesApi placesApi, LocationsApi locationsApi, EmailingApi emailingApi)
        : base(logger)
    {
        this.accountsApi = accountsApi ?? throw new ArgumentNullException(nameof(accountsApi));
        this.placesApi = placesApi ?? throw new ArgumentNullException(nameof(placesApi));
        this.locationsApi = locationsApi ?? throw new ArgumentNullException(nameof(locationsApi));
        this.emailingApi = emailingApi ?? throw new ArgumentNullException(nameof(emailingApi));
    }
}