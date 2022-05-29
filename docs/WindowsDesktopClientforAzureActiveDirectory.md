## How to build a Windows desktop client to access APIs protected by Azure AD 
These types of apps use the **OAUTH2 Authorization Flow**. You can check what this means
[here (Microsoft's docs)](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow) or [here (auth0's docs)](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow) or [here (IETF docs)](https://datatracker.ietf.org/doc/html/rfc6749#section-4.1).

In mobile / desktop applications, an extension is needed to make this flow secure: Proof key, which we will also implement in our sample. You can read about PKCE [here (OAUTH.Net)](https://oauth.net/2/pkce/).

### 1
As a first thing, make sure you create an Application Registration in Azure AD for your client. You can check [here](CreateAClientApplicationInAzureActiveDirectory.md) how to do it.
### 2
Download [this Visual Studio Solution](https://github.com/RiccardoGMoschetti/authentico). You will be interested in these two projects:
- WindowsDesktopClient
- BankAPI (this, only in case you want to experiment calling an API of your own, which you will configure to be protected by Azure AD)

Consequently, if you want to learn how to use the client to just call Microsoft APIs (as Microsoft Graph or the Azure ARM API), just launch the desktop client.
<img src="images/StartWindowsDesktopProject.png" height="20%" alt="Startup Project in Windows Desktop"/>
