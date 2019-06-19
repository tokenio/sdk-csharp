using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;


namespace TokenioTest.Asserts
{
    public sealed class TokenAssertion : Assert
    {
        private readonly Token actual;

        private TokenAssertion(Token actual)
            : base()
        {
            this.actual = actual;
        }

        public static TokenAssertion AssertThat(Token token)
        {
            return new TokenAssertion(token);
        }

        public TokenAssertion HasFrom(Member member)
        {
            Assert.Equal( member.MemberId(), actual.Payload.From.Id);
            return this;
        }

        public TokenAssertion HasRedeemerAlias(Alias alias)
        {
            Assert.Equal(actual.Payload.Transfer.Redeemer.Alias, alias);
            return this;
        }

        public TokenAssertion HasDescription(string description)
        {
            Assert.Equal(actual.Payload.Description, description);
            return this;
        }

        public TokenAssertion HasAmount(double amount)
        {
            Assert.Equal(actual.Payload.Transfer.LifetimeAmount, amount.ToString());
            return this;
        }

        public TokenAssertion HasCurrency(string currency)
        {
            Assert.Equal(actual.Payload.Transfer.Currency, currency);
            return this;
        }

        public TokenAssertion HasNSignatures(int count) 
        {
            Assert.Equal(actual.PayloadSignatures.Count, count);
            return this;
        }

        public TokenAssertion IsEndorsedBy(Member member, Key.Types.Level keyLevel)
        {
            IList<string> keyIdList = GetKeysForLevel(member, keyLevel);
            return HasKeySignatures(keyIdList, TokenSignature.Types.Action.Endorsed);
        }

        public TokenAssertion IsCancelledBy(Member member, Key.Types.Level keyLevel)
        {
            IList<string> keyIdList = GetKeysForLevel(member, keyLevel);
            return HasKeySignatures(keyIdList, TokenSignature.Types.Action.Cancelled);
        }

        public TokenAssertion HasNoSignatures()
        {
            Assert.Empty(actual.PayloadSignatures);
            return this;
        }

        private TokenAssertion HasKeySignatures(ICollection<string> keyIds, TokenSignature.Types.Action? action = null)
        {
            return HasKeySignatures(keyIds.ToArray(), action);
        }

        private TokenAssertion HasKeySignatures(String[] keyIds, TokenSignature.Types.Action? action = null)
        {
            IList<string> keyIdList = new List<string>();
            foreach(TokenSignature signature in actual.PayloadSignatures)
            {
                if(action == null || action == signature.Action)
                {
                    keyIdList.Add(signature.Signature.KeyId);
                }
            }

            foreach (var i in keyIds)
            {
                Assert.True(keyIdList.Contains(i));
            }
            //Assert.Equal(keyIdList, keyIds);
            return this;
        }

        private static IList<string> GetKeysForLevel(Member member, Key.Types.Level keyLevel)
        {
            IList<string> list = new List<string>();
            foreach (Key key in member.GetKeysBlocking() )
            {
                if (key.Level.Equals(keyLevel))
                {
                    list.Add(key.Id);
                }
            }
            return list;
        }

    }
}
