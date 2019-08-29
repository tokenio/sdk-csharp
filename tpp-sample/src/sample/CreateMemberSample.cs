using System;
using System.IO;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Security;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;

namespace TokenioSample
{
    public static class CreateMemberSample
    {
        /// <summary>
        /// Creates and returns a new token member.
        /// </summary>
        /// <returns>a new Member instance</returns>
        public static TppMember CreateMember()
        {
            // Create the client, which communicates with
            // the Token cloud.
            try
            {

                var key = Directory.CreateDirectory("./keys");

                TokenClient tokenClient = TokenClient.NewBuilder()
               .ConnectTo(Tokenio.TokenCluster.SANDBOX)
               .WithKeyStore(new UnsecuredFileSystemKeyStore(key.FullName))
               .Build();



                // An alias is a "human-readable" reference to a member.
                // Here, we use a random email. This works in test environments.
                // but in production, TokenOS would try to verify we own the address,
                // so a random address wouldn't be useful for much.
                // We use a random address because otherwise, if we ran a second
                // time, Token would say the alias was already taken.
                Alias alias = new Alias
                {
                    Value = TestUtil.RandomNumeric(10) + "+noverify@example.com",
                    Type = Alias.Types.Type.Email
                };

                TppMember newMember = tokenClient.CreateMemberBlocking(alias);
                // let user recover member by verifying email if they lose keys
                newMember.UseDefaultRecoveryRule();

                return newMember;
            }
            catch (IOException ioe)
            {
                throw new ArgumentException("", ioe);
            }
        }
    }
}
