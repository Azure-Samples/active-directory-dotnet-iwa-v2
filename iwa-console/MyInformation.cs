// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace iwa_console
{
    /// <summary>
    /// MyInformation
    /// </summary>
    public class MyInformation
    {
        /// <summary>
        /// MyInformation ctor
        /// </summary>
        /// <param name="app"></param>
        /// <param name="client"></param>
        /// <param name="microsoftGraphBaseEndpoint"></param>
        public MyInformation(IPublicClientApplication app, HttpClient client, string microsoftGraphBaseEndpoint)
        {
            tokenAcquisitionHelper = new PublicAppUsingIntegratedWindowsAuthentication(app);
            protectedApiCallHelper = new ProtectedApiCallHelper(client);
            MicrosoftGraphBaseEndpoint = microsoftGraphBaseEndpoint;
        }

        /// <summary>
        /// tokenAcquisitionHelper
        /// </summary>
        protected PublicAppUsingIntegratedWindowsAuthentication tokenAcquisitionHelper;

        /// <summary>
        /// protectedApiCallHelper
        /// </summary>
        protected ProtectedApiCallHelper protectedApiCallHelper;

        /// <summary>
        /// Scopes to request access to the protected web API (here Microsoft Graph)
        /// </summary>
        private static string[] Scopes { get; set; } = new string[] { "User.Read", "User.ReadBasic.All" };

        /// <summary>
        /// Base endpoint for Microsoft Graph
        /// </summary>
        private string MicrosoftGraphBaseEndpoint { get; set; }

        /// <summary>
        /// URLs of the protected web APIs to call (here Microsoft Graph endpoints)
        /// </summary>
        private string WebApiUrlMe { get { return $"{MicrosoftGraphBaseEndpoint}/v1.0/me"; } }
        private string WebApiUrlMyManager { get { return $"{MicrosoftGraphBaseEndpoint}/v1.0/me/manager"; } }

        /// <summary>
        /// Calls the web API and displays its information
        /// </summary>
        /// <returns></returns>
        public async Task DisplayMeAndMyManagerAsync()
        {
            AuthenticationResult authenticationResult = await tokenAcquisitionHelper
                .AcquireTokenFromCacheOrIntegratedWindowAuthenticationAsync(Scopes)
                .ConfigureAwait(false);

            if (authenticationResult != null)
            {
                DisplaySignedInAccount(authenticationResult.Account);

                string accessToken = authenticationResult.AccessToken;
                await CallWebApiAndDisplayResultAsync(WebApiUrlMe, accessToken, "Me")
                    .ConfigureAwait(false);

                await CallWebApiAndDisplayResultAsync(WebApiUrlMyManager, accessToken, "My manager")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Display Signed In Account
        /// </summary>
        /// <param name="account"></param>
        private static void DisplaySignedInAccount(IAccount account)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{account.Username} successfully signed-in");
        }

        /// <summary>
        /// Call WebApi And Display Result 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task CallWebApiAndDisplayResultAsync(string url, string accessToken, string title)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(title);
            Console.ResetColor();
            await protectedApiCallHelper.CallWebApiAndProcessResultAsync(url, accessToken, Display)
                .ConfigureAwait(false);
            Console.WriteLine();
        }

        /// <summary>
        /// Display the result of the web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }
    }
}
