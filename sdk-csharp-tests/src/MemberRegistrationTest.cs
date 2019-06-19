using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Grpc.Core;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.TokenRequests;
using Tokenio.Tpp.TokenRequests;
using Tokenio.User;
using Tokenio.Utils;
using TokenioTest.Common;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Common.TokenProtos.TokenOperationResult.Types;
using Account = Tokenio.Account;
using Member = Tokenio.User.Member;
using ProtoToken = Tokenio.Proto.Common.TokenProtos.TokenRequest;
using Tokenio.Proto.Common.AliasProtos;
using TokenioTest;
using TokenioTest.Asserts;
using Tokenio;

namespace TokenioTest
{
    public class MemberRegistrationTest
    {
        private static readonly int ALIAS_VERIFICATION_TIMEOUT_MS = 60000;
        private static readonly int ALIAS_VERIFICATION_POLL_FREQUENCY_MS = 1000;

        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();



        [Fact]
        public void CreateMember()
        {
            Alias alias = Sample.alias();
            Member member = rule.Token().CreateMemberBlocking(alias);
            MemberAssertion.AssertThat(member).HasAlias(alias).HasNKeys(3);

        }

        [Fact]
        public void CreateMember_verifyAlias() 
        {
            Alias alias = Sample.alias(false);
            Member member = rule.Member(alias);
            MemberAssertion.AssertThat(member).HasNAliases(0).HasNKeys(3);


        //    // wait until verification email is sent
        //    Polling.WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS,

        //        ()=>Assert.True(rule))
        //    waitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () ->
        //        assertThat(rule.mockServiceClient().getEmails(alias.getValue()).size())
        //                .isNotZero()
        //);

        //// check verification email content
        //Mail email = rule.mockServiceClient().getEmails(alias.getValue()).get(0);
        //Personalization personalization = email.getPersonalizations().get(0);

        //assertThat(email.getTemplateId())
        //        .isNotNull()
        //        .isNotBlank();

        //assertThat(email.getPersonalizations()).hasSize(1);
        //assertThat(personalization.getTo()).hasSize(1);
        //assertThat(personalization.getTo().get(0).getEmail())
        //        .isEqualToIgnoringCase(alias.getValue());
        //assertThat(email.getSubject()).isEqualTo("Verify Your Email");

        //String verificationLink = personalization.getSubstitutions().get("[link]");
        //assertThat(verificationLink).isNotNull();

        //// verify email
        //Request request = new Request.Builder()
        //        .url(verificationLink)
        //        .build();
        //Response response = new OkHttpClient().newCall(request).execute();
        //assertThat(response.isSuccessful()).isTrue();

        //assertThat(member)
                //.hasNAliases(1)
                //.hasAlias(alias);
    }


        [Fact]
        public void LoginMember()
        {
            Alias alias = Sample.alias();
            Member member = rule.Token().CreateMemberBlocking(alias);
            MemberAssertion.AssertThat(member).HasNAliases(1);
            Member loggedIn = rule.Token().GetMemberBlocking(member.MemberId());
            MemberAssertion.AssertThat(loggedIn)
                    .HasAliases(member.GetAliasesBlocking()).HasNKeys(3);
        }


        [Fact]
        public void LoginBusinessMember()
        {
            Alias alias = Sample.DomainAlias();
            Tokenio.Tpp.Member member = tppRule.Token().CreateMemberBlocking(alias);
            MemberAssertion.AssertThat(member).HasAlias(alias);
            Tokenio.Tpp.Member loggedIn = tppRule.Token().GetMemberBlocking(member.MemberId());
            MemberAssertion.AssertThat(loggedIn)
                    .HasAliases(member.GetAliasesBlocking()).HasNKeys(3);
        }


        [Fact]
        public void provisionDevice()
        {
            Alias alias = Sample.alias();
            Member member = rule.Token().CreateMemberBlocking(alias);
            MemberAssertion.AssertThat(member).HasAlias(alias);
            using (Tokenio.User.TokenClient secondDevice = (Tokenio.User.TokenClient)rule.NewSdkInstance())
            {
                DeviceInfo deviceInfo = secondDevice
                        .ProvisionDeviceBlocking(member.GetFirstAliasBlocking());
                member.ApproveKeysBlocking(deviceInfo.Keys);

                Member loggedIn = secondDevice.GetMemberBlocking(deviceInfo.MemberId);

                MemberAssertion.AssertThat(loggedIn)
                  .HasAliases(member.GetAliasesBlocking()).HasNKeys(6);

            }
        }



    [Fact]
    public void AddAlias()
        {
            Alias alias1 = Sample.alias();
            Member member = rule.Token().CreateMemberBlocking(alias1);
            MemberAssertion.AssertThat(member).HasAlias(alias1);

            Alias alias2 = Sample.alias();
            member.AddAliasBlocking(alias2);
            MemberAssertion.AssertThat(member).HasAlias(alias2);

            Alias alias3 = Sample.alias();
            member.AddAliasBlocking(alias3);
            MemberAssertion.AssertThat(member)
                   .HasAliases(alias1, alias2, alias3)
                    .HasNKeys(3);
        }


       [Fact]
        public void AddAliases()
        {
            Alias alias1 = Sample.alias();
            Alias alias2 = Sample.alias();
            Alias alias3 = Sample.alias();

            Member member = rule.Token().CreateMemberBlocking(alias1);
            MemberAssertion.AssertThat(member).HasAlias(alias1);
            Alias[] aliases= new[] {alias2, alias3 };

            member.AddAliasesBlocking(aliases.ToList());
            MemberAssertion.AssertThat(member)
                .HasAliases(alias1, alias2, alias3);
        }


        [Fact]
        public void RemoveAlias()
        {
            Alias alias1 = Sample.alias();
            Alias alias2 = Sample.alias();

            Member member = rule.Token().CreateMemberBlocking(alias1);
            MemberAssertion.AssertThat(member).HasAlias(alias1);

            member.AddAliasBlocking(alias2);
            MemberAssertion.AssertThat(member)
               .HasAliases(alias1, alias2);

            member.RemoveAliasBlocking(alias2);
            MemberAssertion.AssertThat(member)
                   .HasAliases(alias1)
                    .HasNKeys(3);
        }

       [Fact]
        public void AliasDoesNotExist()
        {
            var alias = Sample.alias();
            Assert.Null(rule.Token().ResolveAliasBlocking(alias));
        }

        [Fact]
        public void AliasExists()
        {
            Alias alias = Sample.alias();
            rule.Token().CreateMemberBlocking(alias);
           Assert.NotEmpty(rule.Token().ResolveAliasBlocking(alias).Id);
        }

    }
}
