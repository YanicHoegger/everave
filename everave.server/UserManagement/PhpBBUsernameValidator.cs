using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace everave.server.UserManagement
{
    public class PhpBBUsernameValidator : IUserValidator<ApplicationUser>
    {
        // Example: disallowed names and regex patterns (like phpBB's)
        private static readonly string[] DisallowedUsernames =
        [
            "admin", "moderator", "support", "*bot*"
        ];

        private static readonly Regex InvalidCharsRegex = new Regex(
            @"[:@\[\]\""\n\r\t]", // phpBB disallowed characters
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var errors = new List<IdentityError>();
            var username = user.UserName;

            if (string.IsNullOrWhiteSpace(username))
            {
                errors.Add(new IdentityError
                {
                    Code = "UsernameRequired",
                    Description = "Username is required."
                });
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            var trimmedUsername = username.Trim();

            // Check length
            if (trimmedUsername.Length < 3 || trimmedUsername.Length > 29)
            {
                errors.Add(new IdentityError
                {
                    Code = "UsernameLength",
                    Description = "Username must be between 3 and 20 characters."
                });
            }

            // Check disallowed patterns (wildcards)
            foreach (var disallowed in DisallowedUsernames)
            {
                var pattern = "^" + Regex.Escape(disallowed).Replace("\\*", ".*") + "$";
                if (Regex.IsMatch(trimmedUsername, pattern, RegexOptions.IgnoreCase))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "UsernameDisallowed",
                        Description = $"Username '{trimmedUsername}' is not allowed."
                    });
                    break;
                }
            }

            // Check for invalid characters
            if (InvalidCharsRegex.IsMatch(trimmedUsername))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidUsernameCharacters",
                    Description = "Username contains invalid characters (e.g. :, @, [, ], etc.)."
                });
            }

            return errors.Any()
                ? Task.FromResult(IdentityResult.Failed(errors.ToArray()))
                : Task.FromResult(IdentityResult.Success);
        }
    }
}
