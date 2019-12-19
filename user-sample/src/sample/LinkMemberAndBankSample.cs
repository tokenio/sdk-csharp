namespace Tokenio.Sample.User
{
    /// <summary>
    /// Links a Token member and a bank.
    /// </summary>
    public static class LinkMemberAndBankSample
    {
        /// <summary>
        /// Links a Token member and a bank.
        /// <p>Bank linking is currently only supported by the Token PSD2 mobile app.
        /// This sample shows how to link a test member with a test bank account.
        /// Real bank linking works similarly, but the BankAuthorization comes from
        /// user interaction with a bank's website.</p>
        /// </summary>
        /// <param name="member">Token member to link to a bank</param>
        /// <returns>linked token accounts</returns>
        public static Tokenio.User.Account LinkBankAccounts(Tokenio.User.Member member)
        {
            return member.CreateTestBankAccountBlocking(1000.0, "EUR");
        }
    }
}