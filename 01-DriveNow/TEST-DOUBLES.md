**Name:** Omar Salem  
**Group / ID:** Group 3

# Test doubles

## Hand-written double types

| Kind | Example | Intended use |
|---|---|---|
| Dummy | `DummyAuditLogger` | Satisfy a dependency that the test never uses |
| Stub | `StubClock` | Return a fixed time for time-boundary rules |
| Spy | `SpySmsSender` | Record whether and what notification was sent |
| Mock | `Mock<IPaymentGateway>` | Verify payment calls and arguments at the gateway boundary |
| Fake | `FakeBookingRepository` | Provide an in-memory repository with controllable booking state |

The current production services still instantiate concrete adapters internally. These examples define the planned test-double roles; they are not yet wired into DriveNow tests.


