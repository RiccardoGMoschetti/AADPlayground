using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Text.Json;

namespace BankDesktopClient
{

    internal enum BrowserOperations
    { 
        LoggingIN, LoggingOUT
    }
   
    public partial class MainWindow : Window
    {
        //         https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow
        static HttpClient httpClient = new();
        private BrowserOperations currentBrowserOperation;
        private APITypes currentAPIOperation;
        private void RestoreBrowser() => AuthBrowser.NavigateToString("<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Ready to authenticate you with Azure Active Directory.</body>");        
        private void StartOperationsWithBrowser() => AuthBrowser.NavigateToString("<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Navigating to Azure Active Directory.</body>");

        public MainWindow()
        {
            InitializeComponent();
            AADConfiguration.FillApiEndpoints();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            currentAPIOperation = APITypes.BankInfoAPI;
            RestoreBrowser();
        }
        // nonce!
        private async void AuthBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            string URLfromNavigation = e?.Uri?.OriginalString ?? string.Empty;
            string URLquery = e?.Uri?.Query ?? string.Empty;
            if (string.IsNullOrEmpty(URLfromNavigation)) { return; }
            if (currentBrowserOperation == BrowserOperations.LoggingIN && URLfromNavigation.StartsWith(AADConfiguration.AADReturnURL))
            {
                this.Cursor = Cursors.Arrow;
                string error = HttpUtility.ParseQueryString(URLquery)?.Get("error") ?? string.Empty;
                string errorDescripition = HttpUtility.ParseQueryString(URLquery)?.Get("error_description") ?? string.Empty;

                if (string.IsNullOrEmpty(error))
                {
                    string authCode = HttpUtility.ParseQueryString(URLquery)?.Get("code") ?? string.Empty;
                    AuthCodeBankInfo.Text = authCode ?? string.Empty;
                    AuthCodeBankInfo.UpdateLayout();
                    string result =
                        $"<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Successfully received an Authorization Code.<br/>This was the GET request: {AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].AuthCodeRequestURL} </body>";
                    AuthBrowser.NavigateToString(result);
                    AADSecretStore.AADapiAccessData[currentAPIOperation].AuthCode = authCode;
                    await Task.Run(() => MessageBox.Show("Successfully retrieved a new Authorization Code for the Bank Info API.", "AAD Playground: Get Authorization Code", MessageBoxButton.OK, MessageBoxImage.Information));

                }
                else
                {
                    string result=
                        $"<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Sorry, there was an error retrieving the Authorization Code. The error was the error {error}, and the description was {errorDescripition}.<br/>This was the GET request: {AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].AuthCodeRequestURL}</body>";
                    AuthBrowser.NavigateToString(result);
                }
            }
        }
        private void LogMeOut_Click(object sender, RoutedEventArgs e)
        {
            currentBrowserOperation = BrowserOperations.LoggingOUT;
            AuthBrowser.Navigate(AADConfiguration.logoutURL);
            

        }
        private void GetAuthCodeForBankInfoAPI_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            StartOperationsWithBrowser();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            currentAPIOperation = APITypes.BankInfoAPI;
            AuthBrowser.Navigate(AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].AuthCodeRequestURL);

        }
        private void GetAuthCodeForMSGraph_Click(object sender, RoutedEventArgs e)
        {
            currentBrowserOperation = BrowserOperations.LoggingIN;
            currentAPIOperation = APITypes.MSGraph;
            AuthBrowser.Navigate(AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].AuthCodeRequestURL);
        }


        private void AuthBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Debug.Write(e.Uri.ToString());
        }

        private void CopyAuthCodeMSGraph_Click(object sender, RoutedEventArgs e)
        {

            Clipboard.SetText(AuthCodeMSGraph.Text);

        }
        private void CopyAuthBankInfo_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AuthCodeBankInfo.Text);
            MessageBox.Show("Successfully copied the authorization code to the clipboard", "AAD Playground: Copy Auth Code", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyBankAPITokenInfo_Click (object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BankInfoAPITokens.Text);
            MessageBox.Show("Successfully copied the whole response to the clipboard", "AAD Playground: Copy Response", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyBankAPIRefreshToken_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BankInfoAPITokens.Text);
            MessageBox.Show("Successfully copied the refresh token to the clipboard", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyBankAPIAccessToken_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].AccessToken);
            MessageBox.Show("Successfully copied the access token to the clipboard. To examine it, you can use a decoding tool as https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token",MessageBoxButton.OK, MessageBoxImage.Information);
        }   
        private void CopyBankAPIIDToken_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].IDToken);
            MessageBox.Show("Successfully copied the ID token to the clipboard.  To examine it, you can use a decoding tool as https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private async void GetTokensForBankInfoAPI_Click(object sender, RoutedEventArgs e)
        {

            var tokenRequestInners = new List<KeyValuePair<string, string>>
            {
                //new KeyValuePair<string, string>("resource", "https://graph.microsoft.com"),
                new KeyValuePair<string, string>("client_id", AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].ClientID??string.Empty),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("scope", AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].Scope??string.Empty+"offline"),
                new KeyValuePair<string, string>("code", AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].AuthCode??string.Empty),
                new KeyValuePair<string, string>("redirect_uri", AADConfiguration.AADReturnURL),
            };
            var formContent = new FormUrlEncodedContent(tokenRequestInners);

            HttpResponseMessage response = new();
            response=httpClient.PostAsync(AADConfiguration.CodeTokenEndpoints[APITypes.BankInfoAPI].TokenRequestURL, formContent).Result;
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
               var errorSegue= MessageBox.Show(string.Format("Sorry, AAD returned an error:{0}{1}{0}with these details:{0}{2}{0}{0}Do you want to copy the error to the clipboard?", Environment.NewLine, response.ReasonPhrase, content),"AAD Playground: Error",MessageBoxButton.YesNo,MessageBoxImage.Stop);
               if (errorSegue==MessageBoxResult.Yes)
                    Clipboard.SetText(content); 
            }
            else
            {
                MessageBox.Show("Successfully retrieved an access token and a refresh token", "AAD: Get Token", MessageBoxButton.OK, MessageBoxImage.Information);
                AADTokenResponse? AADTokenResponse = JsonSerializer.Deserialize<AADTokenResponse>(content);
                AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].IDToken = AADTokenResponse?.id_token;
                AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].AccessToken = AADTokenResponse?.access_token;
                AADSecretStore.AADapiAccessData[APITypes.BankInfoAPI].RefreshToken=AADTokenResponse?.refresh_token;
                BankInfoAPITokens.Text = content;
                AuthCodeBankInfo.UpdateLayout();
            }
        }
    }
}
