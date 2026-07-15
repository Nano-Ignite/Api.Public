using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Api.Public.Responses.Places;
using DynamicExpression.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Nano.App.ApiClient.Requests;
using Nano.Common.Consts;
using Nano.Common.Extensions;
using Nano.Data.Abstractions.Identity.Extensions;
using Svc.Places.Models.Api;
using Svc.Places.Models.Criterias;
using Svc.Places.Models.Criterias.Types;
using Svc.Places.Models.Data;

namespace Api.Public.Controllers;

/// <inheritdoc />
public class PlacesController(ILogger<PlacesController> logger, PlacesApi placesApi) 
    : BaseController(logger)
{
    /// <summary>
    /// Get Place By Id.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place response.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("{placeId:guid}")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(PlaceResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlaceAsync([FromRoute][Required]Guid placeId, CancellationToken cancellationToken = default)
    {
        var place = await placesApi.Entity
            .GetAsync<Place>(placeId, cancellationToken);

        if (place == null)
        {
            return this.NotFound();
        }

        var response = new PlaceResponse(place);

        return this.Ok(response);
    }

    /// <summary>
    /// Auto Complete Places.
    /// </summary>
    /// <param name="keyword">The keyword.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("auto-complete")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> AutoCompletePlacesAsync([FromRoute][Required][MinLength(2)]string keyword, CancellationToken cancellationToken = default)
    {
        var places = await placesApi.Entity
            .QueryAsync<Place, PlaceQueryCriteria>(new Query<PlaceQueryCriteria>
            {
                Criteria =
                {
                    Keyword = keyword
                },
                Paging =
                {
                    Count = 10
                },
                Order =
                {
                    By = nameof(Place.Name)
                }
            }, cancellationToken);

        var responses = places
            .Select(x => new PlaceResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Get Places Within.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("within")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlacesWithinAsync([FromQuery][Required]Viewport viewport, CancellationToken cancellationToken = default)
    {
        var places = await placesApi.Entity
            .QueryAsync<Place, PlaceQueryCriteria>(new Query<PlaceQueryCriteria>
            {
                Criteria =
                {
                    Viewport = viewport
                },
                Paging =
                {
                    Count = 100
                },
                Order =
                {
                    By = nameof(Place.Name)
                }
            }, cancellationToken);

        var responses = places
            .Select(x => new PlaceResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Get Place Logo Image.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>The place logo.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpGet]
    [Route("{placeId:guid}/logo")]
    [Produces(HttpContentType.JPEG, HttpContentType.PNG)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlaceLogoImageAsync([FromRoute][Required]Guid placeId, CancellationToken cancellationToken = default)
    {
        var namedStream = await placesApi
            .GetPlaceLogoImageAsync(placeId, cancellationToken);

        if (namedStream == null)
        {
            return this.NotFound();
        }

        var extension = Path.GetExtension(namedStream.Name);

        var httpContentType = extension
            .GetHttpContentType();

        return this.File(namedStream.Stream, httpContentType, namedStream.Name);
    }

    /// <summary>
    /// Get Place Pictures.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place picture responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("{placeId:guid}/pictures")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlacePictureResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlacePicturesAsync([FromRoute][Required]Guid placeId, [FromQuery]Pagination? paging, CancellationToken cancellationToken = default)
    {
        var pictures = await placesApi.Entity
            .QueryAsync<PlacePicture, PlacePictureQueryCriteria>(new QueryRequest<PlacePictureQueryCriteria>
            {
                Query =
                {
                    Criteria =
                    {
                        PlaceId = placeId
                    },
                    Order =
                    {
                        By = nameof(PlacePicture.OrderIndex)
                    },
                    Paging = paging ?? new Pagination()
                },
                IncludeDepth = 0
            }, cancellationToken);

        var responses = pictures
            .Select(x => new PlacePictureResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Get Place Picture File.
    /// </summary>
    /// <param name="pictureId">The picture id.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>The profile picture.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpGet]
    [Route("pictures/{pictureId:guid}")]
    [Produces(HttpContentType.JPEG, HttpContentType.PNG)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlacePictureAsync([FromRoute][Required]Guid pictureId, CancellationToken cancellationToken = default)
    {
        var namedStream = await placesApi
            .GetPlacePictureAsync(pictureId, cancellationToken);

        if (namedStream == null)
        {
            return this.NotFound();
        }

        var extension = Path.GetExtension(namedStream.Name);

        var httpContentType = extension
            .GetHttpContentType();

        return this.File(namedStream.Stream, httpContentType, namedStream.Name);
    }

    /// <summary>
    /// Get Place Favorites.
    /// </summary>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place favorite user context responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("favorites")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceFavoriteResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlaceFavoritesAsync([FromQuery]Pagination? paging, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        var placeFavorites = await placesApi.Entity
            .QueryAsync<PlaceFavorite, PlaceFavoriteQueryCriteria>(new QueryRequest<PlaceFavoriteQueryCriteria>
            {
                Query =
                {
                    Criteria =
                    {
                        UserId = jwtUserId
                    },
                    Paging = paging ?? new Pagination()
                }
            }, cancellationToken);

        var responses = placeFavorites
            .Select(x => new PlaceFavoriteResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Add Favorite Place.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>Void.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpPost]
    [Route("{placeId:guid}/favorites/add")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> AddFavoritePlaceAsync([FromRoute][Required]Guid placeId, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        var placeFavorite = await placesApi
            .AddPlaceFavoriteAsync(placeId, jwtUserId, cancellationToken);

        if (placeFavorite == null)
        {
            return this.NotFound();
        }

        return this.Created("{placeId}/favorites/add", placeFavorite);
    }

    /// <summary>
    /// Remove Favorite Place.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>Void.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpDelete]
    [Route("{placeId:guid}/favorites/remove")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> RemoveFavoritePlaceAsync([FromRoute][Required]Guid placeId, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        await placesApi
            .RemovePlaceFavoriteAsync(placeId, jwtUserId, cancellationToken);

        return this.Ok();
    }

    /// <summary>
    /// Get Visited Places.
    /// </summary>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place visit user context responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("visited")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceVisitResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlacesVisitedAsync([FromQuery]Pagination? paging, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        var placeVisits = await placesApi.Entity
            .QueryAsync<PlaceVisit, PlaceVisitQueryCriteria>(new QueryRequest<PlaceVisitQueryCriteria>
            {
                Query =
                {
                    Criteria =
                    {
                        UserId = jwtUserId
                    },
                    Paging = paging ?? new Pagination()
                }
            }, cancellationToken);

        var responses = placeVisits
            .Select(x => new PlaceVisitResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Get Place Visits.
    /// </summary>
    /// <param name="placeId">The place id-</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place visit user context responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("{placeId:guid}/visits")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceVisitResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlaceVisitsAsync([FromRoute][Required]Guid placeId, [FromQuery]Pagination? paging, CancellationToken cancellationToken = default)
    {
        var jwtUserId = this.HttpContext
            .GetJwtUserId<Guid>();

        var placeVisits = await placesApi.Entity
            .QueryAsync<PlaceVisit, PlaceVisitQueryCriteria>(new QueryRequest<PlaceVisitQueryCriteria>
            {
                Query =
                {
                    Criteria =
                    {
                        PlaceId = placeId,
                        UserId = jwtUserId
                    },
                    Paging = paging ?? new Pagination()
                }
            }, cancellationToken);

        var responses = placeVisits
            .Select(x => new PlaceVisitResponse(x));

        return this.Ok(responses);
    }

    /// <summary>
    /// Get Place Visitors.
    /// </summary>
    /// <param name="placeId">The place id.</param>
    /// <param name="paging">The paging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The place visit responses.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="500">Error occured.</response>
    [HttpGet]
    [Route("{placeId:guid}/visitors")]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<PlaceVisitResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetPlaceVisitorsAsync([FromRoute][Required]Guid placeId, [FromQuery]Pagination? paging, CancellationToken cancellationToken = default)
    {
        var placeVisits = await placesApi.Entity
            .QueryAsync<PlaceVisit, PlaceVisitQueryCriteria>(new QueryRequest<PlaceVisitQueryCriteria>
            {
                Query =
                {
                    Criteria =
                    {
                        PlaceId = placeId
                    },
                    Paging = paging ?? new Pagination()
                }
            }, cancellationToken);

        var responses = placeVisits
            .Select(x => new PlaceVisitResponse(x));

        return this.Ok(responses);
    }
}