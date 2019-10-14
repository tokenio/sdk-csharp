using static Tokenio.TokenCluster.TokenEnv;

namespace Tokenio {
    public class TokenCluster {
        private TokenCluster(string url, string webAppUrl) {
            WebAppUrl = webAppUrl;
            Url = url;
        }

        public string WebAppUrl { get; }

        public string Url { get; }

        public static TokenCluster PRODUCTION => new TokenCluster("api-grpc.token.io", "web-app.token.io");

        public static TokenCluster INTEGRATION => new TokenCluster("api-grpc.int.token.io", "web-app.int.token.io");

        public static TokenCluster SANDBOX => new TokenCluster("api-grpc.sandbox.token.io", "web-app.sandbox.token.io");

        public static TokenCluster STAGING => new TokenCluster("api-grpc.stg.token.io", "web-app.stg.token.io");

        public static TokenCluster PERFORMANCE => new TokenCluster("api-grpc.perf.token.io", "web-app.perf.token.io");

        public static TokenCluster DEVELOPMENT => new TokenCluster("api-grpc.dev.token.io", "web-app.dev.token.io");

        public enum TokenEnv {
            Development,
            Production,
            Integration,
            Sandbox,
            Staging,
            Performance
        }

        public static TokenCluster GetCluster(TokenEnv env) {
            switch (env) {
                case Production:
                    return PRODUCTION;
                case Integration:
                    return INTEGRATION;
                case Sandbox:
                    return SANDBOX;
                case Staging:
                    return STAGING;
                case Performance:
                    return PERFORMANCE;
                case Development:
                    return DEVELOPMENT;
                default:
                    return DEVELOPMENT;
            }
        }
    }
}
