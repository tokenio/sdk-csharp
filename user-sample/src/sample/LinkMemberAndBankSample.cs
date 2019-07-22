using Tokenio.User;

namespace TokenioSample
{
    public class LinkMemberAndBankSample
    {
        public static Account LinkBankAccounts(Member member)
        {
            return member.CreateTestBankAccountBlocking(1000.0, "EUR");
        }
    }
}
