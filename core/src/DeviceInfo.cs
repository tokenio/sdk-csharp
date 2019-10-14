using System.Collections.Generic;
using Tokenio.Proto.Common.SecurityProtos;

namespace Tokenio {
    /// <summary>
    /// Information about a  device being provisioned.
    /// </summary>
    public class DeviceInfo {
        /// <summary>
        /// Creates an instance of <see cref="DeviceInfo"/>.
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <param name="keys">list of keys</param>
        public DeviceInfo(string memberId, IList<Key> keys) {
            MemberId = memberId;
            Keys = keys;
        }

        /// <summary>
        /// Gets the member ID.
        /// </summary>
        public string MemberId { get; }

        /// <summary>
        /// Gets the device keys.
        /// </summary>
        public IList<Key> Keys { get; }
    }
}
