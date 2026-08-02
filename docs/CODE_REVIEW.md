# Code review — ServiceableBus (engineering notes)

Private review notes for the current tree on `master`. Severity: **P0** (correctness / security), **P1** (reliability / design debt), **P2** (polish / maintainability).

No code was changed in the docs-only pass that produced this file.

---

## Summary

ServiceableBus is a thin ASP.NET Core–oriented wrapper over Azure Service Bus: DI registration, a hosted processor host, JSON publish/subscribe, and typed handlers with optional application properties.

The shape is clear and usable for a small app, but several contract/implementation mismatches, resource-lifetime bugs, and silent message-loss paths need attention before treating this as a hardened library.

---

## P0 — Correctness / security

### 1. `IServiceablePublisher` vs `ServiceablePublisher` signature mismatch

- Interface: `PublishAsync<T>(T message, Func<ServiceablePropertyBag> action)` — second arg required, non-nullable.
- Implementation: `PublishAsync<T>(T message, Func<ServiceablePropertyBag>? action = null)` — optional.

Consumers of the public interface cannot omit the bag; tests call the concrete `internal` type with one argument. Align the interface and implementation (and document the chosen contract).

### 2. Dual JSON stacks; Newtonsoft attributes are inert on the wire path

- `ServiceableBusEvent<T>` uses Newtonsoft `[JsonProperty("camelCase...")]`.
- Publisher and listeners use **System.Text.Json** with no naming policy that mirrors those attributes.

Wire JSON will not reliably match the annotated names. Pick one serializer end-to-end, or configure STJ to match the intended contract (and drop unused Newtonsoft if STJ wins).

### 3. Null deserialize still completes the message

In both queue and topic listeners, `CompleteMessageAsync` runs even when `Deserialize` returns `null`. That drops poison/empty payloads without handler invocation or dead-letter. Prefer dead-letter (or abandon) when the body cannot be bound to `T`.

### 4. Connection string committed in sample `appsettings.json`

`samples/ServiceableBus.Azure.Sample.Api/appsettings.json` contains a full namespace connection string including `SharedAccessKey`. Treat as compromised if this ever hit a shared remote: rotate the key, move to user secrets / env, keep only a placeholder in repo.

### 5. `ServiceablePropertyBag.ToDictionary()`

```csharp
return Properties.ToDictionary();
```

Standard LINQ has no parameterless `ToDictionary()` on `(string Key, object Value)[]`. This should be `ToDictionary(p => p.Key, p => p.Value)` (and should handle duplicate keys). Confirm it actually builds in CI; if it does, document which API is being hit.

---

## P1 — Reliability / design

### 6. Two independent `ServiceBusClient` instances

- `ServiceableBusBackgroundService` constructs a client for listeners.
- `ServiceableBusClientFactory` constructs another for senders.

Same connection string, no shared lifetime/disposal coordination for the factory’s client (`IServiceableBusClientFactory` / factory is not `IAsyncDisposable`). Prefer one client (or factory-owned client) injected into both publish and receive paths, disposed once on shutdown.

### 7. `Dispose` / `DisposeAsync` misuse on processors

Listeners’ `Dispose()` and `StopProcessor` call `_processor?.DisposeAsync()` without awaiting. That can race shutdown and leave AMQP links open. Implement `IAsyncDisposable` on listeners (and stop using sync `Dispose` for async SDK types), and await from the hosted service.

### 8. Fire-and-forget error handling

`ProcessErrorAsync` and catch blocks use `Console.WriteLine`. No `ILogger`, no metrics, no correlation with `CorrelationId` / message id. For a library, inject `ILogger<T>` at minimum.

### 9. Dead-letter without reason/description

`DeadLetterMessageAsync(args.Message)` omits reason and error description, which makes ASB dead-letter investigation harder. Pass exception type/message (truncated) into the DLQ properties.

### 10. Reflection to invoke `Handle`

Listeners resolve the handler then `GetMethod("Handle")` + `Invoke`. Since `T` is known, resolve `IServiceableBusEventHandler<T>` and call `Handle` directly. Less brittle, better trim/AOT story, easier to test.

### 11. Registration surface locked to `WebApplicationBuilder`

Extensions cannot be used from generic `Host.CreateApplicationBuilder` / worker services without ASP.NET. Overloads on `IHostApplicationBuilder` or `IServiceCollection` (+ `IConfiguration`) would widen adoption. The `Microsoft.AspNetCore.App` framework reference is otherwise heavier than a messaging library needs.

### 12. Publisher options registered as multiple `IServiceablePublisherOptions` singletons

Works with `IEnumerable<>`, but duplicate registrations for the same `T` fail at runtime via `FirstOrDefault` (first wins, silent). Consider a keyed dictionary or fail-fast on duplicate `MessageType`.

### 13. No processor options exposed

Prefetch, auto-lock renew, max concurrent calls, sub-queue, etc. are all SDK defaults. Fine for a v0; for production you will want `ServiceBusProcessorOptions` (or a thin options wrapper) on listener registration.

### 14. Sample / tests drift from the intended event model

- Sample events correctly use `ServiceableBusEvent<TPayload>`.
- Unit tests use a bare `IServiceableBusEvent` with `Property` only — fine for isolation, but they do not cover the envelope (`MessageTypeName` / `Payload` / `CreatedAt`) or property-bag publish path thoroughly.
- Publisher tests do not cover missing options, application properties, or JSON shape.

---

## P2 — Maintainability / API polish

### 15. Empty marker interfaces

`IServiceableBusEvent` and `IServiceableBusPayload` carry no members. That is acceptable for constraints, but gives no guidance; the abstract `ServiceableBusEvent<T>` is where the real contract lives and is easy to miss.

### 16. Naming inconsistency

- Background service field: `_queueListeners` holds all `IServiceableListener` (queues and topics).
- `AddServiceableBusEventSender` vs resolve type `IServiceablePublisher` (sender/publisher naming mix).
- Interface file historically referenced as publisher/bus naming in docs (`IServiceableBusPublisher`) — actual type is `IServiceablePublisher`.

### 17. `ServiceablePublisherOptions` constructor param named `queueName` but property is `QueueOrTopicName`

Minor, but matches the “sender works for queue or topic” behavior — rename the parameter for clarity.

### 18. `ExcludeFromCodeCoverage` on options

Reasonable for POCOs; ensure real logic stays covered (listeners/publisher paths especially).

### 19. Hosted service is `public`; most implementation types are `internal`

Good default. `ServiceableBusBackgroundService` being public is fine for DI, but its constructor creates the ASB client immediately — hard to unit test without a real (or substitutable) client. Consider injecting `ServiceBusClient` or the factory.

### 20. Package / README identity

README previously said `dotnet add package ServiceableBus` and `using ServiceableBus.Extensions`. Actual projects/packages are `ServiceableBus.Azure` / `ServiceableBus.Contracts`, and extensions live in `ServiceableBus.Azure.Extensions`. (Fixed in root README + `/docs` in this pass.)

### 21. CI versioning

`build.yml` packs with `MAJOR.MINOR.RUN_NUMBER` on `master` only. Fine, but package README is the repo root README — keep that file accurate since it ships in the nupkg.

### 22. Topic sample event reuses nested payload type oddly

`TestTopicEvent` inherits `ServiceableBusEvent<TestEvent.TestEventPayload>` while also declaring its own nested `TestEventPayload` record that is unused for the base type argument. Harmless at runtime if unused, but confusing for readers of the sample.

---

## Test gaps (priority order)

1. Publish with `ServiceablePropertyBag` → assert `ApplicationProperties` on the `ServiceBusMessage`.
2. Publish when options missing → `InvalidOperationException`.
3. Listener: deserialize `null` / invalid JSON → expected settle action (once defined).
4. Listener: handler throws → dead-letter (and ideally reason).
5. Interface compile surface: call `IServiceablePublisher` the way apps do (not only the concrete class).
6. Extension registration order / multiple listeners smoke test against `ServiceCollection`.

---

## Suggested follow-up work (not done here)

1. Align publisher interface/implementation; standardize on one JSON stack.
2. Single shared `ServiceBusClient` with correct async disposal.
3. Fix complete-on-null and DLQ metadata; add `ILogger`.
4. Typed handler invoke; `IServiceCollection` registration API.
5. Rotate any leaked sample credentials; scrub sample config.
6. Expand tests around settle semantics and property bags.

---

## What looks solid

- Clear split between Contracts and Azure implementation.
- Consistent registration “builder” story for queue listener, topic listener, handler, sender.
- Scoped handler resolution per message (correct DI lifetime for typical app services).
- Application properties plumbed both ways via `ServiceablePropertyBag`.
- Existing MSTest + NSubstitute coverage for start/stop processor and a happy-path handle.
- Stryker workflow present — mutation testing mindset is good for a messaging lib.
