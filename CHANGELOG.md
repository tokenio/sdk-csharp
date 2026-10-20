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
- Removed `GetDefaultAgent`. New members are created without a recovery agent.
- Removed the unauthenticated `TokenClient.ResolveAlias` and `TokenClient.GetMemberId`. Use
  `Member.ResolveAlias` / `Member.GetMemberId` (and the `Blocking` variants) instead.
- Removed the unauthenticated `GetMember` calls. Member lookups use the authenticated client.
- `TokenCluster.DEVELOPMENT` now points at the sandbox hosts
  (`api-grpc.sandbox.token.io`, `web-app.sandbox.token.io`).

#### TPP (`Token.SDK.TPP`)
- Removed member creation from `TokenClient`: `CreateMember`, `CreateMemberBlocking`,
  `CreateMemberInRealmBlocking`, `SetUpMember`, `SetUpMemberBlocking`.
- Removed `CompleteRecovery*` and `RecoverEidasMember`.
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  `ParseTokenRequestCallbackUrl`, `ParseTokenRequestCallbackParams`,
  `GetTokenRequestResult` and `RetrieveTokenRequest`.

#### User (`Token.SDK.User`)
- Removed `CompleteRecovery*`.
- Removed `NotifyCreateAndEndorseToken` and `UpdateTokenRequest`.
- `TokenClient.CreateMember` no longer takes a recovery agent.
- `TokenClient.ProvisionDevice` and `ProvisionDeviceBlocking` take a `memberId` instead of an `Alias`.
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  `NotifyAddKey`, `NotifyPaymentRequest`, `GetBlob` and `RetrieveTokenRequest`.

### Added
- `Member.ResolveAlias` and `Member.GetMemberId`, with `Blocking` variants (Core).
- `Member.GetTokenRequestResult`, `ParseTokenRequestCallbackUrl`, `ParseTokenRequestCallbackParams` and
  `RetrieveTokenRequest` (TPP).
- `Member.NotifyPaymentRequest`, `NotifyAddKey`, `GetBlob` and `RetrieveTokenRequest` (User).
- `Util.TOKEN` alias constant (TPP).

### Removed
- The legacy `Token.SDK.Net` project, its samples and its tests. It is superseded by Core, TPP and User.
- The recovery and eIDAS recovery samples, and the TPP member creation sample.

### Changed
- Samples and sample tests were updated for the above, and now include customer data on
  transfer destinations.

## 1.0.9