using System;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using TokenioTest.Common;
using Xunit;
using Level = Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using PurposeOfPayment = Tokenio.Proto.Common.TransferInstructionsProtos.PurposeOfPayment;
using Sample = TokenioTest.Testing.Sample.Sample;

namespace TokenioTest
{
	public abstract class TransferTokenBuilderTestBase : IDisposable
	{
		internal LinkedAccount payerAccount;
		internal Member payer;
		internal LinkedAccount payeeAccount;
		internal Member payee;
		public TokenUserRule rule = new TokenUserRule();

		protected TransferTokenBuilderTestBase()
		{
			payerAccount = rule.LinkedAccount();
			payer = payerAccount.GetMember();
			payeeAccount = rule.LinkedAccount(payerAccount);
			payee = payeeAccount.GetMember();
		}

		public void Dispose()
		{
		}
	}

	public class TransferTokenBuilderTest : TransferTokenBuilderTestBase
	{
		[Fact]
		public void BasicToken()
		{
			TokenPayload payload = payer.PrepareTransferTokenBlocking(
					payerAccount.TransferTokenBuilder(100.0, payeeAccount)
							.SetDescription("book purchase"))
					.TokenPayload;
			payer.CreateTokenBlocking(payload, Level.Standard);
		}

		[Fact]
		[Obsolete ("This test is deprecated.")]
		public void NoSource()
		{
			Assert.Throws<Exception>(() => payer.CreateTransferToken(100.0, payerAccount.GetCurrency())
					.SetToMemberId(payee.MemberId())
					.SetDescription("book purchase")
					.ExecuteBlocking());
		}

		[Fact]
		public void Full()
		{
			TokenPayload payload = payer.PrepareTransferTokenBlocking(
					payerAccount.TransferTokenBuilder(100.0, payeeAccount)
							.SetEffectiveAtMs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000)
							.SetExpiresAtMs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1000000)
							.SetChargeAmount(40)
							.SetDescription("book purchase")
							.SetRefId(Sample.RandomNumeric(15))
							.SetPurposeOfPayment(PurposeOfPayment.Savings))
					.TokenPayload;
			Token token = payer.CreateTokenBlocking(payload, Level.Standard);
			Assert.Equal(PurposeOfPayment.Savings, token.Payload
					.Transfer
					.Instructions
					.Metadata
					.TransferPurpose);
		}
	}
}