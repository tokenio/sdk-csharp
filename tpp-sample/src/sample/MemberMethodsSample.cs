﻿using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class MemberMethodsSample
    {

        private static readonly byte[] PICTURE = Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQEASABIAAD//gATQ3JlYXRlZCB3aXRoIEdJTVD/2wBDA"
                    + "BALDA4MChAODQ4SERATGCgaGBYWGDEjJR0oOjM9PDkzODdASFxOQERXRT"
                    + "c4UG1RV19iZ2hnPk1xeXBkeFxlZ2P/2wBDARESEhgVGC8aGi9jQjhCY2N"
                    + "jY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2Nj"
                    + "Y2NjY2P/wgARCAAIAAgDAREAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAA"
                    + "AAABv/EABQBAQAAAAAAAAAAAAAAAAAAAAD/2gAMAwEAAhADEAAAAT5//8"
                    + "QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABBQJ//8QAFBEBAAAAAAA"
                    + "AAAAAAAAAAAAAAP/aAAgBAwEBPwF//8QAFBEBAAAAAAAAAAAAAAAAAAAA"
                    + "AP/aAAgBAgEBPwF//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQAGP"
                    + "wJ//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABPyF//9oADAMBAA"
                    + "IAAwAAABAf/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAwEBPxB//8Q"
                    + "AFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAgEBPxB//8QAFBABAAAAAAAA"
                    + "AAAAAAAAAAAAAP/aAAgBAQABPxB//9k=");

        /// <summary>
        /// Aliases the specified member.
        /// </summary>
        /// <param name="member">Member.</param>
        public static void Aliases(TppMember member)
        {
            Alias alias = new Alias() { 
                    Type=Alias.Types.Type.Domain,
                    Value= "verified-domain.com"

            };
            // add the alias
            member.AddAliasBlocking(alias);
            // remove the alias
            member.RemoveAliasBlocking(alias);

        }

        /// <summary>
        /// Resolves the alias.
        /// </summary>
        /// <param name="client">Client.</param>
        public static void ResolveAlias(TokenClient client)
        {
            Alias alias = new Alias()
            {
                Value = "user-email@example.com"

            };
           

            // If this call fails then the alias does not correspond to an existing member.
            TokenMember resolved = client.ResolveAliasBlocking(alias);

            // resolved member ID from alias
            string memberId = resolved.Id;

            // The resolved alias
            // will have the correct type, e.g. EMAIL.
            Alias resolvedAlias = resolved.Alias;
        }


        /// <summary>
        /// Keys the specified crypto and member.
        /// </summary>
        /// <param name="crypto">Crypto.</param>
        /// <param name="member">Member.</param>
        public static void Keys(ICryptoEngine crypto, TppMember member)
        {
            Key lowKey = crypto.GenerateKey(Level.Low);
            member.ApproveKeyBlocking(lowKey);

            Key standardKey = crypto.GenerateKey(Level.Standard);
            Key privilegedKey = crypto.GenerateKey(Level.Privileged);
            member.ApproveKeysBlocking(new[] { standardKey, privilegedKey });

            member.RemoveKeyBlocking(lowKey.Id);
        }


        /// <summary>
        /// Profiles the specified member.
        /// </summary>
        /// <returns>The profiles.</returns>
        /// <param name="member">Member.</param>
        public static Profile Profiles(TppMember member)
        {
            Profile name = new Profile() { 
                DisplayNameFirst= "Tycho",
                DisplayNameLast = "Nestoris"
            };

            member.SetProfileBlocking(name);
            member.SetProfilePictureBlocking("image/jpeg", PICTURE);

            Profile profile = member.GetProfileBlocking(member.MemberId());
            return profile;
        }
    }

}
