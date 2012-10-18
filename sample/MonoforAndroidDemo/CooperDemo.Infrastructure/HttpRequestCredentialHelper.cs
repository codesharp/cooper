using System.Net;

namespace CooperDemo.Infrastructure
{
    public class HttpRequestCredentialHelper
    {
        public static void SetDefaultCredentialValidationLogic()
        {
            var logger = DependencyResolver.Resolve<ILoggerFactory>().Create("CredentialHelper");
            ServicePointManager.ServerCertificateValidationCallback =
                (srvPoint, certificate, chain, errors) =>
                {
                    logger.InfoFormat("certificate: {0}", certificate);
                    logger.InfoFormat("chain.ChainElements[0]: {0}", chain.ChainElements[0].Certificate);

                    if (certificate != null
                        && chain != null
                        && chain.ChainElements.Count > 0
                        && chain.ChainElements[0].Certificate != null
                        && certificate.ToString() == chain.ChainElements[0].Certificate.ToString())
                    {
                        logger.Info("certificate equal.");
                    }

                    //Always return true;
                    return true;
                };

            //下面两句不是必须的
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            ServicePointManager.Expect100Continue = true;
        }
    }
}

