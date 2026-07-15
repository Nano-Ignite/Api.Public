using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Public.Requests.Accounts;

/// <summary>
/// User Sign Up Request.
/// </summary>
public class UserSignUpRequest
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
    /// Email Address.
    /// </summary>
    [Required]
    [EmailAddress]
    public virtual string EmailAddress { get; set; } = null!;

    /// <summary>
    /// Password.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public virtual string Password { get; set; } = null!;

    /// <summary>
    /// Date of Birth.
    /// </summary>
    [Required]
    public virtual DateOnly DateOfBirth { get; set; }
}