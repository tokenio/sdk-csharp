using System;
using System.Threading.Tasks;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.User.Rpc;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;

namespace Tokenio.User
{
    /// <summary>
    /// Represents a funding account in the Token system.
    /// </summary>
    public class Account : Tokenio.Account
    {
        private readonly Member member;
        private readonly Client client;

        internal Account(Member member, ProtoAccount account, Client client) : base(member, account, client)
        {
            this.client = client;
            this.member = member;
        }

        internal Account(Tokenio.Account account, Client client, Member member) : base(account)
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
        /// <returns>taskk</returns>
        public async Task SetAsDefault()
        {
            await client.SetDefaultAccount(this.Id());
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
        public async Task<bool> IsDefault()
        {
            return await client.IsDefault(this.Id());
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <returns>true if this account is default; false otherwise.</returns>
        public bool IsDefaultBlocking()
        {
            return IsDefault().Result;
        }
    }
}
