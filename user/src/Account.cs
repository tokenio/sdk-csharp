using System;
using System.Threading.Tasks;
using Tokenio.User.Rpc;
using Tokenio.Proto.Common.MoneyProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;


namespace Tokenio.User
{
    public class Account : Tokenio.Account
    {
        private readonly Member member;
        private readonly Client client;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="member">account owner</param>
        /// <param name="account">account information</param>

        internal Account(Member member, ProtoAccount account, Client client)
            : base(member, account, client)
        {
            this.client = client;
            this.member = member;
        }

        internal Account(Tokenio.Account account, Client client, Member member)
            : base(account)
        {
            this.client = client;
            this.member = member;
        }

        public override Tokenio.Member Member()
        {
            return member;
        }

        /// <summary>
        /// Sets this account as a member's default account.
        /// </summary>
        /// <returns>a Task</returns>
        public Task SetAsDefault()
        {
            return client.SetDefaultAccount(this.Id());
        }

        /// <summary>
        /// Sets this account as a member's default account.
        /// </summary>
        public void SetAsDefaultBlocking()
        {
            SetAsDefault().Wait();
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <returns>true if this account is default; false otherwise.</returns>
        public Task<bool> IsDefault()
        {
            return client.IsDefault(this.Id());
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <returns>true if this account is default; false otherwise.</returns>
        public bool IsDefaultBlocking()
        {
            return IsDefault().Result;
        }

        /// <summary>
        /// Looks up an account current balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the current balance</returns>
        [Obsolete("GetCurrentBalance is deprecated. Use GetBalance(keyLevel) instead")]
        public Task<Money> GetCurrentBalance(Level keyLevel)
        {
            return client.GetBalance(account.Id, keyLevel)
                .Map(balance => balance.Current);
        }

        /// <summary>
        /// Looks up an account current balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the current balance</returns>
        [Obsolete("GetCurrentBalanceBlocking is deprecated. Use GetBalanceBlocking(keyLevel).Current instead")]
        public Money GetCurrentBalanceBlocking(Level keyLevel)
        {
            return GetCurrentBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up an account available balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the available balance</returns>
        [Obsolete("GetAvailableBalance is deprecated. Use GetBalance(keyLevel) instead.")]
        public Task<Money> GetAvailableBalance(Level keyLevel)
        {
            return client.GetBalance(account.Id, keyLevel)
                .Map(balance => balance.Available);
        }

        /// <summary>
        /// Looks up an account available balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the available balance</returns>
        [Obsolete("GetAvailableBalanceBlocking is deprecated. Use GetBalanceBlocking(keyLevel).Available instead.")]
        public Money GetAvailableBalanceBlocking(Level keyLevel)
        {
            return GetAvailableBalance(keyLevel).Result;
        }
    }
}
