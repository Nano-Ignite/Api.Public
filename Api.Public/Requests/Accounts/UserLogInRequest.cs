using System.ComponentModel.DataAnnotations;

namespace Api.Public.Requests.Accounts;

/// <summary>
/// User Log In Request.
/// </summary>
public class UserLogInRequest
{
    /// <summary>
    /// App Id.
    /// </summary>
    [MaxLength(256)]
    public virtual string AppId { get; set; } = null!;

    /// <summary>
    /// Email Address.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public virtual string EmailAddress { get; set; } = null!;

    /// <summary>
    /// Password.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public virtual string Password { get; set; } = null!;
}