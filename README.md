# TaskTracker

Task management system with filtering and sorting features.

## Features

- Create and manage tasks
- Filter by status or due date
- Sort by title, due date, status, or urgency
- Find the most urgent task

## Implementation

All sorting and filtering use custom algorithms (no LINQ or Array.Sort).

- **Filtering**: Linear scan, O(n) time
- **Sorting**: Insertion sort for most cases, Counting sort for status sorting
- **Urgency**: Calculated from due date and status