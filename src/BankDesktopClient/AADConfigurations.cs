using System.Collections.Generic;

namespace BankDesktopClient
{
    internal struct AADApplication
    {
        internal string? AuthCodeRequestURL { get; set; }
        internal string? TokenRequestURL { get; set; }
        internal string? APIEndpointURL { get; set; }   
        internal string? ClientID { get; set; }
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
        static AADSecretStore() {
            AADapiAccessData= new Dictionary<APITypes, AADApplicationSecrets>();
            AADapiAccessData.Add(APITypes.BankInfoAPI, new AADApplicationSecrets());
            AADapiAccessData.Add(APITypes.MSGraph, new AADApplicationSecrets());
          }
    }
    internal static class AADConfiguration
    {
        internal const string LoginBaseUrl= "https://login.microsoftonline.com";
        internal const string AADReturnURL = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        internal const string TenantID = "e0aeaef9-9bf6-4b6d-bebb-25ee10f820af";
        internal const string BankDesktopClientClientID = "bc7a21b8-d4d8-449e-bcad-c740e843fa49";
        internal const string BankInfoAPIScope = "api://BankInfoAPI/My.CurrentBalance.Read";
        internal static readonly Dictionary<APITypes, AADApplication> CodeTokenEndpoints = new();
        internal static readonly string BankInfoAPITokenURL=$"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/token";
        internal static readonly string BankAuthURL = $"{LoginBaseUrl}/{TenantID}/oauth2/v2.0/authorize?client_id={BankDesktopClientClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20{BankInfoAPIScope}%20offline_access";
        internal const string LogoutURL = $"{LoginBaseUrl}/{TenantID}/oauth2/logout?client_id={BankDesktopClientClientID}&redirect_uri={AADReturnURL}";
        internal const string BankInfoAPIEndPoint = "https://localhost:7268/currentBalance";
        public static void FillApiEndpoints()
        {
            CodeTokenEndpoints.Add(APITypes.BankInfoAPI, new AADApplication { AuthCodeRequestURL = BankAuthURL, TokenRequestURL = BankInfoAPITokenURL, ClientID=BankDesktopClientClientID, Scope=BankInfoAPIScope, APIEndpointURL=BankInfoAPIEndPoint});
        }
    }

}
