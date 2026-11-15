**Name:** Omar Salem  
**Group / ID:** Group 3

# Naming conventions

## Tests

- Test projects end in `.UnitTests` or `.ArchitectureTests` and reference the production project they exercise.
- Test classes use the production type name plus `Tests` (`RentalMathTests`, `ParkingFeeCalculatorTests`).
- Test files use the class name. Methods follow `Method_Scenario_ExpectedResult` where practical.
- Use xUnit `[Fact]`, `[Theory]`, and data attributes. Use FluentAssertions for all assertions.
- DriveNow unit tests tag the business rule with `[Trait("Rule", "BR-xx")]`.

## Test doubles

- Hand-written fakes and stubs use `Fake<Type>` and `Stub<Type>` names.
- Mocks use `Mock<Type>` and are reserved for interaction checks at external boundaries.
- Builders use `TypeBuilder` or a Bogus `TypeFaker` when randomized data is useful.

## Production code

- Interfaces begin with `I` and live beside the layer that owns the abstraction.
- Repository types end with `Repository` and belong in a `Repositories` namespace.
- Services end with `Service` and belong in a `Services` namespace.
- Exceptions end with `Exception` and belong in `DriveNow.Domain.Exceptions`.


