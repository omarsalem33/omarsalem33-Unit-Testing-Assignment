**Name:** Omar Salem  
**Group / ID:** Group 3

# Baseline run

Before changing the DriveNow code, I ran the existing solution with:

```powershell
dotnet test 01-DriveNow/DriveNow/DriveNow.sln --no-restore
```

The solution contained only `DriveNow.LegacyTests`; the baseline run passed 5 tests. The existing project had no DriveNow unit-test project. I did not create or run the three naive `BookingService.CreateBooking` tests required by Task 2, so this file does not claim a completed FIRST audit.


