# Changelog

Covers the published NuGet packages `Token.SDK.Core`, `Token.SDK.TPP` and `Token.SDK.User`.

## 2.0.0

Gateway calls that were previously made without authentication now use authenticated requests.
This is a breaking release: the unauthenticated methods have been removed, not deprecated.

### Breaking changes

#### Core (`Token.SDK.Core`)
- Removed the recovery operations from `TokenClient`, `Member` and `Rpc.Client`: `BeginRecovery`,
  `CreateRecoveryAuthorization`, `GetRecoveryAuthorization`, `CompleteRecovery*`, `AddRecoveryRule`,
  `UseDefaultRecoveryRule`, `AuthorizeRecovery`.
  ([1f32b96](https://github.com/tokenio/sdk-csharp/commit/1f32b96))
- Removed `GetDefaultAgent`. New members are created without a recovery agent.
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- Removed the unauthenticated `TokenClient.ResolveAlias` and `TokenClient.GetMemberId`. Use
  `Member.ResolveAlias` / `Member.GetMemberId` (and the `Blocking` variants) instead.
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- Removed the unauthenticated `GetMember` calls. Member lookups use the authenticated client.
  ([29f792e](https://github.com/tokenio/sdk-csharp/commit/29f792e),
  [1f32b96](https://github.com/tokenio/sdk-csharp/commit/1f32b96))
- `TokenCluster.DEVELOPMENT` now points at the sandbox hosts
  (`api-grpc.sandbox.token.io`, `web-app.sandbox.token.io`).
  ([deee15b](https://github.com/tokenio/sdk-csharp/commit/deee15b))

#### TPP (`Token.SDK.TPP`)
- Removed member creation from `TokenClient`: `CreateMember`, `CreateMemberBlocking`,
  `CreateMemberInRealmBlocking`, `SetUpMember`, `SetUpMemberBlocking`.
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- Removed `CompleteRecovery*` and `RecoverEidasMember`.
  ([1f32b96](https://github.com/tokenio/sdk-csharp/commit/1f32b96))
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  - `ParseTokenRequestCallbackUrl` and `ParseTokenRequestCallbackParams`
    ([29f792e](https://github.com/tokenio/sdk-csharp/commit/29f792e))
  - `GetTokenRequestResult` ([24efbab](https://github.com/tokenio/sdk-csharp/commit/24efbab))
  - `RetrieveTokenRequest` ([31db9d0](https://github.com/tokenio/sdk-csharp/commit/31db9d0))

#### User (`Token.SDK.User`)
- Removed `CompleteRecovery*`.
  ([1f32b96](https://github.com/tokenio/sdk-csharp/commit/1f32b96))
- Removed `NotifyCreateAndEndorseToken` and `UpdateTokenRequest`.
  ([cb14bd9](https://github.com/tokenio/sdk-csharp/commit/cb14bd9))
- `TokenClient.CreateMember` no longer takes a recovery agent.
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- `TokenClient.ProvisionDevice` and `ProvisionDeviceBlocking` take a `memberId` instead of an `Alias`.
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  `NotifyAddKey`, `NotifyPaymentRequest`, `GetBlob` and `RetrieveTokenRequest`.
  ([31db9d0](https://github.com/tokenio/sdk-csharp/commit/31db9d0),
  [fc8a954](https://github.com/tokenio/sdk-csharp/commit/fc8a954))

### Added
- `Member.ResolveAlias` and `Member.GetMemberId`, with `Blocking` variants (Core).
  ([d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))
- `Member.GetTokenRequestResult`, `ParseTokenRequestCallbackUrl`, `ParseTokenRequestCallbackParams` and
  `RetrieveTokenRequest` (TPP).
  ([29f792e](https://github.com/tokenio/sdk-csharp/commit/29f792e),
  [24efbab](https://github.com/tokenio/sdk-csharp/commit/24efbab),
  [31db9d0](https://github.com/tokenio/sdk-csharp/commit/31db9d0))
- `Member.NotifyPaymentRequest`, `NotifyAddKey`, `GetBlob` and `RetrieveTokenRequest` (User).
  ([fc8a954](https://github.com/tokenio/sdk-csharp/commit/fc8a954),
  [31db9d0](https://github.com/tokenio/sdk-csharp/commit/31db9d0))
- `Util.TOKEN` alias constant (TPP).
  ([29f792e](https://github.com/tokenio/sdk-csharp/commit/29f792e))

### Removed
- The legacy `Token.SDK.Net` project, its samples and its tests. It is superseded by Core, TPP and User.
  ([76ea05f](https://github.com/tokenio/sdk-csharp/commit/76ea05f))
- The recovery and eIDAS recovery samples, and the TPP member creation sample.
  ([1f32b96](https://github.com/tokenio/sdk-csharp/commit/1f32b96),
  [d03998d](https://github.com/tokenio/sdk-csharp/commit/d03998d))

### Changed
- Samples and sample tests were updated for the above, and now include customer data on
  transfer destinations.
  ([7d92326](https://github.com/tokenio/sdk-csharp/commit/7d92326),
  [fe3a8e6](https://github.com/tokenio/sdk-csharp/commit/fe3a8e6),
  [deee15b](https://github.com/tokenio/sdk-csharp/commit/deee15b))

## 1.0.9
