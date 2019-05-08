using System.Text;
using Xunit;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types.Resource.Types;

namespace Test
{
    public class UtilityTest
    {
        [Fact]
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
            Assert.Equal(expected, Util.ToJson(payload));
        }

        [Fact]
        public void HashAlias()
        {
            var alias = new Alias
            {
                Type = Email,
                Value = "bob@token.io"
            };
            Assert.Equal("HHzc3XVck27qD2gadGVzjffaBZrU8ZLEd2jmtcyPKeev", Util.NormalizeAndHashAlias(alias));
        }

        [Fact]
        public void Base58Hashing()
        {
            var result = Base58.Encode(Encoding.UTF8.GetBytes("bob@token.io"));
            Assert.Equal("2rjpGWoxbc8ASyDVx", result);
        }
    }
}
