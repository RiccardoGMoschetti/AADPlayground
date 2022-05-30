# Creating a new Application in Azure AD

To be able to consume APIs protected by Azure AD, first you have to register an Application in Azure AD.  
Then, you will then grant this Application the possibility to access specific APIs (Microsoft's and/or created by you).  
## 1
Go to the Azure Active Directory blade in your Azure Portal, click on _App registrations_ and register a New Application in the directory.
<img src="images/CreateNewApplicationInAzureAD.png" height="20%" alt="Creating a new application in Azure Active Directory"/>

## 2
Then, describe the characteristics of the application: name, audience, type. You can also choose the type of application you are creating (in this case, public client), but this will be done more easily later, with steps (4a and 4b)
<img src="images/RegisterApplicationScreen.png" height="20%" alt="Register a new application in Azure Active Directory"/>


## 3
Grab the IDs Azure creates for you. You will need Client ID and Tenant ID in your code (the latter ID is the same for all of the apps in your tenant)
<img src="images/IDsOfAzureADApplication.png" height="20%" alt="Get clnew application in Azure Active Directory"/>

## 4a
Choose what type of client(s) you will create. Mobile and desktop clients have their own section.  
<img src="images/ConfigurePlatformsForApplication.png" height="20%" alt="Get clnew application in Azure Active Directory"/>

## 4b
Your code will have to match, as a reply URL, the same URL you configure here (_https://login.microsoftonline.com/common/oauth2/nativeclient_ is a good choice)  
<img src="images/ConfigureRedirectURIforApplicationInAzureAD.png" height="20%" alt="Configure a redirect URI for your platform in Azure AD"/>

## 5


