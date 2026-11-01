**Name:** Omar Salem  
**Group / ID:** Group 3

# Hidden inputs audit

| Source | Hidden input | Why it hurts tests | Status |
|---|---|---|---|
| `PricingService` | `DateTime.Today`, internally created promo repository | Promo validity depends on wall-clock date and global seed state | Not removed yet |
| `BookingService` | `DateTime.Now`, internally created repositories, gateways, notifications, random generator | Timing, references, and side effects cannot be controlled independently | Not removed yet |
| `ReturnService` | `DateTime.Now`, internally created repositories, payment and email adapters | Return fees and side effects are coupled to current time and global state | Not removed yet |
| `CancellationService` | `DateTime.Now`, internally created repositories, payment and notification adapters | Fee boundaries and interactions are difficult to test deterministically | Not removed yet |
| `InMemoryDatabase` | `DateTime.Today` in static seed initialization | Seed values vary by execution date and share mutable static collections | Not removed yet |
| `AuditLog` | File append and current timestamp | Calling a use case writes to a real file | Not removed yet |

`DateTime.Today` in the console is presentation-only. The items above still need refactoring behind injectable clock, repository, reference-generator, and logging abstractions.


