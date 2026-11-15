**Name:** Omar Salem  
**Group / ID:** Group 3

# Bug report and fixes

| Rule | Source issue observed | Change made | Test evidence |
|---|---|---|---|
| BR-03 | Rental days rounded up at exactly 60 leftover minutes (`>= 60`) | Changed the grace check to require more than 60 minutes | `RentalMathTests.CalculateRentalDays_applies_more_than_sixty_minute_grace` covers 25h00 and 25h01 |
| BR-21 | Overlap check used inclusive comparisons and rejected back-to-back reservations | Changed overlap to strict interval intersection | Not yet covered by an automated test |
| BR-16 | Luxury booking required age strictly greater than 25 | Changed age condition to 25 or older | Not yet covered by an automated test |
| BR-33 | Seven-day rentals did not receive the duration discount | Included day 7 in the 7–13 band | `PricingServiceTests.Calculate_applies_duration_discount_bands` |
| BR-37 | Promo discount was not reduced when combined discounts crossed the cap | Limited promo discount to the remaining discount percentage | `PricingServiceTests.Calculate_caps_combined_duration_loyalty_and_promo_discounts` |
| BR-39 | Silver customers incorrectly received the free additional driver | Limited the free first driver to Gold and Platinum tiers | Worked example B test checks 700 EGP for one Silver additional driver |
| BR-41 | Exactly 240 late minutes was charged as a whole day instead of four started hours | Switched the daily-rate branch to strictly more than 240 minutes | `ReturnServiceTests.LateFee_applies_the_correct_boundary` |
| BR-51 | Cancellation exactly 48 hours before pickup was charged 50% | Changed the free-fee boundary to 48 hours or more | `CancellationServiceTests.CalculateCancellationFee_uses_hour_boundary_and_platinum_exemption` |

The assignment requires a failing-test commit before each fix. This working tree does not yet contain that commit sequence, so the table records source issues and edits without inventing commit IDs.





