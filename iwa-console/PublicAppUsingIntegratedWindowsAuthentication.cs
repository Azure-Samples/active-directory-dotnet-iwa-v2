/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iwa_console
{
    /// <summary>
    /// Security token provider using Integrated Windows Authentication
    /// </summary>
    public class PublicAppUsingIntegratedWindowsAuthentication
    {
        /// <summary>
        /// Constructor of a public application leveraging Integrated Windows Authentication to sign-in a user
        /// </summary>
        /// <param name="app">MSAL.NET Public client application</param>
        /// <param name="httpClient">HttpClient used to call the protected Web API</param>
        /// <remarks>
        /// For more information see https://aka.ms/msal-net-iwa
        /// </remarks>
        public PublicAppUsingIntegratedWindowsAuthentication(IPublicClientApplication app)
        {
            App = app;
        }
        protected IPublicClientApplication App { get; private set; }

        /// <summary>
        /// Acquires a token from the token cache, or Integrated Windows Authentication
        /// </summary>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult> AcquireATokenFromCacheOrIntegratedWindowwAuthenticationAsync(IEnumerable<String> scopes)
        {
            AuthenticationResult result = null;
            var accounts = await App.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    result = await App.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
                }
                catch (MsalUiRequiredException)
                {
                }
            }

            // Cache empty or no token for account in the cache, attempt by Integrated Windows Authentication
            if (result == null)
            {
                result = await GetTokenForWebApiUsingIntegratedWindowsAuthenticationAsync(scopes);
            }

            return result;
        }

        /// <summary>
        /// Gets an access token so that the application accesses the web api in the name of the user
        /// who is signed-in in Windows (for a domain joined or AAD joined machine)
        /// </summary>
        /// <returns>An authentication result, or null if the user canceled sign-in</returns>
        private async Task<AuthenticationResult> GetTokenForWebApiUsingIntegratedWindowsAuthenticationAsync(IEnumerable<string> scopes)
        {
            AuthenticationResult result=null;
            try
            {
                result = await App.AcquireTokenByIntegratedWindowsAuthAsync(scopes);
            }
            catch (MsalUiRequiredException ex) when (ex.Message.Contains("AADSTS65001"))
            {
                // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application 
                // with ID '{appId}' named '{appName}'.Send an interactive authorization request for this user and resource.

                // you need to get user consent first. This can be done, if you are not using .NET Core (which does not have any Web UI)
                // by doing (once only) an AcquireTokenAsync interactive.

                // If you are using .NET core or don't want to do an AcquireTokenInteractive, you might want to suggest the user to navigate
                // to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read
                throw;
            }
            catch (MsalServiceException)
            {
                // Kind of errors you could have (in ex.Message)

                // MsalServiceException: AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint.
                // you used common.
                // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted or otherwise organizations

                // MsalServiceException: AADSTS70002: The request body must contain the following parameter: 'client_secret or client_assertion'.
                // Explanation: this can happen if your application was not registered as a public client application in Azure AD 
                // Mitigation: in the Azure portal, edit the manifest for your application and set the `allowPublicClient` to `true` 
                throw;
            }
            catch (MsalClientException)
            {
                // Error Code: unknown_user Message: Could not identify logged in user
                // Explanation: the library was unable to query the current Windows logged-in user or this user is not AD or AAD 
                // joined (work-place joined users are not supported). 

                // Mitigation 1: on UWP, check that the application has the following capabilities: Enterprise Authentication, 
                // Private Networks (Client and Server), User Account Information

                // Mitigation 2: Implement your own logic to fetch the username (e.g. john@contoso.com) and use the 
                // AcquireTokenByIntegratedWindowsAuthAsync overload that takes in the username
                throw;
            }
            return result;
        }
    }
}
