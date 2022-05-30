## Creating a new Application in Azure AD

To be able to consume APIs protected by Azure AD, first you have to register an Application in Azure AD.  
Then, you will then grant this Application the possibility to access specific APIs (Microsoft's and/or created by you).  
## 1
Go to the Azure Active Directory blade in your Azure Portal, click on App registrations and register a New Application in the directory.
<img src="images/CreateNewApplicationInAzureAD.png" height="20%" alt="Creating a new application in Azure Active Directory"/>

## 2
Then, describe the characteristics of the application: name, audience, type
<img src="images/RegisterApplicationScreen.png" height="20%" alt="Register a new application in Azure Active Directory"/>

## 3
Grab the IDs. In your application code, you will need these data provided by AAD: Client ID and Tenant ID (the latter is the same for all of your apps)
<img src="images/IDsOfAzureADApplication.png" height="20%" alt="Get clnew application in Azure Active Directory"/>

