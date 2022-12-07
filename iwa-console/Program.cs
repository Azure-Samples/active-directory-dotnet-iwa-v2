// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using iwa_console.MSAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace iwa_console
{
    /// <summary>
    /// This sample signs-in the user signed-in on a Windows machine joined to a Windows domain or AAD joined
    /// For more information see https://aka.ms/msal-net-iwa
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Using appsettings.json as our configuration settings and utilizing IOptions pattern - https://learn.microsoft.com/dotnet/core/extensions/options
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Read configuration
            AzureADConfig azureADConfig = configuration.GetSection("AzureAD").Get<AzureADConfig>();
            MSGraphApiConfig msGraphApiConfig = configuration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();

            var msalClientHelper = new MSALClientHelper(azureADConfig);

            var handle = WindowsHelper.GetConsoleOrTerminalWindow();
            await msalClientHelper.InitializePublicClientAppForWAMBrokerAsync(handle);

            var msGraphHelper = new MSGraphHelper(msGraphApiConfig, msalClientHelper);

            await msGraphHelper.SignInAndInitializeGraphServiceClient();

            var logedInUser = await msGraphHelper.GetMeAsync();

            Console.WriteLine("User information:\n");

            Console.WriteLine($"Display name:\n{ logedInUser.DisplayName ?? "Value not set"}\n");
            Console.WriteLine($"Employee ID:\n{ logedInUser.EmployeeId  ?? "Value not set"}\n");
            Console.WriteLine($"Job title:\n{ logedInUser.JobTitle ?? "Value not set"}\n");
            Console.WriteLine($"Email:\n{ logedInUser.Mail ?? "Value not set"}\n");
            Console.WriteLine($"Mobile phone:\n{ logedInUser.MobilePhone ?? "Value not set"}\n");
            Console.WriteLine($"About me:\n{ logedInUser.AboutMe ?? "Value not set"}\n");

           while (true)
           {
                Console.WriteLine("\nSign user out? [y/n]");
                var input = Console.ReadKey();

                if (input.KeyChar == 'y')
                {
                    msalClientHelper.SignOutUser();
                    Console.WriteLine("\nUser signed-out successfully.");
                    return;
                }

                if (input.KeyChar == 'n')
                {
                    Console.WriteLine("\nUser is not signed-out and credentials are still cached.");
                    return;
                }
           }
        }
    }
}
