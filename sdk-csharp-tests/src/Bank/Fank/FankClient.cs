using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Io.Token.Proto.Bankapi;
using log4net;
using Refit;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Utils;
using FankAccount = Io.Token.Proto.Bankapi.Account;
using TokenioTest.Logging;


namespace TokenioTest.Bank.Fank
{
    public class FankClient
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan HTTP_TIMEOUT = TimeSpan.FromSeconds(90);
        private readonly IFankClientApi fankApi;

        //private IRestClient restClient;
        private HttpClient httpClient;

        public FankClient(string hostName, int port, bool useSsl)
        {
            string protocol = useSsl ? "https" : "http";
            string urlFormat = "{0}://{1}:{2}";
            //adapter = new RestAdapter(string.Format(urlFormat,protocol,hostName,port));
            //fankApi = adapter.Create<IFankClientApi>();

            string baseUrl = string.Format(urlFormat, protocol, hostName, port);
            httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = HTTP_TIMEOUT;
            fankApi = RestService.For<IFankClientApi>(httpClient);

        }

        public  Task<Client> AddClient(string bic, string firstName, string lastName)
        {
            var request = new AddClientRequest
            {
                FirstName = firstName,
                LastName = lastName,
            };
            var api = fankApi.AddClient(
                            bic,
                            Util.ToJson(request));
            AddClientResponse response = wrap(api,
            new AddClientResponse());
            return Task.Run(() =>
            {
                return response.Client;
            });

        }

   
        public  Task<FankAccount> AddAccount(
            Client client,
            string name,
            string bic,
            string number,
            double amount,
            string currency)
        {

            var request = new AddAccountRequest
            {
                ClientId = client.Id,
                Name = name,
                AccountNumber = number,
                Balance = new Money
                {
                    Value = amount.ToString(),
                    Currency = currency
                }
            };

            var api = 
                    fankApi.AddAccount(
                            bic,
                            client.Id,
                            Util.ToJson(request));
            AddAccountResponse response = wrap(api,
                    new AddAccountResponse());
            return Task.Run(() =>
            {
                return response.Account;
            });
        }

        public  Task<BankAuthorization> StartAccountsLinking(
            string alias,
            string clientId,
            string bic,
            IList<string> accountNumbers)
        {
            var request = new AuthorizeLinkAccountsRequest
            {
                MemberId = alias,
                ClientId = clientId
            };
            request.Accounts.Add(accountNumbers);
            var api = fankApi.AuthorizeLinkAccounts(
                            bic,
                            clientId,
                            Util.ToJson(request));
            BankAuthorization response = wrap(api,
                    new BankAuthorization());

            return Task.Run(() =>
            {
                return response;
            });
                 
        }


        private T wrap<T>(Task<string> response, T builder) where T : IMessage
        {
            try
            {
                //if (response.Status != TaskStatus.RanToCompletion)
                //{
                //    throw new SystemException($"Error in Fank api call{response.Status}");
                //}
                var json = Util.NormalizeJson(response.Result);
                return (T)JsonParser.Default.Parse(json, builder.Descriptor);

            }
            catch (IOException ex)
            {
                throw new SystemException(ex.Message, ex);
            }
        }

    }
}
