using System.ComponentModel.DataAnnotations;

namespace Api.Public.Requests.Accounts;

/// <summary>
/// User Log In Refresh Request.
/// </summary>
public class UserLogInRefreshRequest
{
    /// <summary>
    /// Refresh Token.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public virtual string RefreshToken { get; set; } = null!;
}