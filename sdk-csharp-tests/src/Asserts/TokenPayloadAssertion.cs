using System;
using Tokenio;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;

namespace TokenioTest.Asserts
{
    public class TokenPayloadAssertion : Assert
    {
        private readonly TokenPayload actual;

        private TokenPayloadAssertion(TokenPayload actual) : base()
        {
            this.actual = actual;
        }

        public static TokenPayloadAssertion AssertThat(TokenPayload tokenPayload)
        {
            return new TokenPayloadAssertion(tokenPayload);
        }

        public TokenPayloadAssertion HasFrom(Member member)
        {
            Assert.Equal(member.MemberId(),actual.From.Id);
            return this;
        }

        public TokenPayloadAssertion HasDescription(string description)
        {
            Assert.Equal(description, actual.Description);
            return this;
        }

        public TokenPayloadAssertion HasAmount(double amount)
        {
            Assert.Equal(amount.ToString(), actual.Transfer.LifetimeAmount);
            return this;
        }

        public TokenPayloadAssertion HasCurrency(string currency)
        {
            Assert.Equal(currency, actual.Transfer.Currency);
            return this;
        }

    }
}
