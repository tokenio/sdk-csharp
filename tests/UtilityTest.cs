using System.Text;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types.Resource.Types;

namespace Test
{
    [TestFixture]
    public class UtilityTest
    {
        [Test]
        public void JsonSerializer()
        {
            var payload = new TokenPayload
            {
                To = new TokenMember
                {
                    Id = "memberId"
                },
                Access = new AccessBody
                {
                    Resources =
                    {
                        new Resource
                        {
                            AllAddresses = new AllAddresses()
                        }
                    }
                },
                RefId = "refId"
            };
            var expected = "{\"access\":{\"resources\":[{\"allAddresses\":{}}]},\"refId\":\"refId\",\"to\":{\"id\":\"memberId\"}}";
            Assert.AreEqual(expected, Util.ToJson(payload));
        }

        [Test]
        public void HashAlias()
        {
            var alias = new Alias
            {
                Type = Email,
                Value = "bob@token.io"
            };
            Assert.AreEqual(Util.NormalizeAndHash(alias), "HHzc3XVck27qD2gadGVzjffaBZrU8ZLEd2jmtcyPKeev");
        }

        [Test]
        public void Base58Hashing()
        {
            var result = Base58.Encode(Encoding.UTF8.GetBytes("bob@token.io"));
            Assert.AreEqual("2rjpGWoxbc8ASyDVx", result);
        }
    }
}
