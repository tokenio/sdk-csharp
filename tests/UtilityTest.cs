using System.Text;
using Io.Token.Proto.Common.Alias;
using Io.Token.Proto.Common.Token;
using NUnit.Framework;
using sdk;
using sdk.Security;
using static Io.Token.Proto.Common.Alias.Alias.Types.Type;
using static Io.Token.Proto.Common.Token.AccessBody.Types;
using static Io.Token.Proto.Common.Token.AccessBody.Types.Resource.Types;

namespace tests
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
            Assert.AreEqual(Util.HashAlias(alias), "HHzc3XVck27qD2gadGVzjffaBZrU8ZLEd2jmtcyPKeev");
        }

        [Test]
        public void Base58Hashing()
        {
            var result = Base58.Encode(Encoding.UTF8.GetBytes("bob@token.io"));
            Assert.AreEqual("2rjpGWoxbc8ASyDVx", result);
        }
    }
}
