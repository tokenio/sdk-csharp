using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.BankProtos;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Rpc;
using Tokenio.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio
{
    /// <summary>
    /// Represents a Member in the Token system. Each member has an active secret.
    /// and public key pair that is used to perform authentication.
    /// </summary>
    public class Member
    {

        protected readonly string memberId;
        private readonly Client client;
        protected readonly TokenCluster tokenCluster;
        protected readonly string partnerId;
        protected readonly string realmId;


        /// <summary>
        /// Creates an instance of <see cref="Member"/>
        /// </summary>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="client">RPC client used to perform operations against the server</param>
        /// <param name="tokenCluster">Token cluster, e.g. sandbox, production</param>
        /// <param name="partnerId">member ID of the partner, if applicable</param>
        /// <param name="realmId">the realm id</param>

        public Member(string memberId,
            Client client,
            TokenCluster tokenCluster,
            string partnerId,
            string realmId)
        {
            this.memberId = memberId;
            this.client = client;
            this.tokenCluster = tokenCluster;
            this.partnerId = partnerId;
            this.realmId = realmId;
        }

        /// <summary>
        /// Gets the member id.
        /// </summary>
        /// <returns>a unique ID that identifies the member in the Token system</returns>
        public string MemberId()
        {
            return memberId;
        }

        /// <summary>
        /// Partners the identifier.
        /// </summary>
        /// <returns>member ID</returns>
        public string PartnerId()
        {
            return partnerId;
        }

        /// <summary>
        /// Gets member ID of realm owner.
        /// </summary>
        /// <returns> owner member ID</returns>
        public string RealmId()
        {
            return realmId;
        }

        /// <summary>
        /// Gets the token cluster.
        /// </summary>
        /// <returns>The token cluster.</returns>
        public TokenCluster GetTokenCluster()
        {

            return tokenCluster;
        }

        /// <summary>
        /// Gets the last hash.
        /// </summary>
        /// <returns>the last hash</returns>
        public async Task<string> GetLastHash()
        {
            return await client
                .GetMember()
                .Map(m => m.LastHash);
        }

        /// <summary>
        /// Gets the last hash.
        /// </summary>
        /// <returns>the last hash</returns>
        public string GetLastHashBlocking()
        {
            return GetLastHash().Result;
        }

        /// <summary>
        /// Gets all aliases owned by the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public async Task<IList<Alias>> GetAliases()
        {
            return await client.GetAliases();
        }

        /// <summary>
        /// Gets all aliases owned by the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public IList<Alias> GetAliasesBlocking()
        {
            return GetAliases().Result;
        }

        /// <summary>
        /// Gets the first alias owner by the user.
        /// </summary>
        /// <returns>first alias owned by the user, or throws exception if no aliases are found</returns>
        public async Task<Alias> GetFirstAlias()
        {
            return await GetAliases().Map(aliases => aliases.Count > 0 ? aliases[0] : throw new NoAliasesFoundException(MemberId()));
        }

        /// <summary>
        /// Gets the fisrt alias owned by the user.
        /// </summary>
        /// <returns>first alias owned by the user, or throws exception if not aliases are found</returns>
        public Alias GetFirstAliasBlocking()
        {
            return GetFirstAlias().Result;
        }

        /// <summary>
        /// Gets all public keys for this member.
        /// </summary>
        /// <returns>list of public keys that are approved for this member</returns>
        public async Task<IList<Key>> GetKeys()
        {
            return await client
                .GetMember()
                .Map(member => (IList<Key>)member.Keys.ToList());
        }

        /// <summary>
        /// Gets all public keys for this member.
        /// </summary>
        /// <returns>list of public keys that are approved for this member</returns>
        public IList<Key> GetKeysBlocking()
        {
            return GetKeys().Result;
        }

        /// <summary>
        /// Links a funding bank account to Token and returns it to the caller.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public async Task<IList<Account>> GetAccountsImpl()
        {
            return await client
                .GetAccounts()
                .Map(accounts => (IList<Account>)accounts
                    .Select(account => new Account(this, account, client))
                    .ToList());
        }

        public IList<Account> GetAccountsImplBlocking()
        {
            var acc = GetAccountsImpl().Result;
            return acc;

        }


        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public async Task<Account> GetAccountImpl(string accountId)
        {
            return await client
                .GetAccount(accountId)
                .Map(account => new Account(this, account, client));
        }

        /// <summary>
        /// Adds new aliases for the member.
        /// </summary>
        /// <param name="aliases">aliases, e.g. 'john', must be unique</param>
        /// <returns>a task that indicates whether the operation finished or had an error</returns>
        public async Task AddAliases(IList<Alias> aliases)
        {
            aliases = aliases.Select(alias =>
            {
                if (!string.IsNullOrEmpty(partnerId) && !partnerId.Equals("token"))
                {
                        // Realm must equal member's partner ID if affiliated
                        if (!string.IsNullOrEmpty(alias.Realm) && !alias.Realm.Equals(partnerId))
                    {
                        throw new InvalidRealmException(alias.Realm, partnerId);
                    }
                    alias.Realm = partnerId;
                }
                if (!string.IsNullOrEmpty(realmId))
                {

                    alias.Realm = realmId;
                }
                return alias;
            }).ToList();
            var operations = aliases.Select(Util.ToAddAliasOperation).ToList();
            var metadata = aliases.Select(Util.ToAddAliasMetadata).ToList();
            await client.UpdateMember(operations, metadata);
        }

        /// <summary>
        /// Adds new aliases for the member.
        /// </summary>
        /// <param name="aliases">aliases, e.g. 'john', must be unique</param>
        public void AddAliasesBlocking(IList<Alias> aliases)
        {
            AddAliases(aliases).Wait();
        }

        /// <summary>
        /// Adds a new alias for the member.
        /// </summary>
        /// <param name="alias">the alias</param>
        /// <returns>a task</returns>
        public async Task AddAlias(Alias alias)
        {
            await AddAliases(new List<Alias> { alias });
        }

        /// <summary>
        /// Adds a new alias for the member.
        /// </summary>
        /// <param name="alias">the alias</param>
        /// <returns>a task</returns>
        public void AddAliasBlocking(Alias alias)
        {
            AddAlias(alias).Wait();
        }

        /// <summary>
        /// Retries alias verification.
        /// </summary>
        /// <param name="alias">the alias to be verified</param>
        /// <returns>the verification id</returns>
        public async Task<string> RetryVerification(Alias alias)
        {
            return await client.RetryVerification(alias);
        }

        /// <summary>
        /// Retries alias verification.
        /// </summary>
        /// <param name="alias">the alias to be verified</param>
        /// <returns>the verification id</returns>
        public string RetryVerificationBlocking(Alias alias)
        {
            return RetryVerification(alias).Result;
        }

        /// <summary>
        /// Adds the recovery rule.
        /// </summary>
        /// <param name="rule">the recovery rule</param>
        /// <returns>the updated member</returns>
        public async Task<ProtoMember> AddRecoveryRule(RecoveryRule rule)
        {
            return await client.UpdateMember(new List<MemberOperation>
                    {
                        new MemberOperation
                        {
                            RecoveryRules = new MemberRecoveryRulesOperation
                            {
                                RecoveryRule = rule
                            }
                        }
                    });
        }

        /// <summary>
        /// Adds the recovery rule.
        /// </summary>
        /// <param name="rule">the recovery rule</param>
        /// <returns>the updated member</returns>
        public ProtoMember AddRecoveryRuleBlocking(RecoveryRule rule)
        {
            return AddRecoveryRule(rule).Result;
        }

        /// <summary>
        /// Set Token as the recovery agent.
        /// </summary>
        /// <returns>a task</returns>
        public async Task UseDefaultRecoveryRule()
        {
            await client.UseDefaultRecoveryRule();
        }

        /// <summary>
        /// Set Token as the recovery agent.
        /// </summary>
        /// <returns>a task</returns>
        public void UseDefaultRecoveryRuleBlocking()
        {
            UseDefaultRecoveryRule().Wait();
        }

        /// <summary>
        /// Authorizes recovery as a trusted agent.
        /// </summary>
        /// <param name="authorization">the authorization</param>
        /// <returns>the signature</returns>
        public async Task<Signature> AuthorizeRecovery(Authorization authorization)
        {
            return await client.AuthorizeRecovery(authorization);
        }

        /// <summary>
        /// Authorizes recovery as a trusted agent.
        /// </summary>
        /// <param name="authorization">the authorization</param>
        /// <returns>the signature</returns>
        public Signature AuthorizeRecoveryBlocking(Authorization authorization)
        {
            return AuthorizeRecovery(authorization).Result;
        }

        /// <summary>
        /// Gets the member id of the default recovery agent.
        /// </summary>
        /// <returns>the member id</returns>
        public async Task<string> GetDefaultAgent()
        {
            return await client.GetDefaultAgent();
        }

        /// <summary>
        /// Gets the member id of the default recovery agent.
        /// </summary>
        /// <returns>the member id</returns>
        public string GetDefaultAgentBlocking()
        {
            return GetDefaultAgent().Result;
        }

        /// <summary>
        /// Verifies a given alias.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <returns>a task</returns>
        public async Task VerifyAlias(string verificationId, string code)
        {
            await client.VerifyAlias(verificationId, code);
        }

        /// <summary>
        /// Verifies a given alias.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <returns>a task</returns>
        public void VerifyAliasBlocking(string verificationId, string code)
        {
            VerifyAlias(verificationId, code).Wait();
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="aliases">the aliases to remove</param>
        /// <returns>a task</returns>
        public async Task RemoveAliases(IList<Alias> aliases)
        {
            var operations = aliases.Select(Util.ToRemoveAliasOperation).ToList();
            await client.UpdateMember(operations);
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="aliases">the aliases to remove</param>
        /// <returns>a task</returns>
        public void RemoveAliasesBlocking(IList<Alias> aliases)
        {
            RemoveAliases(aliases).Wait();
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="alias">the alias to remove</param>
        /// <returns>a task</returns>
        public async Task RemoveAlias(Alias alias)
        {
            await RemoveAliases(new List<Alias> { alias });
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="alias">the alias to remove</param>
        /// <returns>a task</returns>
        public void RemoveAliasBlocking(Alias alias)
        {
            RemoveAlias(alias).Wait();
        }

        /// <summary>
        /// Approves public keys owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keys">the keys to add</param>
        /// <returns>a task</returns>
        public async Task ApproveKeys(IList<Key> keys)
        {
            var operations = keys.Select(Util.ToAddKeyOperation).ToList();
            await client.UpdateMember(operations);
        }

        /// <summary>
        /// Approves public keys owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keys">the keys to add</param>
        public void ApproveKeysBlocking(IList<Key> keys)
        {
            ApproveKeys(keys).Wait();
        }

        /// <summary>
        /// Approves a public key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="key">the key to add</param>
        /// <returns>a task that indicates whether the operation finished or had an error</returns>
        public async Task ApproveKey(Key key)
        { 
            await ApproveKeys(new List<Key> { key });
        }

        /// <summary>
        /// Approves a public key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="key">the key to add</param>
        /// <returns>a task that indicates whether the operation finished or had an error</returns>
        public void ApproveKeyBlocking(Key key)
        {
            ApproveKey(key).Wait();
        }

        /// <summary>
        /// Approves a key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keyPair">the keypair to add</param>
        /// <returns>a task</returns>
        public async Task ApproveKey(KeyPair keyPair)
        {
            await ApproveKey(keyPair.ToKey());
        }

        /// <summary>
        /// Approves a key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keyPair">the keypair to add</param>
        /// <returns>a task that indicates whether the operation finished or had an error</returns>
        public void ApproveKeyBlocking(KeyPair keyPair)
        {
            ApproveKey(keyPair).Wait();
        }

        /// <summary>
        /// Removes some public keys owned by this member.
        /// </summary>
        /// <param name="keyIds">the IDs of the keys to remove</param>
        /// <returns>a task</returns>
        public async Task RemoveKeys(IList<string> keyIds)
        {
            var operations = keyIds.Select(Util.ToRemoveKeyOperation).ToList(); 
            await client.UpdateMember(operations);
        }

        /// <summary>
        /// Removes some public keys owned by this member.
        /// </summary>
        /// <param name="keyIds">the IDs of the keys to remove</param>
        /// <returns>a task</returns>
        public void RemoveKeysBlocking(IList<string> keyIds)
        {
            RemoveKeys(keyIds).Wait();
        }

        /// <summary>
        /// Removes a public key owned by this member.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>a task</returns>
        public async Task RemoveKey(string keyId)
        {
            await RemoveKeys(new List<string> { keyId });
        }

        /// <summary>
        /// Removes a public key owned by this member.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>a task</returns>
        public void RemoveKeyBlocking(string keyId)
        {
            RemoveKey(keyId).Wait();
        }

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        public async Task<Transaction> GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel)
        {
            return await client.GetTransaction(accountId, transactionId, keyLevel);
        }

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        public Transaction GetTransactionBlocking(
            string accountId,
            string transactionId,
            Level keyLevel)
        {
            return GetTransaction(accountId, transactionId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        public async Task<PagedList<Transaction>> GetTransactions(
            string accountId,
            int limit,
            Level keyLevel,
            string offset = null)
        {
            return await client.GetTransactions(accountId, limit, keyLevel, offset);
        }

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        public PagedList<Transaction> GetTransactionsBlocking(
            string accountId,
            int limit,
            Level keyLevel,
            string offset = null)
        {
            return GetTransactions(accountId, limit, keyLevel, offset).Result;
        }
        
        /// <summary>
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        public async Task<StandingOrder> GetStandingOrder(
            string accountId,
            string standingOrderId,
            Level keyLevel)
        {
            return await client.GetStandingOrder(accountId, standingOrderId, keyLevel);
        }

        /// <summary>
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        public StandingOrder GetStandingOrderBlocking(
                string accountId,
                string standingOrderId,
                Level keyLevel)
        {
            return GetStandingOrder(accountId, standingOrderId, keyLevel).Result;
        }
        
        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        public async Task<PagedList<StandingOrder>> GetStandingOrders(
                string accountId,
                int limit,
                Level keyLevel,
                string offset = null)
        {
            return await client.GetStandingOrders(accountId, limit, keyLevel, offset);
        }

        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        public PagedList<StandingOrder> GetStandingOrdersBlocking(
                string accountId,
                int limit,
                Level keyLevel,
                string offset = null)
        {
            return GetStandingOrders(accountId, limit, keyLevel, offset).Result;
        }

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public async Task<Balance> GetBalance(string accountId, Level keyLevel)
        {
            return await client.GetBalance(accountId, keyLevel);
        }

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Balance GetBalanceBlocking(string accountId, Level keyLevel)
        {
            return GetBalance(accountId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        public async Task<IList<Balance>> GetBalances(IList<string> accountIds, Level keyLevel)
        {
            return await client.GetBalances(accountIds, keyLevel);
        }

        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        public IList<Balance> GetBalancesBlocking(IList<string> accountIds, Level keyLevel)
        {
            return GetBalances(accountIds, keyLevel).Result;
        }

        /// <summary>
        /// Confirms the funds.
        /// </summary>
        /// <returns>The funds.</returns>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="currency">Currency.</param>
        public async Task<bool> ConfirmFunds(string accountId, double amount, string currency)
        {
            var money = new Money
            {
                Currency = currency,
                Value = amount.ToString()
            };
            return await client.ConfirmFunds(accountId, money);
        }

        /// <summary>
        /// Confirm that the given account has sufficient funds to cover the charge
        /// </summary>
        /// <returns>true if the account has sufficient funds to cover the charge.</returns>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="currency">Currency.</param>
        public bool ConfirmFundsBlocking(string accountId, double amount, string currency)
        {
            return ConfirmFunds(accountId, amount, currency).Result;
        }

        /// <summary>
        /// Returns linking information for a specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public async Task<BankInfo> GetBankInfo(string bankId)
        {
            return await client.GetBankInfo(bankId);
        }

        /// <summary>
        /// Returns linking information for a specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public BankInfo GetBankInfoBlocking(string bankId)
        {
            return GetBankInfo(bankId).Result;
        }

        /// <summary>
        /// Deletes the member
        /// </summary>
        /// <returns>Task</returns>
        public async Task DeleteMember()
        {
            await client.DeleteMember();
        }

        /// <summary>
        /// Deletes the member
        /// </summary>
        /// <returns>Task</returns>
        public void DeleteMemberBlocking()
        {
            DeleteMember().Wait();
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public async Task<IList<TransferDestination>> ResolveTransferDestinations(string accountId)
        {
            return await client.ResolveTransferDestination(accountId);
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public IList<TransferDestination> ResolveTransferDestinationsBlocking(string accountId)
        {
            return ResolveTransferDestinations(accountId).Result;
        }

        public Signature SignTokenPayload(TokenPayload payload, Level keyLevel)
        {
            return client.SignTokenPayload(payload, keyLevel);
        }

        /// <summary>
        /// Gets a member's public profile.
        /// </summary>
        /// <param name="memberId">member ID of member whose profile we want</param>
        /// <returns>their profile</returns>
        public async Task<Profile> GetProfile(string memberId)
        {
            return await client.GetProfile(memberId);
        }

        /// <summary>
        /// Gets a member's public profile.
        /// </summary>
        /// <param name="memberId">member ID of member whose profile we want</param>
        /// <returns>their profile</returns>
        public Profile GetProfileBlocking(string memberId)
        {
            return GetProfile(memberId).Result;
        }

        /// <summary>
        /// Gets a member's public profile picture.
        /// </summary>
        /// <param name="memberId">member ID of member whose profile we want</param>
        /// <param name="size">desired size category (small, medium, large, original)</param>
        /// <returns>blob with picture; empty blob (no fields set) if has no picture</returns>
        public async Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            return await client.GetProfilePicture(memberId, size);
        }

        /// <summary>
        /// Gets a member's public profile picture.
        /// </summary>
        /// <param name="memberId">member ID of member whose profile we want</param>
        /// <param name="size">desired size category (small, medium, large, original)</param>
        /// <returns>blob with picture; empty blob (no fields set) if has no picture</returns>
        public Blob GetProfilePictureBlocking(string memberId, ProfilePictureSize size)
        {
            return GetProfilePicture(memberId, size).Result;
        }

        /// <summary>
        /// Sets security metadata included in all requests
        /// </summary>
        /// <param name="metaData">security metadata</param>
        /// TODO: RD-2335: Change class from SecurityMetaData to TrackingMetaData
        public void SetTrackingMetaData(SecurityMetadata metaData)
        {
            client.SetTrackingMetadata(metaData);
        }

        /// <summary>
        /// Clears the security metadata
        /// </summary>
        public void ClearTrackingMetaData()
        {
            client.ClearTrackingMetaData();
        }

        /// <summary>
        /// Creates a test bank account in a fake bank and links the account.
        /// </summary>
        /// <param name="balance">account balance to set</param>
        /// <param name="currency">currency code, e.g. "EUR"</param>
        /// <returns>the linked account</returns>
        protected Task<Account> CreateTestBankAccountImpl(
            double balance,
            string currency)
        {
            var money = new Money
            {
                Value = balance.ToString(),
                Currency = currency

            };
            return ToAccount(client.CreateAndLinkTestBankAccount(money));

        }

        private Task<Account> ToAccount(Task<ProtoAccount> account)
        {
            return account.Map(acc => new Account(this, acc, client));
        }
    }
}
