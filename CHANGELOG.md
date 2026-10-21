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
  ([#107](https://github.com/tokenio/sdk-csharp/pull/107))
- Removed `GetDefaultAgent`. New members are created without a recovery agent.
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- Removed the unauthenticated `TokenClient.ResolveAlias` and `TokenClient.GetMemberId`. Use
  `Member.ResolveAlias` / `Member.GetMemberId` (and the `Blocking` variants) instead.
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- Removed the unauthenticated `GetMember` calls. Member lookups use the authenticated client.
  ([#102](https://github.com/tokenio/sdk-csharp/pull/102),
  [#107](https://github.com/tokenio/sdk-csharp/pull/107))
- `TokenCluster.DEVELOPMENT` now points at the sandbox hosts
  (`api-grpc.sandbox.token.io`, `web-app.sandbox.token.io`).
  ([#97](https://github.com/tokenio/sdk-csharp/pull/97))

#### TPP (`Token.SDK.TPP`)
- Removed member creation from `TokenClient`: `CreateMember`, `CreateMemberBlocking`,
  `CreateMemberInRealmBlocking`, `SetUpMember`, `SetUpMemberBlocking`.
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- Removed `CompleteRecovery*` and `RecoverEidasMember`.
  ([#107](https://github.com/tokenio/sdk-csharp/pull/107))
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  - `ParseTokenRequestCallbackUrl` and `ParseTokenRequestCallbackParams`
    ([#102](https://github.com/tokenio/sdk-csharp/pull/102))
  - `GetTokenRequestResult` ([#104](https://github.com/tokenio/sdk-csharp/pull/104))
  - `RetrieveTokenRequest` ([#110](https://github.com/tokenio/sdk-csharp/pull/110))

#### User (`Token.SDK.User`)
- Removed `CompleteRecovery*`.
  ([#107](https://github.com/tokenio/sdk-csharp/pull/107))
- Removed `NotifyCreateAndEndorseToken` and `UpdateTokenRequest`.
  ([#103](https://github.com/tokenio/sdk-csharp/pull/103))
- `TokenClient.CreateMember` no longer takes a recovery agent.
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- `TokenClient.ProvisionDevice` and `ProvisionDeviceBlocking` take a `memberId` instead of an `Alias`.
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- Moved the following from `TokenClient` to `Member`, using authenticated requests:
  `NotifyAddKey`, `NotifyPaymentRequest`, `GetBlob` and `RetrieveTokenRequest`.
  ([#110](https://github.com/tokenio/sdk-csharp/pull/110),
  [#105](https://github.com/tokenio/sdk-csharp/pull/105))

### Added
- `Member.ResolveAlias` and `Member.GetMemberId`, with `Blocking` variants (Core).
  ([#111](https://github.com/tokenio/sdk-csharp/pull/111))
- `Member.GetTokenRequestResult`, `ParseTokenRequestCallbackUrl`, `ParseTokenRequestCallbackParams` and
  `RetrieveTokenRequest` (TPP).
  ([#102](https://github.com/tokenio/sdk-csharp/pull/102),
  [#104](https://github.com/tokenio/sdk-csharp/pull/104),
  [#110](https://github.com/tokenio/sdk-csharp/pull/110))
- `Member.NotifyPaymentRequest`, `NotifyAddKey`, `GetBlob` and `RetrieveTokenRequest` (User).
  ([#105](https://github.com/tokenio/sdk-csharp/pull/105),
  [#110](https://github.com/tokenio/sdk-csharp/pull/110))
- `Util.TOKEN` alias constant (TPP).
  ([#102](https://github.com/tokenio/sdk-csharp/pull/102))

### Removed
- The legacy `Token.SDK.Net` project, its samples and its tests. It is superseded by Core, TPP and User.
  ([#108](https://github.com/tokenio/sdk-csharp/pull/108))
- The recovery and eIDAS recovery samples, and the TPP member creation sample.
  ([#107](https://github.com/tokenio/sdk-csharp/pull/107),
  [#111](https://github.com/tokenio/sdk-csharp/pull/111))

### Changed
- Samples and sample tests were updated for the above, and now include customer data on
  transfer destinations.
  ([#94](https://github.com/tokenio/sdk-csharp/pull/94),
  [#95](https://github.com/tokenio/sdk-csharp/pull/95),
  [#97](https://github.com/tokenio/sdk-csharp/pull/97))

## 1.0.9
