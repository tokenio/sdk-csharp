using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using Tokenio.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public abstract class TestUtil
    {
        private static string DEV_KEY = "f3982819-5d8d-4123-9601-886df2780f42";
        private static string TOKEN_REALM = "token";

        /// <summary>
        /// Generates random user name to be used for testing.
        /// </summary>
        /// <returns>The alias.</returns>
        public static Alias RandomAlias()
        {
            return new Alias
            {
                Value = "alias-" + Util.Nonce().ToLower() + "+noverify@example.com",
                Type = Alias.Types.Type.Domain,
                Realm = TOKEN_REALM
            };
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <returns>The client.</returns>
        public static Tokenio.Tpp.TokenClient CreateClient()
        {
            return Tokenio.Tpp.TokenClient.Create(Tokenio.TokenCluster.DEVELOPMENT, DEV_KEY);
        }

        /// <summary>
        /// Creates the user member.
        /// </summary>
        /// <returns>The user member.</returns>
        public static UserMember CreateUserMember()
        {
            Tokenio.User.TokenClient
                client = Tokenio.User.TokenClient.Create(Tokenio.TokenCluster.DEVELOPMENT, DEV_KEY);
            Alias alias = RandomAlias();
            UserMember member = client.CreateMemberBlocking(alias);
            member.CreateTestBankAccountBlocking(1000.0, "EUR");
            return member;
        }

        public static string RandomNumeric(int size)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }
    }
}