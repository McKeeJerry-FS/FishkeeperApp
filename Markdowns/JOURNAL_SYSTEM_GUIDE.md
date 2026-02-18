# Journal System Documentation

## Overview

The AquaHub Journal system allows users to document their aquarium journey with detailed entries linked to specific tanks. Journal entries support text content, timestamps, titles, and optional images.

## Features

### Core Functionality

- **Create Journal Entries**: Document observations, events, milestones, and notes
- **View All Entries**: Browse all journal entries in a clean, organized table
- **Entry Details**: View complete entry details with linked maintenance and water test records
- **Edit Entries**: Update existing journal entries
- **Delete Entries**: Remove unwanted entries with confirmation
- **Tank Association**: Each entry is linked to a specific tank

### Dashboard Integration

- **Latest Entry Display**: The home dashboard shows the most recent journal entry for authenticated users
- **Call to Action**: New users see an invitation to create their first journal entry
- **Quick Navigation**: Direct links to view full entries and create new ones from the dashboard

## Database Schema

### JournalEntry Table

- `Id` (int, Primary Key)
- `TankId` (int, Foreign Key to Tank)
- `Title` (string, 2-200 characters, required)
- `Content` (string, up to 5000 characters, required)
- `Timestamp` (DateTime, required)
- `ImagePath` (string, optional)

### Junction Tables

- **JournalMaintenanceLink**: Links journal entries to maintenance activities
  - `Id` (int, Primary Key)
  - `JournalEntryId` (int, Foreign Key)
  - `MaintenanceLogId` (int, Foreign Key)

- **JournalWaterTestLink**: Links journal entries to water tests
  - `Id` (int, Primary Key)
  - `JournalEntryId` (int, Foreign Key)
  - `WaterTestId` (int, Foreign Key)

## Navigation

Journal entries can be accessed from:

1. **Main Navigation**: Care dropdown → Journal
2. **Home Dashboard**: Latest entry widget with "View All Entries" link
3. **Direct URL**: `/Journal/Index`

## User Interface

### Index View

- Displays all journal entries in a table format
- Shows: Date/Time, Title, Tank, Content Preview, Actions
- Empty state with call-to-action for first entry
- Filtering by date (newest first)
- Tips and information panel

### Create/Edit Views

- Tank selection dropdown
- Date/Time picker (defaults to current time)
- Title input (2-200 characters)
- Content textarea (up to 5000 characters)
- Optional image path input
- Helpful tips sidebar

### Details View

- Full entry display with formatted content
- Tank and timestamp information
- Optional image display
- Linked maintenance activities (when implemented)
- Linked water tests (when implemented)
- Quick action buttons (Edit, Delete, Back)

### Delete View

- Confirmation screen showing entry details
- Warning message about permanent deletion
- Cancel and Confirm options

## Security

- All journal operations require authentication
- Users can only access their own journal entries
- Tank ownership is verified before creating/editing entries
- SQL injection protection via Entity Framework

## Future Enhancements

1. **Image Upload**: Direct image upload instead of path input
2. **Rich Text Editor**: Support for formatted text, lists, and styling
3. **Tags/Categories**: Organize entries by custom tags
4. **Search**: Full-text search across journal entries
5. **Export**: PDF or text export of journal entries
6. **Linking UI**: Visual interface to link maintenance and water test records
7. **Attachments**: Support for multiple images or documents
8. **Templates**: Pre-defined entry templates for common observations
9. **Reminders**: Set reminders to make regular journal entries
10. **Sharing**: Share entries with other users or make them public

## API Endpoints

### GET /Journal

Lists all journal entries for the authenticated user

### GET /Journal/Details/{id}

Displays details of a specific journal entry

### GET /Journal/Create

Displays the create journal entry form

### POST /Journal/Create

Creates a new journal entry

### GET /Journal/Edit/{id}

Displays the edit form for a journal entry

### POST /Journal/Edit/{id}

Updates an existing journal entry

### GET /Journal/Delete/{id}

Displays the delete confirmation page

### POST /Journal/Delete/{id}

Deletes a journal entry

## Usage Tips

1. **Daily Observations**: Create entries to track daily changes and behaviors
2. **Event Documentation**: Record important events like equipment changes or new livestock additions
3. **Problem Tracking**: Document issues and their resolutions
4. **Milestone Celebrations**: Celebrate achievements like successful breeding or coral propagation
5. **Regular Entries**: Consistent journaling helps identify patterns and trends

## Technical Implementation

- **Controller**: `JournalController.cs`
- **Model**: `JournalEntry.cs`, `JournalMaintenanceLink.cs`, `JournalWaterTestLink.cs`
- **Views**: `/Views/Journal/` directory
- **Database**: Managed via Entity Framework Core migrations
- **Authentication**: ASP.NET Core Identity

## Migration

Database migration: `20260218021843_AddJournalEntries`

To apply the migration:

```bash
dotnet ef database update
```

## Validation Rules

- **Title**: Required, 2-200 characters
- **Content**: Required, maximum 5000 characters
- **Tank**: Required, must belong to the authenticated user
- **Timestamp**: Required
- **ImagePath**: Optional

## Testing Checklist

- [x] Create journal entry
- [x] View journal index
- [x] View entry details
- [x] Edit existing entry
- [x] Delete entry
- [x] Dashboard displays latest entry
- [x] Dashboard shows CTA when no entries exist
- [x] Navigation link works
- [x] Security: Cannot access other users' entries
- [x] Validation: Title and content required
- [x] Validation: Character limits enforced
