using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BankAPI
{
    public static class AADConfiguration
    {
        public static OpenIdConnectConfiguration? OpenIdConnectConfiguration;
        public const string Instance = "https://login.microsoftonline.com/";
        public const string TenantID = "e0aeaef9-9bf6-4b6d-bebb-25ee10f820af";
        public const string BankInfoAPIClientID = "6fa7f288-4641-4320-82be-5e0bbaaaa0b7";
        public const string WellKnownEndpoint = $"{Instance}{TenantID}/v2.0/.well-known/openid-configuration";
        public const string BankInfoAPIScope = "My.CurrentBalance.Read";
        public const string ScopeType = @"http://schemas.microsoft.com/identity/claims/scope";
        static AADConfiguration()
        {
            OpenIdConnectConfiguration = OpenIdConnectConfigurationRetriever.GetAsync(WellKnownEndpoint, CancellationToken.None).Result;
        }
    }
}
