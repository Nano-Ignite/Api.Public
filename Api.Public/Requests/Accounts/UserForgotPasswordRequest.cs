using System.ComponentModel.DataAnnotations;

namespace Api.Public.Requests.Accounts;

/// <summary>
/// User Forgot Password Request.
/// </summary>
public class UserForgotPasswordRequest
{
    /// <summary>
    /// Email Address.
    /// </summary>
    [Required]
    [EmailAddress]
    public virtual string EmailAddress { get; set; } = null!;
}