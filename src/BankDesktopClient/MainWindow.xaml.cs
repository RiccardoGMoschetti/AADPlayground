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
        private APITypes CurrentAPI;
        private void RestoreBrowser() => AuthBrowser.NavigateToString("<body style=\"font-family: 'Segoe UI Semilight', Arial, sans-serif\">Ready to authenticate you with Azure AD.</body>");
        private void StartOperationsWithBrowser() => AuthBrowser.NavigateToString("<body style=\"font-family: 'Segoe UI Semilight', Arial, sans-serif\">Navigating to Azure AD.</body>");
        public MainWindow()
        {
            InitializeComponent();
            FillAvailableAPICombo();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            RestoreBrowser();
        }
        private void FillAvailableAPICombo()
        {
            AvailableAPIs.Items.Clear();
            foreach (var API in AADConfiguration.APIEndpoints)
            {
                AvailableAPIs.Items.Add(new ComboBoxItem { Content = API.Value.CommonName });
            }
        }
        private bool CheckStartDataIsFilled()
        {
            if (ClientID.Text?.Length == 0 || TenantID.Text?.Length == 0 || CurrentAPI == APITypes.Null)
            {
                MessageBox.Show("Please fill both ClientID and TenantID, then choose an API", "AAD Playground: Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        // nonce!
        private async void AuthBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
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
                        $"<body style=\"font-family: 'Segoe UI Semilight', Arial, sans-serif\">Successfully received an Authorization Code, with this GET request: {AADConfiguration.APIEndpoints[CurrentAPI].AuthCodeRequestURL}</body>";
                    AuthBrowser.NavigateToString(result);
                    AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode = authCode;
                    await Task.Run(() => MessageBox.Show("Successfully retrieved a new Authorization Code for the API", "AAD Playground: Get Authorization Code", MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    string result =
                        $"<body style=\"font-family: 'Segoe UI Semilight', Arial, sans-serif\">Sorry, there was an error retrieving the Authorization Code. The error was: <strong>{error}</strong>, and the description was:<br/>{errorDescripition}.<br/>This was the GET request: {AADConfiguration.APIEndpoints[CurrentAPI].AuthCodeRequestURL}</body>";
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
        {
            if (!CheckStartDataIsFilled()) return;
            AADConfiguration.TenantID = TenantID.Text;
            AADConfiguration.ClientID = ClientID.Text;
            EmptyAllText();
            this.Cursor = Cursors.Wait;
            RestoreBrowser();
            StartOperationsWithBrowser();
            currentBrowserOperation = BrowserOperations.LoggingIN;
            AuthBrowser.Navigate(AADConfiguration.APIEndpoints[CurrentAPI].AuthCodeRequestURL);
        }
        private void CopyAuthorizationCode_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode == null) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy Auth Code", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode);
                MessageBox.Show("Successfully copied the Authorization Code to the clipboard", "AAD Playground: Copy Auth Code", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void CopyCompleteTokenInfo(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (String.IsNullOrEmpty(TokenResponse.Text)) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy Response", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Clipboard.SetText(TokenResponse.Text);
                MessageBox.Show("Successfully copied the whole Response to the clipboard", "AAD Playground: Copy Response", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void CopyRefreshToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken == null) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken);
                MessageBox.Show("Successfully copied the Refresh Token to the clipboard", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void CopyAccessToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken == null) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken);
                MessageBox.Show("Successfully copied the Access Token to the clipboard. To examine it, you can use a decoding tool like https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void CopyIDToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken == null) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Clipboard.SetText(AADSecretStore.AADapiAccessData[CurrentAPI].IDToken);
                MessageBox.Show("Successfully copied the ID Token to the clipboard. To examine it, you can use a decoding tool like https://jwt.ms or https://jwt.io", "AAD Playground: Copy Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void CallAPI_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AADSecretStore.AADapiAccessData[CurrentAPI].AccessToken);
            HttpResponseMessage response = new();
            response = await httpClient.GetAsync(AADConfiguration.APIEndpoints[CurrentAPI].APIEndpointURL);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(string.Format("Sorry, AAD returned an error:{0}{1}{0} - {2}", Environment.NewLine, response.ReasonPhrase, content), "AAD Playground: Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            else
            {
                APIResponse.Text = content;
                APIResponse.UpdateLayout();
                MessageBox.Show("Successfully called the API with the Access Token", "AAD: Call API", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void GetTokens_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            var tokenRequestInners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", AADConfiguration.ClientID??string.Empty),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("scope", AADConfiguration.APIEndpoints[CurrentAPI].Scope??string.Empty+"offline"),
                new KeyValuePair<string, string>("code", AADSecretStore.AADapiAccessData[CurrentAPI].AuthCode??string.Empty),
                new KeyValuePair<string, string>("redirect_uri", AADConfiguration.AADReturnURL),
            };
            var formContent = new FormUrlEncodedContent(tokenRequestInners);
            HttpResponseMessage response = new();
            response = await httpClient.PostAsync(AADConfiguration.APIEndpoints[CurrentAPI].TokenRequestURL, formContent);
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
                MessageBox.Show("Successfully retrieved an Access Token and a Refresh Token for the API", "AAD: Get Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void CopyAPIResult_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            if (string.IsNullOrEmpty(APIResponse.Text)) MessageBox.Show("Nothing to copy yet", "AAD Playground: Copy API Response", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                MessageBox.Show("Successfully copied the API Response to the clipboard", "AAD Playground: Copy API Response", MessageBoxButton.OK, MessageBoxImage.Information);
                Clipboard.SetText(APIResponse.Text);
            }
        }
        private async void RefreshToken_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckStartDataIsFilled()) return;
            var tokenRequestInners = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", AADConfiguration.ClientID??string.Empty),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("scope", AADConfiguration.APIEndpoints[CurrentAPI].Scope??string.Empty+"offline"),
                new KeyValuePair<string, string>("refresh_token", AADSecretStore.AADapiAccessData[CurrentAPI].RefreshToken??string.Empty),
                new KeyValuePair<string, string>("redirect_uri", AADConfiguration.AADReturnURL),
            };
            var formContent = new FormUrlEncodedContent(tokenRequestInners);
            HttpResponseMessage response = new();
            response = await httpClient.PostAsync(AADConfiguration.APIEndpoints[CurrentAPI].TokenRequestURL, formContent);
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
                MessageBox.Show("Successfully retrieved a new Access Token and a new Refresh Token for the API", "AAD: Refresh Token", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void AvailableAPIs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosenAPI = ((ComboBoxItem)((e.AddedItems[0] ?? new ComboBoxItem()))).Content.ToString();
            CurrentAPI = AADConfiguration.APIEndpoints.FirstOrDefault(j => j.Value.CommonName == chosenAPI).Key;
            EmptyAllText();
            RestoreBrowser();
        }
        private void EmptyAllText()
        {
            AuthCode.Text = string.Empty;
            TokenResponse.Text = string.Empty;
            APIResponse.Text = String.Empty;
        }
    }
}
