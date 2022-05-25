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
using System.Net.Http.Headers;
using System.Linq;

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
        private APITypes? CurrentAPI;
        private void RestoreBrowser() => AuthBrowser.NavigateToString("<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Ready to authenticate you with Azure Active Directory.</body>");
        private void StartOperationsWithBrowser() => AuthBrowser.NavigateToString("<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Navigating to Azure Active Directory.</body>");
        public MainWindow()
        {
            InitializeComponent();
            AADConfiguration.FillApiEndpoints();
            FillAvailableAPICombo();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            RestoreBrowser();
        }
        private void FillAvailableAPICombo()
        {
            AvailableAPIs.Items.Clear();
           // AvailableAPIs.Items.Add(new ComboBoxItem { Content = "select" });
            foreach (var API in AADConfiguration.AADAppInfo)
            { 
                AvailableAPIs.Items.Add(new ComboBoxItem { Content = API.Value.CommonName });
            }
        }
        private bool CheckStartDataIsFilled()
        {
            if (ClientID.Text?.Length == 0 || TenantID.Text?.Length == 0 || CurrentAPI==null)
            {
                MessageBox.Show("Please Fill both ClientID and TenantID, then Choose an API", "AAD Playground: Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
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
                    AuthCode.Text = authCode ?? string.Empty;
                    AuthCode.UpdateLayout();
                    string result =
                        $"<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Successfully received an Authorization Code.<br/>This was the GET request: {AADConfiguration.AADAppInfo[CurrentAPI].AuthCodeRequestURL} </body>";
                    AuthBrowser.NavigateToString(result);
                    AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode = authCode;
                    await Task.Run(() => MessageBox.Show("Successfully Retrieved a new Authorization Code for the API.", "AAD Playground: Get Authorization Code", MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    string result =
                        $"<body style=\"background-color: #E1E5F2; font-family: 'Segoe UI Semilight', Arial, sans-serif\">Sorry, there was an error retrieving the Authorization Code. The error was: <strong>{error}</strong>, and the description was:<br/>{errorDescripition}.<br/>This was the GET request: {AADConfiguration.AADAppInfo[CurrentAPI].AuthCodeRequestURL}</body>";
                    AuthBrowser.NavigateToString(result);
                }
            }
        }
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentBrowserOperation = BrowserOperations.LoggingOUT;
            AuthBrowser.Navigate(AADConfiguration.LogoutURL);
        }
        private void GetAuthorizationCode_Click(object sender, RoutedEventArgs e)
        {   if (!CheckStartDataIsFilled()) return;
            EmptyAllText();
            this.Cursor = Cursors.Wait;
            RestoreBrowser();
            StartOperationsWithBrowser();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            AuthBrowser.Navigate(AADConfiguration.AADAppInfo[CurrentAPI].AuthCodeRequestURL);
        }
        private void AuthBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {

        }
         private void CopyAuthorizationCode_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode);
            MessageBox.Show("Successfully copied the authorization code to the clipboard", "AAD Playground: Copy Auth Code", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyCompleteTokenInfo(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            Clipboard.SetText(TokenResponse.Text);
            MessageBox.Show("Successfully copied the whole response to the clipboard", "AAD Playground: Copy Response", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyRefreshToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken);
            MessageBox.Show("Successfully copied the refresh token to the clipboard", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void CopyAccessToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken);
            MessageBox.Show("Successfully copied the access token to the clipboard. To examine it, you can use a decoding tool as https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void CopyIDToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].IDToken);
            MessageBox.Show("Successfully copied the ID token to the clipboard.  To examine it, you can use a decoding tool as https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async void CallAPI_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken);
            HttpResponseMessage response = new();
            response = await httpClient.GetAsync(AADConfiguration.AADAppInfo[CurrentAPI].APIEndpointURL);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(string.Format("Sorry, AAD returned an error:{0}{1}{0} - {2}", Environment.NewLine, response.ReasonPhrase, content), "AAD Playground: Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            else
            {
                MessageBox.Show("Successfully called the API with the Access Token.", "AAD: Call API", MessageBoxButton.OK, MessageBoxImage.Information);
                APIResponse.Text = content;
            }
        }

        private async void GetTokens_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            var tokenRequestInners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", AADConfiguration.ClientID??string.Empty),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("scope", AADConfiguration.AADAppInfo[CurrentAPI].Scope??string.Empty+"offline"),
                new KeyValuePair<string, string>("code", AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode??string.Empty),
                new KeyValuePair<string, string>("redirect_uri", AADConfiguration.AADReturnURL),
            };
            var formContent = new FormUrlEncodedContent(tokenRequestInners);
            HttpResponseMessage response = new();
            response = await httpClient.PostAsync(AADConfiguration.AADAppInfo[CurrentAPI].TokenRequestURL, formContent);
            var tokenAPIResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorSegue = MessageBox.Show(string.Format("Sorry, AAD returned an error:{0}{1}{0}with these details:{0}{2}{0}{0}Do you want to copy the error to the clipboard?", Environment.NewLine, response.ReasonPhrase, tokenAPIResponse), "AAD Playground: Error", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                if (errorSegue == MessageBoxResult.Yes)
                    Clipboard.SetText(tokenAPIResponse);
                return;
            }
            else
            {
                AADTokenResponse? AADTokenResponse = JsonSerializer.Deserialize<AADTokenResponse>(tokenAPIResponse);
                AADSecretStore.AADapiAccessData[CurrentAPI].IDToken = AADTokenResponse?.id_token;
                AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken = AADTokenResponse?.access_token;
                AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken = AADTokenResponse?.refresh_token;
                TokenResponse.Text = tokenAPIResponse;
                TokenResponse.UpdateLayout();
                MessageBox.Show("Successfully retrieved an access token and a refresh token for the API.", "AAD: Get Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void CopyAPIResult_Click(object sender, RoutedEventArgs e)
        {


        }

        private async void RefreshToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            var tokenRequestInners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", AADConfiguration.ClientID??string.Empty),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("scope", AADConfiguration.AADAppInfo[CurrentAPI].Scope??string.Empty+"offline"),
                new KeyValuePair<string, string>("refresh_token", AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken??string.Empty),
                new KeyValuePair<string, string>("redirect_uri", AADConfiguration.AADReturnURL),
            };
            var formContent = new FormUrlEncodedContent(tokenRequestInners);
            HttpResponseMessage response = new();
            response = await httpClient.PostAsync(AADConfiguration.AADAppInfo[CurrentAPI].TokenRequestURL, formContent);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorSegue = MessageBox.Show(string.Format("Sorry, AAD returned an error:{0}{1}{0}with these details:{0}{2}{0}{0}Do you want to copy the error to the clipboard?", Environment.NewLine, response.ReasonPhrase, content), "AAD Playground: Error", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                if (errorSegue == MessageBoxResult.Yes)
                    Clipboard.SetText(content);
                return;
            }
            else
            {
                AADTokenResponse? AADTokenResponse = JsonSerializer.Deserialize<AADTokenResponse>(content);
                AADSecretStore.AADapiAccessData[CurrentAPI].IDToken = AADTokenResponse?.id_token;
                AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken = AADTokenResponse?.access_token;
                AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken = AADTokenResponse?.refresh_token;
                TokenResponse.Text = content;
                AuthCode.UpdateLayout();
                MessageBox.Show("Successfully Retrieved an Access Token and a Refresh Token for the API.", "AAD: Get Token", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
        private void AvailableAPIs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? chosenAPI = ((ComboBoxItem)e.AddedItems[0])?.Content?.ToString();
            CurrentAPI = AADConfiguration.AADAppInfo.FirstOrDefault(j=>j.Value.CommonName==chosenAPI).Key;
            EmptyAllText();
            RestoreBrowser();
        }
        private void EmptyAllText()
        {

            AuthCode.Text = String.Empty;
            TokenResponse.Text = String.Empty;
            APIResponse.Text = String.Empty;
        } 
    }
}
