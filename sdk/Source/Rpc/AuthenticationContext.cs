using System.Threading;
using static Io.Token.Proto.Common.Security.Key.Types;

namespace sdk.Rpc
{
    public static class AuthenticationContext
    {
        private static readonly ThreadLocal<string> onBehalfOf = new ThreadLocal<string>();
        private static readonly ThreadLocal<Level> keyLevel = new ThreadLocal<Level>(() => Level.Low);
        private static readonly ThreadLocal<bool> customerInitiated = new ThreadLocal<bool>();

        public static string OnBehalfOf
        {
            get => onBehalfOf.Value;
            set => onBehalfOf.Value = value;
        }

        public static Level KeyLevel
        {
            get => keyLevel.Value;
            set => keyLevel.Value = value;
        }

        public static bool CustomerInitiated
        {
            get => customerInitiated.Value;
            set => customerInitiated.Value = value;
        }
        
        public static Level ResetKeyLevel() {
            var level = keyLevel.Value;
            keyLevel.Value = Level.Low;
            return level;
        }
    }
}
