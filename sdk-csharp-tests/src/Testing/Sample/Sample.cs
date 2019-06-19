using System;
using Google.Protobuf.Collections;
using Tokenio.Proto.Common.AddressProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.TokenRequests;
using AccessBody = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.ResourceType;
using TokenMember = Tokenio.Proto.Common.TokenProtos.TokenMember;

namespace TokenioTest.Testing.Sample
{
    public abstract class Sample
    {
        private Sample()
        {
        }

        /// <summary>
        /// Randoms the numeric.
        /// </summary>
        /// <returns>The numeric.</returns>
        /// <param name="size">Size.</param>
        public static string RandomNumeric(int size)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }

        /// <summary>
        /// Address this instance.
        /// </summary>
        /// <returns>The address.</returns>
        public static Address Address()
        {
            return new Address
            {
                HouseNumber = "425",
                Street = "Broadway",
                City = "Redwood City",
                PostCode = "94063",
                Country = "US"
            };
        }

        /// <summary>
        /// Handlers the instructions.
        /// </summary>
        /// <returns>The instructions.</returns>
        /// <param name="target">Target.</param>
        /// <param name="platform">Platform.</param>
        public static MapField<string, string> HandlerInstructions(string target, string platform)
        {            
            MapField<string, string> handlerInstructions = new MapField<string, string>();
            handlerInstructions.Add("TARGET", target);
            handlerInstructions.Add("PLATFORM", platform);
            return handlerInstructions;
        }

        /// <summary>
        /// Profile this instance.
        /// </summary>
        /// <returns>The profile.</returns>
        public static Profile Profile()
        {
            return new Profile
            {
                DisplayNameFirst = RandomNumeric(15),
                DisplayNameLast = RandomNumeric(15)
            };
        }

        /// <summary>
        /// Accesses the token request.
        /// </summary>
        /// <returns>The token request.</returns>
        /// <param name="toMember">To member.</param>
        /// <param name="redirectUrl">Redirect URL.</param>
        /// <param name="csrfToken">Csrf token.</param>
        /// <param name="state">State.</param>
        /// <param name="fromMember">From member.</param>
        /// <param name="customizationId">Customization identifier.</param>
        public static TokenRequest AccessTokenRequest(
            TokenMember toMember,
            string redirectUrl,
            string csrfToken,
            string state = null,
            TokenMember fromMember = null,
            string customizationId = null)
        {
            TokenRequest.AccessBuilder requestBuilder = TokenRequest
                .AccessTokenRequestBuilder(AccessBody.Accounts, AccessBody.Balances)
                .SetUserRefId(RandomNumeric(15))
                .SetRedirectUrl(redirectUrl)
                .SetToMemberId(toMember.Id)
                .SetDescription(RandomNumeric(15))
                .SetCsrfToken(csrfToken)
                .SetBankId("iron")
                .SetReceiptRequested(false);
            if(!string.IsNullOrEmpty(state))
            {
                requestBuilder.SetState(state);
            }
            if (!string.IsNullOrEmpty(customizationId))
            {
                requestBuilder.SetCustomizationId(customizationId);
            }
            if(fromMember != null)
            {
                requestBuilder.SetFromMemberId(fromMember.Id); 
            }

            return requestBuilder.build();
        }

        /// <summary>
        /// Transfers the token request.
        /// </summary>
        /// <returns>The token request.</returns>
        /// <param name="toMember">To member.</param>
        /// <param name="redirectUrl">Redirect URL.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="currency">Currency.</param>
        /// <param name="csrfToken">Csrf token.</param>
        /// <param name="state">State.</param>
        /// <param name="fromMember">From member.</param>
        /// <param name="customizationId">Customization identifier.</param>
        /// <param name="sourceAccountId">Source account identifier.</param>
        public static TokenRequest TransferTokenRequest(
            TokenMember toMember,
            string redirectUrl,
            string amount,
            string currency,
            string csrfToken,
            string state = null,
            TokenMember fromMember = null,
            string customizationId = null,
            string sourceAccountId = null)
        {
            TokenRequest.TransferBuilder requestBuilder = TokenRequest.TransferTokenRequestBuilder(
                    Double.Parse(amount),
                    currency)
                    .SetUserRefId(RandomNumeric(15))
                    .SetRedirectUrl(redirectUrl)
                    .SetToMemberId(toMember.Id)
                    .SetDescription(RandomNumeric(15))
                    .SetCsrfToken(csrfToken)
                    .SetBankId("iron")
                    .SetReceiptRequested(false);
            if (!string.IsNullOrEmpty(state))
            {
                requestBuilder.SetState(state);
            }
            if (!string.IsNullOrEmpty(customizationId))
            {
                requestBuilder.SetCustomizationId(customizationId);
            }
            if (fromMember != null)
            {
                requestBuilder.SetFromMemberId(fromMember.Id);
            }
            if (!string.IsNullOrEmpty(sourceAccountId))
            {
                requestBuilder.SetSourceAccount(sourceAccountId);
            }
            return requestBuilder.build();
        }
    }
}
