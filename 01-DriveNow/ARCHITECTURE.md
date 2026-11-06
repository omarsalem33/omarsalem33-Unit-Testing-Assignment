**Name:** Omar Salem  
**Group / ID:** Group 3

# Architecture findings

## Intended dependency direction

- `DriveNow.Domain` owns business entities, value objects, enums, exceptions, and pure calculations; it should reference no other DriveNow project.
- `DriveNow.Application` owns use cases and should depend on Domain abstractions.
- `DriveNow.Infrastructure` owns persistence and external adapters.
- `DriveNow.ConsoleApp` is the composition root and wires concrete adapters manually.

## Current gaps to resolve

- `BookingService`, `PricingService`, `ReturnService`, and `CancellationService` construct repositories and gateways internally, so use cases cannot be isolated with test doubles.
- Application currently references Infrastructure directly.
- Clock, promo lookup, random booking references, and audit logging are not injected.
- No `DriveNow.ArchitectureTests` project exists yet.

These are observed from the current project references and constructors; they have not been represented as passing architecture tests.


