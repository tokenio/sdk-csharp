using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.TokenRequests;
using Tokenio.Tpp.TokenRequests;
using Tokenio.Utils;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Account = Tokenio.Account;
using ProtoToken = Tokenio.Proto.Common.TokenProtos.TokenRequest;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;


namespace TokenioSample
{
    public class CancelAccessTokenSampleTest
    {
        /// <summary>
        /// Cancels the access token by grantee test.
        /// </summary>
        [Fact]
        public void CancelAccessTokenByGranteeTest()
        {

           using  (TokenClient tokenClient = TestUtil.CreateClient()) {
                Tokenio.User.Member grantor = TestUtil.CreateUserMember();
                string accountId = grantor.GetAccountsBlocking()[0].Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = TestUtil.CreateAccessToken(grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelAccessTokenSample.CancelAccessToken(grantee, token.Id);
                Assert.Equal(result.Status,TokenOperationResult.Types.Status.Success);

            }
        }


    }
}

