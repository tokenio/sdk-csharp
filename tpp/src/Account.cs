using System;

namespace Tokenio.Tpp
{
    public class Account : Tokenio.Account
    {
        private readonly Member member;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="member">account owner</param>
        /// <param name="account">account information</param>

        internal Account(Member member, Tokenio.Account account)
            : base(account)
        {
            this.member = member;
        }

        public Member Member()
        {
            return member;
        }
    }
}