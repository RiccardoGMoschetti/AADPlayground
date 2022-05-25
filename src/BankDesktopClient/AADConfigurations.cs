using System.Collections.Generic;

namespace BankDesktopClient
{
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
        internal static Dictionary<APITypes?, AADApplicationSecrets> AADapiAccessData { get; set; }
        static AADSecretStore() {
            AADapiAccessData= new Dictionary<APITypes?, AADApplicationSecrets>();
            AADapiAccessData.Add(APITypes.BankInfoAPI, new AADApplicationSecrets());
            AADapiAccessData.Add(APITypes.MSGraph, new AADApplicationSecrets());
            AADapiAccessData.Add(APITypes.AzureARM, new AADApplicationSecrets());
        }
    }
    internal static class AADConfiguration
    {
        internal const string LoginBaseUrl= "https://login.microsoftonline.com";
        internal const string AADReturnURL = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        internal const string TenantID = "e0aeaef9-9bf6-4b6d-bebb-25ee10f820af";
        internal const string ClientID = "bc7a21b8-d4d8-449e-bcad-c740e843fa49";
        internal static readonly Dictionary<APITypes?, AADAppEndpoints> AADAppInfo = new();

        internal const string LogoutURL = $"{LoginBaseUrl}/{TenantID}/oauth2/logout?client_id={ClientID}&redirect_uri={AADReturnURL}";
        public static void FillApiEndpoints()
        {
            // Add BankInfoAPI endpoints
            AADAppInfo.Add(APITypes.BankInfoAPI, new AADAppEndpoints
            {
                CommonName ="Sample Bank API (http://localhost)",
                AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20api://BankInfoAPI/My.CurrentBalance.Read%20offline_access",
                TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                Scope = "api://BankInfoAPI/My.CurrentBalance.Read",
                APIEndpointURL = "https://localhost:7268/currentBalance"
            });
            AADAppInfo.Add(APITypes.MSGraph, new AADAppEndpoints
            {
                CommonName = "Microsoft Graph",
                AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20https://graph.microsoft.com/profile%20offline_access",
                TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                Scope = "https://graph.microsoft.com/profile",
                APIEndpointURL = "https://graph.microsoft.com/v1.0/me/"
            });
            AADAppInfo.Add(APITypes.AzureARM, new AADAppEndpoints
            {
                CommonName = "Azure Resource Manager",
                TokenRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token",
                Scope = "https://management.azure.com/user_impersonation",
                AuthCodeRequestURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={ClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=https://management.azure.com/user_impersonation%20offline_access",

                APIEndpointURL = "https://management.azure.com/tenants?api-version=2020-01-01"
            });
        }
    }
}
