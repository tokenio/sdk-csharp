using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio.User;
using TokenioTest.Common;
using Xunit;
using Level = Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace TokenioTest
{
	public abstract class LocalBankStorageTestBase : IDisposable
	{
		internal Member preMember;
		internal LinkedAccount preAccount1;
		internal LinkedAccount preAccount2;
		public TokenUserRule rule = new TokenUserRule("gold");

		protected LocalBankStorageTestBase()
		{
			preMember = rule.Member();
			preAccount1 = rule.LinkedAccount(preMember);
			preAccount2 = rule.LinkedAccount(preMember);
		}

		public void Dispose()
		{
		}
	}

	public class LocalBankStorageTest : LocalBankStorageTestBase
	{
		[Fact]
		public void LinkAccounts()
		{
			LinkedAccount account = rule.LinkedAccount();
			Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
			account.GetMember().UnlinkAccountsBlocking(new List<string>
			{
				account.GetId()
			});
			Assert.True(account.GetMember().GetAccountsBlocking().Count == 0);
		}

		[Fact]
		public void LinkAccounts_Relinking()
		{
			LinkedAccount account = rule.LinkedAccount();
			Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
			rule.RelinkAccount(account);
			rule.RelinkAccount(account);
			Assert.Contains(account.GetAccount(), account.GetMember().GetAccountsBlocking());
		}

		[Fact]
		public void GetAccounts()
		{
			LinkedAccount account = rule.LinkedAccount();
			Assert.True(account.GetMember().GetAccountsBlocking().Contains(account.GetAccount()));
			IList<Account> accounts = account.GetMember().GetAccountsBlocking();
			Assert.True(accounts.Contains(account.GetAccount()));
		}

		[Fact]
		public void GetAccountsMultiple()
		{
			IList<Account> accounts = preAccount1.GetMember().GetAccountsBlocking();
			Assert.True(accounts.Contains(preAccount1.GetAccount()));
			Assert.True(accounts.Contains(preAccount2.GetAccount()));
		}

		[Fact]
		public void GetAccount()
		{
			LinkedAccount account = rule.LinkedAccount();
			Assert.True(account.GetMember().GetAccountsBlocking().Contains(account.GetAccount()));
			Assert.Equal(account.GetMember().GetAccountBlocking(account.GetId()), account.GetAccount());
		}

		[Fact]
		public void GetBalance()
		{
			bool isNaN = !(Double.IsNaN(preAccount1.GetCurrentBalance(Level.Standard)));
			Assert.True(isNaN);
		}

		[Fact]
		public void SetAsDefault_verify()
		{
			preAccount1.GetAccount().SetAsDefaultBlocking();
			Assert.Equal(preMember.GetDefaultAccountBlocking().Id(), preAccount1.GetAccount().Id());
			Assert.NotEqual(preMember.GetDefaultAccountBlocking().Id(), preAccount2.GetAccount().Id());
			preAccount2.GetAccount().SetAsDefaultBlocking();
			Assert.Equal(preMember.GetDefaultAccountBlocking().Id(), preAccount2.GetAccount().Id());
			Assert.NotEqual(preMember.GetDefaultAccountBlocking().Id(), preAccount1.GetAccount().Id());
		}

		[Fact]
		public void SetAsDefault_onlyOneAfterUnlinking()
		{
			preAccount2.GetAccount().SetAsDefaultBlocking();
			Assert.True(preAccount2.GetAccount().IsDefaultBlocking());
			Assert.False(preAccount1.GetAccount().IsDefaultBlocking());
			string[] strs = new string[1];
			strs[0] = preAccount2.GetAccount().Id();
			preMember.UnlinkAccountsBlocking(strs.ToList());
			Assert.False(preAccount2.GetAccount().IsDefaultBlocking());
			Assert.True(preAccount1.GetAccount().IsDefaultBlocking());
			Assert.NotEqual(preMember.GetDefaultAccountBlocking().Id(), preAccount2.GetAccount().Id());
		}
	}
}