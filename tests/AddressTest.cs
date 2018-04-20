using sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Io.Token.Proto.Common.Address;
using NUnit.Framework;
using sdk.Api;
using static tests.TestUtil;

namespace tests
{
    [TestFixture]
    public class AddressTest
    {
        private static readonly TokenIO tokenIO = NewSdkInstance();

        private MemberSync member;

        [SetUp]
        public void Init()
        {
            member = tokenIO.CreateMember(Alias());
        }

        [Test]
        public void AddAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddress(name, payload);
            Assert.AreEqual(name, address.Name);
            Assert.AreEqual(payload, address.Address);
        }

        [Test]
        public void AddAndGetAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddress(name, payload);
            var result = member.GetAddress(address.Id);
            Assert.AreEqual(address, result);
        }

        [Test]
        public void CreateAndGetAddresses()
        {
            var addressMap = new Dictionary<string, Address>
            {
                [Util.Nonce()] = Address(),
                [Util.Nonce()] = Address(),
                [Util.Nonce()] = Address()
            };

            foreach (var entry in addressMap)
            {
                member.AddAddress(entry.Key, entry.Value);
            }

            CollectionAssert.AreEquivalent(
                addressMap, 
                member.GetAddresses().ToDictionary(a => a.Name, a => a.Address));
        }

        [Test]
        public void GetAddresses_NotFound()
        {
            Assert.IsEmpty(member.GetAddresses());
        }

        [Test]
        public void getAddress_NotFound()
        {
            var fakeAddressId = Util.Nonce();
            Assert.Throws<AggregateException>(() => member.GetAddress(fakeAddressId));
        }

        [Test]
        public void DeleteAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddress(name, payload);
            
            member.GetAddress(address.Id);
            Assert.IsNotEmpty(member.GetAddresses());
            
            member.DeleteAddress(address.Id);
            Assert.IsEmpty(member.GetAddresses());
        }
    }
}
