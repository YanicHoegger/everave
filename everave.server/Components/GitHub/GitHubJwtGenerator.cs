using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace everave.server.Components.GitHub
{
    public static class GitHubJwtGenerator
    {
        public static string GenerateJwt(string appId, string privateKeyPem)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());

            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            //var now = DateTimeOffset.UtcNow;
            //var token = new JwtSecurityToken(
            //    issuer: appId,
            //    notBefore: now.UtcDateTime,
            //    expires: now.AddMinutes(10).UtcDateTime,
            //    signingCredentials: credentials,
            //    claims:
            //    [
            //        new Claim("iat", now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            //    ]
            //);

            //return new JwtSecurityTokenHandler().WriteToken(token);

            var now = DateTimeOffset.UtcNow;
            var iat = now.ToUnixTimeSeconds();
            var exp = now.AddMinutes(10).ToUnixTimeSeconds();

            var payload = new JwtPayload
            {
                { "iat", iat },
                { "exp", exp },
                { "iss", appId }
            };

            var token = new JwtSecurityToken(new JwtHeader(credentials), payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
