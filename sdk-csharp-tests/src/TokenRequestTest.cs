using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using TokenioTest.Common;
using Xunit;
using AccessMode = Tokenio.Proto.Common.BlobProtos.Blob.Types.AccessMode;
using Payload = Tokenio.Proto.Common.BlobProtos.Blob.Types.Payload;
using Sample = TokenioTest.Testing.Sample.Sample;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace TokenioTest
{
	public abstract class TokenRequestTestBase : IDisposable
	{
		internal static readonly string tokenUrl = "https://token.io";
		internal static readonly byte[] tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");
		internal static readonly string gifType = "image/gif";
		internal TokenClient tokenClient;
		internal UserMember member;
		internal TppMember businessMember;
		internal Payload logo;
		internal MapField<string, string> colors = new MapField<string, string>();
		internal string consentText = Sample.RandomNumeric(15);
		internal string name = Sample.RandomNumeric(15);
		internal string appName = Sample.RandomNumeric(15);
		internal string customizationId = Sample.RandomNumeric(15);
		public TokenUserRule rule = new TokenUserRule();
		public TokenTppRule tppRule = new TokenTppRule();

		protected TokenRequestTestBase()
		{
			tokenClient = rule.Token();
			member = rule.Member();
			businessMember = tppRule.Member();
			logo = new Payload
			{
				Name = Sample.RandomNumeric(15),
				AccessMode = AccessMode.Public,
				Type = gifType,
				Data = ByteString.CopyFrom(tinyGif),
				OwnerId = businessMember.MemberId()
			};
			colors.Add(Sample.RandomNumeric(15), Sample.RandomNumeric(15));
		}

		public void Dispose()
		{
		}
	}

	public class TokenRequestTest : TokenRequestTestBase
	{
		[Fact]
		public void AddAndGetTransferTokenRequest()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.TransferTokenRequest(
					tokenMember,
					tokenUrl,
					"10.00",
					"EUR",
					Sample.RandomNumeric(15),
					null,
					null,
					null,
					null);
			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			Assert.NotEmpty(requestId);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest, tokenRequest);
		}

		[Fact]
		public void UpdateTransferTokenRequest()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.TransferTokenRequest(
					tokenMember,
					tokenUrl,
					"10.00",
					"EUR",
					Sample.RandomNumeric(15),
					null,
					null,
					null,
					null);
			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			TokenRequestOptions tokenRequestOptions = new TokenRequestOptions
			{
				ReceiptRequested = true
			};
			tokenClient.UpdateTokenRequestBlocking(requestId, tokenRequestOptions);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest.GetTokenRequestOptions().BankId, tokenRequest.GetTokenRequestOptions().BankId);
			Assert.True(retrievedRequest.GetTokenRequestOptions().ReceiptRequested);
		}

		[Fact]
		public void AddAndGetAccessTokenRequest()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.AccessTokenRequest(
					tokenMember,
					tokenUrl,
					Sample.RandomNumeric(15),
					null,
					null,
					null);
			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			Assert.NotEmpty(requestId);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest, tokenRequest);
		}

		[Fact]
		public void UpdateAccessTokenRequest()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.AccessTokenRequest(
					tokenMember,
					tokenUrl,
					Sample.RandomNumeric(15),
					null,
					null,
					null);
			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			TokenRequestOptions tokenRequestOptions = new TokenRequestOptions
			{
				ReceiptRequested = true
			};
			tokenClient.UpdateTokenRequestBlocking(requestId, tokenRequestOptions);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest.GetTokenRequestOptions().BankId, tokenRequest.GetTokenRequestOptions().BankId);
			Assert.True(retrievedRequest.GetTokenRequestOptions().ReceiptRequested);
		}

		[Fact]
		public void AddAndGetTransferTokenRequest_Customization()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.TransferTokenRequest(
					tokenMember,
					tokenUrl,
					"10.00",
					"EUR",
					Sample.RandomNumeric(15),
					null,
					null,
					customizationId = null,
					null);
			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			Assert.NotEmpty(requestId);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest.GetTokenRequestPayload(), tokenRequest.GetTokenRequestPayload());
			Assert.Equal(retrievedRequest.GetTokenRequestOptions(), tokenRequest.GetTokenRequestOptions());
		}

		[Fact]
		public void AddAndGetAccessTokenRequest_Customization()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.AccessTokenRequest(
					tokenMember,
					tokenUrl,
					Sample.RandomNumeric(15),
					null,
					null,
					customizationId = null);

			string requestId = businessMember.StoreTokenRequestBlocking(tokenRequest);
			Assert.NotEmpty(requestId);
			TokenRequest retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
			Assert.Equal(retrievedRequest.GetTokenRequestPayload(), tokenRequest.GetTokenRequestPayload());
			Assert.Equal(retrievedRequest.GetTokenRequestOptions(), tokenRequest.GetTokenRequestOptions());
			Assert.Equal(retrievedRequest.GetTokenRequestPayload().CustomizationId, customizationId);
		}

		[Fact]
		public void AddTokenRequest_Customization_AccessDenied()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = businessMember.MemberId()
			};
			TokenRequest tokenRequest = Sample.AccessTokenRequest(
					tokenMember,
					tokenUrl,
					Sample.RandomNumeric(15),
					null,
					null,
					customizationId = null);
			AggregateException ex = Assert.ThrowsAny<AggregateException>(() =>
					businessMember.StoreTokenRequestBlocking(tokenRequest));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
		}

		[Fact]
		public void AddAndGetTokenRequest_NotFound()
		{
			AggregateException ex = Assert.Throws<AggregateException>(() =>
					tokenClient.RetrieveTokenRequestBlocking("bogus"));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
			ex = Assert.Throws<AggregateException>(() =>
					tokenClient.RetrieveTokenRequestBlocking(member.MemberId()));
			exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.NotFound, exception.StatusCode);
		}

		[Fact]
		public void AddAndGetTokenRequest_WrongMember()
		{
			TokenMember tokenMember = new TokenMember
			{
				Id = member.MemberId()
			};
			TokenRequest tokenRequest = Sample.TransferTokenRequest(
					tokenMember,
					tokenUrl,
					"10.00",
					"EUR",
					Sample.RandomNumeric(15),
					null,
					null,
					null,
					null);
			AggregateException ex = Assert.Throws<AggregateException>(() =>
					businessMember.StoreTokenRequestBlocking(tokenRequest));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
		}

		[Fact]
		public void CreateCustomization_AccessDenied()
		{
			AggregateException ex = Assert.Throws<AggregateException>(() =>
					businessMember.CreateCustomizationBlocking(
							logo,
							colors,
							consentText,
							name,
							appName));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
		}

		[Fact]
		public void CreateCustomization_NotAnOwner()
		{
			Payload wrongLogo = new Payload
			{
				Name = Sample.RandomNumeric(15),
				AccessMode = AccessMode.Public,
				Type = gifType,
				Data = ByteString.CopyFrom(tinyGif),
				OwnerId = member.MemberId()
			};
			AggregateException ex = Assert.Throws<AggregateException>(() =>
					businessMember.CreateCustomizationBlocking(
							wrongLogo,
							colors,
							consentText,
							name,
							appName));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
		}

		[Fact]
		public void CreateCustomization_NotPublicAccessMode()
		{
			Payload wrongLogo = new Payload
			{
				Name = Sample.RandomNumeric(15),
				AccessMode = AccessMode.Default,
				Type = gifType,
				Data = ByteString.CopyFrom(tinyGif),
				OwnerId = businessMember.MemberId()
			};
			AggregateException ex = Assert.Throws<AggregateException>(() =>
					businessMember.CreateCustomizationBlocking(
							wrongLogo,
							colors,
							consentText,
							name,
							appName));
			RpcException exception = (RpcException)ex.InnerException;
			Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
		}
	}
}