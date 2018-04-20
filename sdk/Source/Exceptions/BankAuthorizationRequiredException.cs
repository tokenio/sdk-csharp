using System;

namespace sdk.Exceptions
{
    public class BankAuthorizationRequiredException : Exception
    {
        public BankAuthorizationRequiredException() 
            : base("Must call linkAccounts with bank authorization payload.")
        {
        }
    }
}
