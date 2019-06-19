using System;
using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.User;
using TokenioTest.Bank;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Account = Tokenio.User.Account;
using Member = Tokenio.User.Member;

namespace TokenioTest.Common
{
    public class LinkedAccount
    {
        private readonly TestAccount testAccount;
        private readonly Account account;


        public LinkedAccount(TestAccount testAccount, Account account)
        {
            this.testAccount = testAccount;
            this.account = account;
        }

        public string GetId()
        {
            return account.Id();
        }

        public Account GetAccount()
        {
            return account;
        }

        public Member GetMember()
        {
            return (Member)GetAccount().Member();
        }

        public string GetCurrency()
        {
            return testAccount.GetCurrency();
        }

        public TestAccount TestAccount()
        {
            return testAccount;
        }

        public TransferTokenBuilder TransferTokenBuilder(double amount, LinkedAccount destination)
        {
            return GetMember().CreateTransferToken(amount, GetCurrency())
                .SetToMemberId(destination.GetMember().MemberId())
                .SetToAlias(destination.GetMember().GetFirstAliasBlocking())
                .AddDestination(TokenDestination(destination.GetMember().MemberId(), destination.GetId()))
                .SetAccountId(GetId());
        }

        public TransferTokenBuilder TransferTokenBuilder(double amount, Tokenio.Tpp.Account destination, Tokenio.Tpp.Member destinationMember)
        {
            return GetMember().CreateTransferToken(amount, GetCurrency())
                .SetToMemberId(destinationMember.MemberId())
                .SetToAlias(destinationMember.GetFirstAliasBlocking())
                .AddDestination(TokenDestination(
                    destinationMember.MemberId(),
                    destination.Id()))
                .SetAccountId(GetId());
        }

        public Token CreateTransferToken(double amount, LinkedAccount destination)
        {
            TokenPayload payload = GetMember()
                                        .PrepareTransferTokenBlocking(TransferTokenBuilder(amount, destination))
                                        .TokenPayload;
            return GetMember().CreateTokenBlocking(payload, Level.Standard);
        }


        public Token CreateTransferToken(double amount, Tokenio.Tpp.Account destination, Tokenio.Tpp.Member destinationMember )
        {
            TransferTokenBuilder builder = TransferTokenBuilder(amount, destination, destinationMember);
            TokenPayload payload = GetMember().PrepareTransferTokenBlocking(builder).TokenPayload;
            return GetMember().CreateTokenBlocking(payload, Level.Standard);
        }


        public Token CreateTransferTokenWithTokenDestination(double amount, Tokenio.Tpp.Member payee)
        {
            TransferTokenBuilder builder = GetMember().CreateTransferToken(amount, GetCurrency())
                                                    .SetToMemberId(payee.MemberId())
                                                    .SetToAlias(payee.GetFirstAliasBlocking())
                                                    .AddDestination(TokenDestination(payee.MemberId()))
                                                    .SetAccountId(GetId());

            TokenPayload payload = GetMember().PrepareTransferTokenBlocking(builder).TokenPayload;
            return GetMember().CreateTokenBlocking(payload, Level.Standard);

        }

        public Token CreateTransferTokenWithSepaDestination(double amount, Tokenio.Tpp.Member payee)
        {
            TransferTokenBuilder builder = GetMember().CreateTransferToken(amount, GetCurrency())
                                                        .SetToMemberId(payee.MemberId())
                                                        .SetToAlias(payee.GetFirstAliasBlocking())
                                                        .AddDestination(new TransferDestination
                                                        {
                                                            Sepa = new TransferDestination.Types.Sepa
                                                            {
                                                                Bic = "DABAIE2D",
                                                                Iban = "DE89370400440532013000"
                                                            }
                                                        })
                                                        .SetAccountId(GetId());
            TokenPayload payload = GetMember().PrepareTransferTokenBlocking(builder).TokenPayload;
            return GetMember().CreateTokenBlocking(payload, Level.Standard);

        }

        public Token CreateTransferTokenUnlinkedDestination(double amount)
        {
            TransferTokenBuilder builder = GetMember().CreateTransferToken(amount, GetCurrency())
                                            .AddDestination(new TransferDestination
                                            {
                                                Sepa = new TransferDestination.Types.Sepa
                                                {
                                                    Bic = "DABAIE2D",
                                                    Iban = "DE89370400440532013000"
                                                }
                                            })
                                            .SetAccountId(GetId());
            TokenPayload payload = GetMember().PrepareTransferTokenBlocking(builder).TokenPayload;
            return GetMember().CreateTokenBlocking(payload, Level.Standard);
        }

        public double GetCurrentBalance(Level keyLevel)
        {
            Money balance = account.GetCurrentBalanceBlocking(keyLevel);
            Assert.Equal(balance.Currency, GetCurrency());
            return Double.Parse(balance.Value);
        }

        public double GetAvailableBalance(Level keyLevel)
        {
            Money balance = account.GetAvailableBalanceBlocking(keyLevel);
            Assert.Equal(balance.Currency,GetCurrency());
            return Double.Parse(balance.Value);
        }

        public Transaction GetTransaction(string transactionId, Level keyLevel)
        {
            Transaction transaction = account.GetTransactionBlocking(transactionId, keyLevel);
            Assert.Equal(transaction.Id, transactionId);
            Assert.Equal(transaction.Amount.Currency, GetCurrency());
            return transaction;
        }




        public PagedList<Transaction> GetTransactions(int limit, Level keyLevel, string offset = null)
        {
            return GetTransactions(limit, keyLevel, true, offset);
        }

        public PagedList<Transaction> GetTransactions(int limit, Level keyLevel, bool checkCurrency, string offset = null)
        {
            PagedList<Transaction> transactions = account.GetTransactionsBlocking(
                                                        offset,
                                                        limit,
                                                        keyLevel);


            if (checkCurrency) {
                foreach (Transaction t in transactions.List)
                {
                    Assert.Equal(t.Amount.Currency, GetCurrency());
                }
            }                                                                                        

            return transactions;
        }


      
        private TransferDestination TokenDestination(string memberId)
        {
            return new TransferDestination
            {
                Token = new TransferDestination.Types.Token
                {
                    MemberId = memberId
                }
            };
        }

        private TransferDestination TokenDestination(string memberId, string accountId)
        {
            return new TransferDestination
            {
                Token = new TransferDestination.Types.Token
                {
                    MemberId = memberId,
                    AccountId = accountId
                }
            };
        }


    }
}
