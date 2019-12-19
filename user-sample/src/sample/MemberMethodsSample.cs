using System;
using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using Tokenio.User.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public static class MemberMethodsSample
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
        /// Adds, removes, and resolves aliases.
        /// </summary>
        /// <param name="tokenClient">token client</param>
        /// <param name="member">member</param>
        /// <returns>resolved member ID and alias</returns>
        public static TokenMember Aliases(Tokenio.User.TokenClient tokenClient, UserMember member)
        {
            Alias alias1 = member.GetFirstAliasBlocking();
            Alias alias2 = new Alias
            {
                Type = Alias.Types.Type.Domain,
                Value = "alias2-" + Util.Nonce() + "+noverify@token.io"
            };
            // add the alias
            member.AddAliasBlocking(alias2);
            Alias alias3 = new Alias
            {
                Type = Alias.Types.Type.Domain,
                Value = "alias3-" + Util.Nonce() + "+noverify@token.io"
            };
            Alias alias4 = new Alias
            {
                Type = Alias.Types.Type.Domain,
                Value = "alias4-" + Util.Nonce() + "+noverify@token.io"
            };
            member.AddAliasesBlocking((new[] {alias3, alias4}).ToList());
            // remove the alias
            member.RemoveAliasBlocking(alias1);
            member.RemoveAliasesBlocking((new[] {alias2, alias3}).ToList());
            TokenMember resolved = tokenClient.ResolveAliasBlocking(alias4);
            return resolved;
        }

        /// <summary>
        /// Adds and removes keys.
        /// </summary>
        /// <param name="crypto">crypto engine</param>
        /// <param name="member">member</param>
        public static void Keys(ICryptoEngine crypto, UserMember member)
        {
            Key lowKey = crypto.GenerateKey(Key.Types.Level.Low);
            member.ApproveKeyBlocking(lowKey);
            Key standardKey = crypto.GenerateKey(Key.Types.Level.Standard);
            Key privilegedKey = crypto.GenerateKey(Key.Types.Level.Privileged);
            member.ApproveKeysBlocking(new[] {standardKey, privilegedKey});
            member.RemoveKeyBlocking(lowKey.Id);
        }

        /// <summary>
        /// Sets a profile name and picture.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>profile</returns>
        public static Profile Profiles(UserMember member)
        {
            Profile name = new Profile
            {
                DisplayNameFirst = "Tycho",
                DisplayNameLast = "Nestoris"
            };
            member.SetProfileBlocking(name);
            member.SetProfilePictureBlocking("image/jpeg", PICTURE);
            Profile profile = member.GetProfileBlocking(member.MemberId());
            return profile;
        }
    }
}