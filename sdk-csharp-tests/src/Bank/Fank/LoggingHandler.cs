using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.IO;



namespace TokenioTest.Logging
{
    public class LoggingHandler : DelegatingHandler
    {

     //   private static readonly ILog logger = LogManager
     //.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            //FileInfo f = new FileInfo("log4net.config"); //please modify this line
            //var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            //log4net.Config.XmlConfigurator.Configure(logRepository,f);

            ILog logger = LogManager
     .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            logger.Debug("===============================================*******=======================================================");
            logger.Debug(string.Format("Request: {0}", request));

            if (request.Content != null)
            {
                logger.Debug(await request.Content.ReadAsStringAsync());
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            logger.Debug("Response: {0}");
            logger.Debug(response);
            if (response.Content != null)
            {
                logger.Debug(await response.Content.ReadAsStringAsync());
            }
            logger.Debug("###################################################################################################################");

            return response;

        }
    }
}
