using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iwa_console.MSAL
{
    public class MSGraphApiConfig
    {
        /// <summary>
        /// Gets or sets the MS Graph base URL.
        /// </summary>
        /// <value>
        /// The Microsoft Graph base URL.
        /// </value>
        public string MSGraphBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the scopes for MS graph call.
        /// </summary>
        /// <value>
        /// The scopes as space separated string.
        /// </value>
        public string Scopes { get; set; }

        /// <summary>
        /// Gets the scopes in a format as expected by the various MSAL SDK methods.
        /// </summary>
        /// <value>
        /// The scopes as array.
        /// </value>
        public string[] ScopesArray
        {
            get
            {
                return Scopes.Split(' ');
            }
        }
    }
}
