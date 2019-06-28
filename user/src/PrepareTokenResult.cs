using Tokenio.Proto.Common.TokenProtos;

namespace Tokenio.User {
	public class PrepareTokenResult {
		public static PrepareTokenResult Create(TokenPayload tokenPayload, Policy policy) {
			return new PrepareTokenResult {
				TokenPayload = tokenPayload,
				Policy = policy
			};
		}

		public TokenPayload TokenPayload {
			get;
			private set;
		}

		public Policy Policy {
			get;
			private set;
		}
	}
}