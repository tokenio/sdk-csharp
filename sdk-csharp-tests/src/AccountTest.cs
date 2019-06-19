using System;
using Xunit;
using Tokenio.User;
using TokenioTest.Common;
using System.Collections.Generic;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using System.Linq;
using AccountCase = Tokenio.Proto.Common.AccountProtos.BankAccount.AccountOneofCase;
using StatusCode = Grpc.Core.StatusCode;
using Grpc.Core;


namespace TokenioTest
{

    public abstract class AccountTestBase : IDisposable
    {
        public TokenUserRule rule = new TokenUserRule();

        internal Member preMember;
        internal LinkedAccount preAccount1;
        internal LinkedAccount preAccount2;

        protected AccountTestBase()
        {
            preMember = rule.Member();
            preAccount1 = rule.LinkedAccount(preMember);
            preAccount2 = rule.LinkedAccount(preMember);
        }

        public void Dispose()
        {
            // Do "global" teardown here; Called after every test method.
        }
    }


    public class AccountTest : AccountTestBase
    {



        [Fact]
        public void LinkAccounts()
        {
            LinkedAccount account = rule.LinkedAccount();
            Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
            IList<string> list = new List<string>();
            list.Add(account.GetId());
            account.GetMember().UnlinkAccountsBlocking(list);
            Assert.Empty(account.GetMember().GetAccountsBlocking());
        }

        [Fact]
        public void LinkAccounts_Relinking()
        {
            LinkedAccount account = rule.LinkedAccount();
            Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
            rule.RelinkAccount(account);
            rule.RelinkAccount(account);
            Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
        }

        [Fact]
        public void GetAccounts()
        {
            LinkedAccount account = rule.LinkedAccount();
            Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
            IList<Account> accounts = account.GetMember().GetAccountsBlocking();
            Assert.Contains(account.GetAccount(), accounts);
        }

        [Fact]
        public void GetAccountsMultiple()
        {
            IList<Account> accounts = preAccount1.GetMember().GetAccountsBlocking();
            Assert.Contains(preAccount1.GetAccount(), accounts);
            Assert.Contains(preAccount2.GetAccount(), accounts);
        }

        [Fact]
        public void GetAccount()
        {
            LinkedAccount account = rule.LinkedAccount();
            Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
            Assert.Equal(account.GetMember().GetAccountBlocking(account.GetId()), account.GetAccount());
        }

        [Fact]
        public void GetBalance()
        {
            bool isNan = !Double.IsNaN(preAccount1.GetCurrentBalance(Level.Standard));
            Assert.True(isNan);
        }

        [Fact]
        public void SetAsDefault_Verify()
        {
            preAccount1.GetAccount().SetAsDefaultBlocking();
            Assert.True(preMember.GetDefaultAccountBlocking().Id().Equals(preAccount1.GetAccount().Id()));
            Assert.False(preMember.GetDefaultAccountBlocking().Id().Equals(preAccount2.GetAccount().Id()));
            preAccount2.GetAccount().SetAsDefaultBlocking();
            Assert.True(preMember.GetDefaultAccountBlocking().Id().Equals(preAccount2.GetAccount().Id()));
            Assert.False(preMember.GetDefaultAccountBlocking().Id().Equals(preAccount1.GetAccount().Id()));
        }

        [Fact]
        public void SetAsDefault_OnlyOneAfterUnlinking()
        {
            preAccount2.GetAccount().SetAsDefaultBlocking();
            Assert.True(preAccount2.GetAccount().IsDefaultBlocking());
            Assert.False(preAccount1.GetAccount().IsDefaultBlocking());

            string[] strs = new string[1];
            strs[0] = preAccount2.GetAccount().Id();
            preMember.UnlinkAccountsBlocking(strs.ToList());

            Assert.False(preAccount2.GetAccount().IsDefaultBlocking());
            Assert.True(preAccount1.GetAccount().IsDefaultBlocking());
            Assert.False(preMember.GetDefaultAccountBlocking()
                .Id()
                .Equals(preAccount2.GetAccount().Id()));
        }

        [Fact]

        public void ResolveTransferDestinations()
        {
            IList<TransferDestination> destinations = preMember
                    .ResolveTransferDestinationsBlocking(preAccount1.GetId());


            Assert.NotEmpty(destinations);
            destinations.ToList().ForEach(destination =>
            Assert.NotEqual(TransferDestination.DestinationOneofCase.Token, destination.DestinationCase ));
        }

        [Fact]
        public void ResolveTransferDestinations_Unauthenticated()
        {
            AggregateException ex = Assert.Throws<AggregateException>(() =>
                rule.Member()
               .ResolveTransferDestinationsBlocking(preAccount1.GetId()));
            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public void ResolveTransferDestinations_InactiveAccount()
        {
            string accountId = preAccount2.GetAccount().Id();
            IList<string> list = new List<string>
            {
                accountId
            };
            preMember.UnlinkAccountsBlocking(list);

            AggregateException ex =  Assert.Throws<AggregateException>(() =>
                preMember
                .ResolveTransferDestinationsBlocking(preAccount2.GetId()));
            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        }
    }
}
