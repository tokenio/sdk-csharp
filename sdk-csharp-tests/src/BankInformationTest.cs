using System;
using Tokenio;
using TokenioTest.Common;
using Xunit;

namespace TokenioTest
{
    public abstract class BankInformationTestBase : IDisposable
    {
        public TokenUserRule rule = new TokenUserRule();
        internal Member member;

        protected BankInformationTestBase()
        {
            this.member = rule.Member();
        }

        public void Dispose()
        {

        }
    }

    public class BankInformationTest : BankInformationTestBase
    {
        [Fact]
        public void GetBanks()
        {
            Assert.NotEmpty(rule.GetBanks());
        }

        [Fact]
        public void GetBankInfo()
        {
            Assert.NotNull(member.GetBankInfoBlocking(rule.GetBankId()));
        }

        [Fact]
        public void DontGetBankInfo()
        {
            Assert.Throws<AggregateException>(() => member.GetBankInfoBlocking("nonexistent"));
        }


    }
}
