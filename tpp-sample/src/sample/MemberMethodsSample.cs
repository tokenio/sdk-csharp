using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    public static class MemberMethodsSample {

        private static readonly byte[] PICTURE = Convert.FromBase64String (
            "/9j/4AAQSkZJRgABAQEASABIAAD//gATQ3JlYXRlZCB3aXRoIEdJTVD/2wBDA" +
            "BALDA4MChAODQ4SERATGCgaGBYWGDEjJR0oOjM9PDkzODdASFxOQERXRT" +
            "c4UG1RV19iZ2hnPk1xeXBkeFxlZ2P/2wBDARESEhgVGC8aGi9jQjhCY2N" +
            "jY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2Nj" +
            "Y2NjY2P/wgARCAAIAAgDAREAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAA" +
            "AAABv/EABQBAQAAAAAAAAAAAAAAAAAAAAD/2gAMAwEAAhADEAAAAT5//8" +
            "QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABBQJ//8QAFBEBAAAAAAA" +
            "AAAAAAAAAAAAAAP/aAAgBAwEBPwF//8QAFBEBAAAAAAAAAAAAAAAAAAAA" +
            "AP/aAAgBAgEBPwF//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQAGP" +
            "wJ//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABPyF//9oADAMBAA" +
            "IAAwAAABAf/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAwEBPxB//8Q" +
            "AFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAgEBPxB//8QAFBABAAAAAAAA" +
            "AAAAAAAAAAAAAP/aAAgBAQABPxB//9k=");

        /// <summary>
        /// Adds and removes an alias.
        /// </summary>
        /// <param name="member">member</param>
        public static void Aliases (TppMember member) {
            Alias alias = new Alias {
                Type = Alias.Types.Type.Domain,
                Value = "verified-domain.com"

            };
            // add the alias
            member.AddAliasBlocking (alias);
            // remove the alias
            member.RemoveAliasBlocking (alias);

        }

        /// <summary>
        /// Resolves a user's alias.
        /// </summary>
        /// <param name="client">token client</param>
        public static void ResolveAlias (Tokenio.Tpp.TokenClient client) {
            Alias alias = new Alias {
                Value = "user-email@example.com"

            };

            // If this call fails then the alias does not correspond to an existing member.
            TokenMember resolved = client.ResolveAliasBlocking (alias);

            // resolved member ID from alias
            string memberId = resolved.Id;

            // The resolved alias
            // will have the correct type, e.g. EMAIL.
            Alias resolvedAlias = resolved.Alias;
        }

        /// <summary>
        /// Adds and removes keys.
        /// </summary>
        /// <param name="crypto">crypto engine</param>
        /// <param name="member">member</param>
        public static void Keys (ICryptoEngine crypto, TppMember member) {
            Key lowKey = crypto.GenerateKey (Key.Types.Level.Low);
            member.ApproveKeyBlocking (lowKey);

            Key standardKey = crypto.GenerateKey (Key.Types.Level.Standard);
            Key privilegedKey = crypto.GenerateKey (Key.Types.Level.Privileged);
            member.ApproveKeysBlocking (new [] { standardKey, privilegedKey });

            member.RemoveKeyBlocking (lowKey.Id);
        }

        /// <summary>
        /// Sets a profile name and picture.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>profile</returns>
        public static Profile Profiles (TppMember member) {
            Profile name = new Profile {
                DisplayNameFirst = "Tycho",
                DisplayNameLast = "Nestoris"
            };

            member.SetProfileBlocking (name);
            member.SetProfilePictureBlocking ("image/jpeg", PICTURE);

            Profile profile = member.GetProfileBlocking (member.MemberId ());
            return profile;
        }
    }

}