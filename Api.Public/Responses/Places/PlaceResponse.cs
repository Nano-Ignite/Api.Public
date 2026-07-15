using System;
using Svc.Places.Models.Data;

namespace Api.Public.Responses.Places;

/// <summary>
/// Place Response.
/// </summary>
public class PlaceResponse
{
    /// <summary>
    /// Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="place">The <see cref="Place"/>.</param>
    public PlaceResponse(Place place)
    {
        if (place == null)
            throw new ArgumentNullException(nameof(place));

        this.Id = place.Id;
        this.Name = place.Name;
    }
}