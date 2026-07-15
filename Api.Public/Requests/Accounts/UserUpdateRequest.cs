using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Public.Requests.Accounts;

/// <summary>
/// User Update Request.
/// </summary>
public class UserUpdateRequest
{
    /// <summary>
    /// First Name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public virtual string FirstName { get; set; } = null!;

    /// <summary>
    /// Last Name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public virtual string LastName { get; set; } = null!;

    /// <summary>
    /// Date of Birth.
    /// </summary>
    [Required]
    public virtual DateOnly DateOfBirth { get; set; }
}