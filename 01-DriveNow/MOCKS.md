**Name:** Omar Salem  
**Group / ID:** Group 3

# Mocks

The five legacy tests are in `DriveNow.LegacyTests/ReceiptServiceTests.cs`. They were run before source changes as a baseline and pass after the current edits. They have not yet been replaced with real line-item, VAT, and formatter tests, and no claim is made that they catch a wrong VAT amount.

`ReceiptService` accepts four interfaces: `ILineItemFactory`, `ITaxCalculator`, `ICurrencyFormatter`, and `IReceiptFormatter`. The legacy tests should be examined to establish which of these were introduced solely to mock collaborators. Keep interfaces that represent real variation or an external boundary; remove only those whose abstraction adds no useful seam.

The requested Verify snapshots, deliberate snapshot-diff exercise, and final interface-removal review remain to be done.


