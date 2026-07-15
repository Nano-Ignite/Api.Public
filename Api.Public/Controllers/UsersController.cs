using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Nano.App.ApiClient.Requests;
using Nano.Common.Consts;
using Nano.Data.Abstractions.Identity.Extensions;
using NetTopologySuite.Geometries;
using Svc.Locations.Models.Api;
using Svc.Places.Models.Criterias.Types;
using Svc.Places.Models.Data;

namespace Api.Public.Controllers;

/// <inheritdoc />
public class UsersController(ILogger<UsersController> logger, LocationsApi locationsApi)
    : BaseController(logger)
{
    /// <summary>
    /// Report Location.
    /// </summary>
    /// <param name="coordinate">The coordinate.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>Void.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpPost]
    [Route("report-location")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> ReportLocationAsync([FromRoute][Required]LatLng coordinate, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        await locationsApi.Entity
            .CreateAsync<UserLocation>(new CreateRequest
            {
                Entity = new UserLocation
                {
                    UserId = jwtUserId,
                    Coordinate = new Point(coordinate.Longitude, coordinate.Latitude)
                }
            }, cancellationToken);

        return this.Accepted();
    }
}