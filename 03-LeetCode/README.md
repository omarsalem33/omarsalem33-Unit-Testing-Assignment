**Name:** Omar Salem  
**Group / ID:** Group 3

# LeetCode Binary Search

**Name:** Omar Salem  
**Group / ID:** Group 3  
**LeetCode profile:** [OmarSalem33](https://leetcode.com/u/OmarSalem33/)

## 875. Koko Eating Bananas

`KokoEatingBananas.MinEatingSpeed` binary-searches the answer from 1 through the largest pile. For a candidate speed `k`, the required hours are the sum of `ceil(pile / k)`. If that fits within `h`, the minimum may be smaller, so the upper bound moves left; otherwise, the lower bound moves right. The loop ends at the smallest feasible speed.

Time: `O(n log(maxPile))`. Extra space: `O(1)`.

Example trace for `piles = [3, 6, 7, 11]`, `h = 8`:

| low | high | mid | hours needed | decision |
|---:|---:|---:|---:|---|
| 1 | 11 | 6 | 6 | keep lower half |
| 1 | 6 | 3 | 10 | raise low to 4 |
| 4 | 6 | 5 | 8 | lower high to 5 |
| 4 | 5 | 4 | 8 | lower high to 4 |
| 4 | 4 | - | - | answer 4 |

## 33. Search in Rotated Sorted Array

`SearchInRotatedSortedArray.Search` uses binary search. At each step, one half is sorted. The target's range determines whether to keep that half or discard it. It returns the matching index or `-1` when absent.

Time: `O(log n)`. Extra space: `O(1)`.

Example trace for 
ums = [4, 5, 6, 7, 0, 1, 2]`, `target = 0`:

| low | mid | high | sorted half / decision |
|---:|---:|---:|---|
| 0 | 3 | 6 | left `[4..7]`; 0 is outside, move low to 4 |
| 4 | 5 | 6 | left `[0..1]`; 0 is inside, move high to 4 |
| 4 | 4 | 4 | found at index 4 |

## Submission links

- Koko accepted submission: [Add your accepted LeetCode submission URL]
- Rotated array accepted submission: [Add your accepted LeetCode submission URL]

LeetCode acceptance screenshots must be captured from your account after submitting. These account-specific proof items are left for you to add.



