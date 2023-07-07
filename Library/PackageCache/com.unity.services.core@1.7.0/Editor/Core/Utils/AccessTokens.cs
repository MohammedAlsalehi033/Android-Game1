using System.Threading.Tasks;
using Unity.Services.Core.Scheduler.Internal;
using UnityEditor;

namespace Unity.Services.Core.Editor
{
    /// <summary>
    /// Helper class to get the different kind of tokens used by services at editor time.
    /// </summary>
    public class AccessTokens : IAccessTokens
    {
        readonly IGatewayTokens m_GatewayTokens;

        internal AccessTokens(IGatewayTokens gatewayTokens)
        {
            m_GatewayTokens = gatewayTokens;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="AccessTokens"/> class.
        /// </summary>
        public AccessTokens()
        {
            var env = new CloudEnvironmentConfigProvider();
            ITokenExchangeUrls urls;
            if (env.IsStaging())
            {
                urls = new StagingTokenExchangeUrls();
            }
            else
            {
                urls = new ProductionTokenExchangeUrls();
            }
            m_GatewayTokens = new GatewayTokens(new TokenExchange(urls), new UtcTimeProvider());
        }

        /// <summary>
        /// The access token used by Genesis.
        /// </summary>
        /// <returns>
        /// Genesis Access Token.
        /// </returns>
        public static string GetGenesisToken()
        {
            return CloudProjectSettings.accessToken;
        }

        /// <summary>
        /// Task that represents an asynchronous operation to get services gateway token.
        /// </summary>
        /// <returns>
        /// Task with a result that represents the services gateway token.
        /// </returns>
        public Task<string> GetServicesGatewayTokenAsync()
        {
            return m_GatewayTokens.GetGatewayTokenAsync(GetGenesisToken());
        }

        string IAccessTokens.GetGenesisToken()
        {
            return GetGenesisToken();
        }
    }
}
