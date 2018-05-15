namespace Tokenio
{
    public class TokenCluster
    {
        private TokenCluster(string url, string webAppUrl)
        {
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
    }
}
