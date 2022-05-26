using System.Collections.Generic;

namespace WindowsDesktopClient
{
    internal enum APITypes
    {
        Null, BankInfoAPI, MSGraph, AzureARM
    }
    public struct AADTokenResponse
    {
        public string? token_type { get; set; }
        public string? scope { get; set; }
        public int? expires_in { get; set; }
        public int? ext_expires_in { get; set; }
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? id_token { get; set; }
    }
    internal struct AADAppEndpoints
    {
        internal string? CommonName { get; set; }
        internal string? AuthCodeRequestURL { get; set; }
        internal string? TokenRequestURL { get; set; }
        internal string? APIEndpointURL { get; set; }
        internal string? Scope { get; set; }
    }
    internal class AADApplicationSecrets
    {
        internal string? AuthCode { get; set; }
        internal string? IDToken { get; set; }
        internal string? AccessToken { get; set; }
        internal string? RefreshToken { get; set; }
        internal string? PKCECodeChallenge { get; set; }
        internal string? PKCECodeVerifier { get; set; }
    }
    internal static class AADSecretStore
    {
        internal static Dictionary<APITypes, AADApplicationSecrets> AADapiAccessData { get; set; }
        static AADSecretStore()
        {
            AADapiAccessData = new Dictionary<APITypes, AADApplicationSecrets>();
            AADapiAccessData.Add(APITypes.BankInfoAPI, new AADApplicationSecrets());
            AADapiAccessData.Add(APITypes.MSGraph, new AADApplicationSecrets());
            AADapiAccessData.Add(APITypes.AzureARM, new AADApplicationSecrets());
        }
    }
    internal static class AADConfiguration
    {
        internal const string LoginBaseUrl = "https://login.microsoftonline.com";
        internal const string AADReturnURL = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        internal static string TenantID = string.Empty;
        internal static string ClientID = string.Empty;
        internal static string LogoutURL => $"{LoginBaseUrl}/{TenantID}/oauth2/logout?client_id={ClientID}&redirect_uri={AADReturnURL}";
        internal static Dictionary<APITypes, AADAppEndpoints> APIEndpoints
        {
            get
            {
                Dictionary<APITypes, AADAppEndpoints> apiEndpoints = new();
                apiEndpoints.Add(APITypes.BankInfoAPI, new AADAppEndpoints
                {
                    CommonName = "Sample Bank API (http://localhost)",
                    AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20api://BankInfoAPI/My.CurrentBalance.Read%20offline_access",
                    TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                    Scope = "api://BankInfoAPI/My.CurrentBalance.Read",
                    APIEndpointURL = "https://localhost:7268/currentBalance"
                });
                apiEndpoints.Add(APITypes.MSGraph, new AADAppEndpoints
                {
                    CommonName = "Microsoft Graph",
                    AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20https://graph.microsoft.com/profile%20offline_access",
                    TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                    Scope = "https://graph.microsoft.com/profile",
                    APIEndpointURL = "https://graph.microsoft.com/v1.0/me/"
                });
                apiEndpoints.Add(APITypes.AzureARM, new AADAppEndpoints
                {
                    CommonName = "Azure Resource Manager",
                    TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                    Scope = "https://management.azure.com/user_impersonation",
                    AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=https://management.azure.com/user_impersonation%20offline_access",

                    APIEndpointURL = "https://management.azure.com/tenants?api-version=2020-01-01"
                });
                return apiEndpoints;
            }
        }
    }
}
