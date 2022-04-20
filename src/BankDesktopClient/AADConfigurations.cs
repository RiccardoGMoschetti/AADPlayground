using System.Collections.Generic;

namespace BankDesktopClient
{
    internal struct AADEndpoint
    {
        internal string? AuthCodeRequestURL { get; set; }
        internal string? TokenRequestURL { get; set; }
        internal string? ClientID { get; set; }
        internal string? Scope { get; set; }    
    }
    internal class AADAccessSecret
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
        internal static Dictionary<APITypes, AADAccessSecret> AADapiAccessData { get; set; }
        static AADSecretStore() {
            AADapiAccessData= new Dictionary<APITypes, AADAccessSecret>();
            AADapiAccessData.Add(APITypes.BankInfoAPI, new AADAccessSecret());
            AADapiAccessData.Add(APITypes.MSGraph, new AADAccessSecret());
          }
    }
    internal static class AADConfiguration
    {
        internal const string loginBaseUrl= "https://login.microsoftonline.com";
        internal const string AADReturnURL = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        internal const string tenantID = "e0aeaef9-9bf6-4b6d-bebb-25ee10f820af";
        internal const string bankDesktopClientClientID = "bc7a21b8-d4d8-449e-bcad-c740e843fa49";
        internal const string bankInfoAPIScope = "api://BankInfoAPI/Info.Read";
        internal static readonly Dictionary<APITypes, AADEndpoint> CodeTokenEndpoints = new();
        internal static readonly string bankInfoAPITokenURL=$"{loginBaseUrl}/{tenantID}/oauth2/v2.0/token";
        internal static readonly string bankAuthURL = $"{loginBaseUrl}/{tenantID}/oauth2/v2.0/authorize?client_id={bankDesktopClientClientID}&response_type=code&redirect_uri={AADReturnURL}&response_mode=query&scope=openid%20{bankInfoAPIScope}%20offline_access";
        internal const string logoutURL = $"{loginBaseUrl}/{tenantID}/oauth2/logout?client_id={bankDesktopClientClientID}&redirect_uri={AADReturnURL}";

        public static void FillApiEndpoints()
        {
            CodeTokenEndpoints.Add(APITypes.BankInfoAPI, new AADEndpoint { AuthCodeRequestURL = bankAuthURL, TokenRequestURL = bankInfoAPITokenURL, ClientID=bankDesktopClientClientID, Scope=bankInfoAPIScope});
        }
    }

}
