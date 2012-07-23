using System;
using DotNetOpenAuth.OAuth2;

namespace Cooper.Sync.Google.Helpers
{
    /// <summary>
    /// An authorization flow is the process of obtaining an AuthorizationCode 
    /// when provided with an IAuthorizationState.
    /// </summary>
    internal interface INativeAuthorizationFlow
    {
        /// <summary>
        /// Retrieves the authorization of the user for the given AuthorizationState.
        /// </summary>
        /// <param name="client">The client used for authentication.</param>
        /// <param name="authorizationState">The state requested.</param>
        /// <returns>The authorization code, or null if the user cancelled the request.</returns>
        /// <exception cref="NotSupportedException">Thrown if this flow is not supported.</exception>
        string RetrieveAuthorization(UserAgentClient client, IAuthorizationState authorizationState);
    }
}
