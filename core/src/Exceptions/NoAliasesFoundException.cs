using Grpc.Core;

namespace Tokenio.Exceptions
{
    public class NoAliasesFoundException : RpcException
    {
        public NoAliasesFoundException(string memberId) : base(new Status(StatusCode.NotFound,
            $"No aliases found for member: {memberId}"))
        {
        }
    }
}