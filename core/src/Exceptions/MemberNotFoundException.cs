using Grpc.Core;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Utils;

namespace Tokenio.Exceptions
{
    public class MemberNotFoundException : RpcException
    {
        /// <summary>
        /// Member Not Found Exception
        /// </summary>
        /// <param name="alias">Alias.</param>
        public MemberNotFoundException(Alias alias)
            : base(new Status(StatusCode.NotFound, $"Member could not be resolved for alias: {Util.ToJson(alias)}"))
        {
        }

    }
}
