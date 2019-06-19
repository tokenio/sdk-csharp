using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
namespace TokenioTest.Asserts
{
    public sealed class MemberAssertion : Assert
    {
        private readonly Member actual;

        private MemberAssertion(Member actual)
            : base()
        {
            this.actual = actual;
        }

        public static MemberAssertion AssertThat(Member member)
        {
            return new MemberAssertion(member);
        }

     

        public MemberAssertion HasAlias(Alias alias)
        {

            var set = ClearRealm((actual.GetAliasesBlocking().ToList()));
            Assert.True(set.Contains(ClearRealm(alias)));
            return this;
        }

        public MemberAssertion HasAliases(params Alias[] aliases)
        {
            return HasAliases(aliases.ToList());
        }

        public MemberAssertion HasOneAlias()
        {
            return HasNAliases(1);
        }

        public MemberAssertion HasNAliases(int count)
        {
            Assert.Equal(count, actual.GetAliasesBlocking().Count());
            return this;
        }

        public MemberAssertion HasAliases(IList<Alias> aliases)
        {

            var set = ClearRealm((actual.GetAliasesBlocking().ToList()));
            var set2 = ClearRealm(aliases.ToList());
            bool isSuperset = new HashSet<Alias>(set).IsSupersetOf(set2);
            Assert.True(isSuperset);
            return this;
        }


        public MemberAssertion HasKey(String keyId)
        {
            LinkedList<string> keyIds = new LinkedList<string>();

            foreach (Key key in actual.GetKeysBlocking())
            {
                keyIds.AddLast(key.Id);
            }
                 
            Assert.Contains(keyId,keyIds);
            return this;
        }

        public MemberAssertion HasOneKey()
        {
            return HasNKeys(1);
        }

        public MemberAssertion HasNKeys(int count)
        {
          Assert.Equal(count, actual.GetKeysBlocking().Count());
          return this;
        }

        private Alias ClearRealm(Alias alias)
        {
        
            alias.Realm = "";
            return alias;
                   
        }

        private ISet<Alias> ClearRealm(List<Alias> aliases)
        {
            aliases.ForEach(a => ClearRealm(a));
            return aliases.ToHashSet();
        }

    }
}
