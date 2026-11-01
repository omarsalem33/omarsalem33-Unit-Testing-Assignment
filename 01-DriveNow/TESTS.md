**Name:** Omar Salem  
**Group / ID:** Group 3

# Test plan and current results

## Current automated suite

- `DriveNow.UnitTests`: 43 passed, 0 failed. The tests cover age, leap-day ages, license years, rental-duration boundaries, worked pricing examples, discount bands, promo failure messages, insurance, driver costs, late fees, and cancellation fees.
- `DriveNow.LegacyTests`: 5 passed, 0 failed. These are the existing receipt tests and have not yet been replaced.
- `ParkSmart.UnitTests`: 23 passed; `ParkSmart.ArchitectureTests`: 4 passed. The tests cover all ten fee stages and project dependency/naming rules.

## Commands

```powershell
dotnet test 01-DriveNow/DriveNow/DriveNow.sln
dotnet test 02-ParkSmart-TDD/ParkSmart/ParkSmart.slnx
```

## Remaining self-review evidence

The assignment asks for a five-step self-review loop, AI review screenshot, and recorded repeated runs for the DriveNow feature tasks. Those artifacts have not been captured in this workspace yet. The checks above are actual local test runs, not substitutes for that evidence. Ten consecutive no-build suite runs also passed: DriveNow took 2.65–4.52 seconds per run; ParkSmart took 3.60–7.53 seconds per run.




