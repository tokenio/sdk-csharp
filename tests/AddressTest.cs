using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.AddressProtos;
using static Test.TestUtil;

namespace Test
{
    [TestFixture]
    public class AddressTest
    {
        private static readonly TokenClient tokenIO = NewSdkInstance();

        private Member member;

        [SetUp]
        public void Init()
        {
            member = tokenIO.CreateMemberBlocking(Alias());
        }

        [Test]
        public void AddAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddressBlocking(name, payload);
            Assert.AreEqual(name, address.Name);
            Assert.AreEqual(payload, address.Address);
        }

        [Test]
        public void AddAndGetAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddressBlocking(name, payload);
            var result = member.GetAddressBlocking(address.Id);
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
                member.AddAddressBlocking(entry.Key, entry.Value);
            }

            CollectionAssert.AreEquivalent(
                addressMap, 
                member.GetAddressesBlocking().ToDictionary(a => a.Name, a => a.Address));
        }

        [Test]
        public void GetAddresses_NotFound()
        {
            Assert.IsEmpty(member.GetAddressesBlocking());
        }

        [Test]
        public void GetAddress_NotFound()
        {
            var fakeAddressId = Util.Nonce();
            Assert.Throws<AggregateException>(() => member.GetAddressBlocking(fakeAddressId));
        }

        [Test]
        public void DeleteAddress()
        {
            var name = Util.Nonce();
            var payload = Address();
            var address = member.AddAddressBlocking(name, payload);
            
            member.GetAddressBlocking(address.Id);
            Assert.IsNotEmpty(member.GetAddressesBlocking());
            
            member.DeleteAddressBlocking(address.Id);
            Assert.IsEmpty(member.GetAddressesBlocking());
        }
    }
}
