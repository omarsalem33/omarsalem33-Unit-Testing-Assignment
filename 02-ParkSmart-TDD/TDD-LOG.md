**Name:** Omar Salem  
**Group / ID:** Group 3

# ParkSmart TDD log

The local starter folder was empty, so the first implementation now lives in `ParkSmart/`. The initial fee-calculator test run reported six rule-example failures, which drove adjustments to started-hour counting, per-24-hour caps, and scenario fixtures. The final local suite passes 23 unit tests and 4 custom reflection-based architecture tests.

| Stage group | Requirement exercised | Tests | Result |
|---|---|---|---|
| S1-S3 | Grace window, started hours, invalid exit | `Calculate_applies_grace_and_started_hours`, `Calculate_rejects_exit_at_or_before_entry` | Passing |
| S4-S5 | Per-block caps and vehicle rate/cap | `Calculate_caps_each_24_hour_block`, `Calculate_uses_vehicle_rate_and_cap` | Passing |
| S6-S8 | Night flat, lost ticket, Friday grace | `Calculate_applies_night_flat_fee`, `Calculate_applies_lost_ticket_minimum`, `Calculate_uses_longer_friday_grace` | Passing |
| S9-S10 | Subscriber rules, EV charge, negative energy | Subscriber and energy tests | Passing |

The requested NetArchTest.Rules package is not available in the local package cache; architecture checks use simple assembly-reference reflection instead. The requested setup/red/green/refactor commit protocol and screenshots have not been created. These tests were added and run in the current working tree, so the commit history does not claim a staged TDD sequence.

**Reflection:** S4 made the largest change because the cap resets for each 24-hour block, while the initial grace period applies once to the session. This highlighted the need to test whole-session examples as well as single-hour boundaries.

