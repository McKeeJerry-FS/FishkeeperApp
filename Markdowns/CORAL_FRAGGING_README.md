# Coral Fragging Feature

## Overview

The Coral Fragging feature allows users to track coral propagation activities in their reef tanks. This feature is specifically designed for saltwater, reef, and nano-reef aquarium enthusiasts who engage in coral fragging (coral propagation).

## Key Features

### Access Control

- **Conditional Access**: The Fragging menu item only appears in the navigation when a user has at least one Saltwater, Reef, or Nano-Reef tank
- **Tank-Specific**: All fragging operations are tied to reef-compatible tanks only

### Frag Tracking

Track comprehensive information about each coral frag:

#### Basic Information

- Frag Name/ID (unique identifier)
- Tank assignment
- Parent coral (optional link to source coral in your tank)
- Coral species
- Frag date
- Image upload capability

#### Physical Characteristics

- Initial size (in inches)
- Current size tracking
- Number of polyps
- Coloration
- Growth measurements over time

#### Fragging Details

- Fragging method (Saw, Bone Cutters, Break, Laser, Scalpel, Twist/Snap)
- Fragging notes
- Mounting type (Frag Plug, Rock, Disc, Tile, Magnetic Mount)
- Location in tank

#### Progress Tracking

- Status (Growing, Encrusted, Ready, Sold, Lost, Traded)
- Encrustation tracking
- Last measurement date
- Growth rate calculations

#### Sales & Trading

- Ready for sale flag
- Sale date
- Sale price tracking
- Buyer/trader information
- Total sales reporting

## Usage

### Creating a Reef Tank

Before using the fragging feature, create at least one reef-compatible tank:

1. Go to **Tanks** → **Create Tank**
2. Select tank type: Saltwater, Reef, or Nano-Reef
3. The Fragging menu will automatically appear in navigation

### Creating a Frag

1. Navigate to **Fragging** → **New Frag**
2. Select the tank for this frag
3. Optionally link to a parent coral
4. Enter frag details (name, species, size, etc.)
5. Document the fragging method used
6. Specify mounting and placement
7. Upload an image (optional)

### Tracking Growth

1. Navigate to the frag's detail page
2. Click **Edit**
3. Update **Current Size** and **Last Measurement Date**
4. The system will automatically calculate growth
5. Upload new photos to document progress

### Managing Frag Status

Update the status as your frag progresses:

- **Growing**: Initial healing and growth phase
- **Encrusted**: Frag has attached to its mount
- **Ready**: Frag is mature enough for sale/trade
- **Sold/Traded**: Frag has been sold or traded
- **Lost**: Frag did not survive

### Sales Tracking

When ready to sell or trade:

1. Check **Ready for Sale/Trade** checkbox
2. When sold, enter:
   - Sale date
   - Sale price
   - Buyer information
3. Update status to "Sold" or "Traded"
4. View sales summary on the main fragging page

## Dashboard Features

The main Fragging page includes:

- **Total Frags**: Count of all frags
- **Growing**: Number of actively growing frags
- **Ready for Sale**: Number of mature frags
- **Total Sales**: Sum of all sales revenue

## Filter Options

- Filter frags by tank
- View all frags across all reef tanks
- Search and sort capabilities

## Best Practices

1. **Unique Naming**: Use a consistent naming scheme (e.g., "WWC Rainbow Frag #1", "Blue Tort 2024-01")
2. **Regular Measurements**: Update size weekly or monthly to track growth accurately
3. **Photo Documentation**: Take photos at fragging and monthly thereafter
4. **Method Documentation**: Record your fragging technique for future reference
5. **Parent Tracking**: Link frags to parent corals for lineage tracking
6. **Encrustation Dates**: Note when frags fully attach for health tracking

## Navigation Path

**Home** → **Fragging** (only visible with reef tanks)

## Model Structure

- **Model**: `CoralFrag`
- **Controller**: `FraggingController`
- **Views**: Index, Create, Edit, Details, Delete
- **Database Table**: `CoralFrags`

## Future Enhancements (Potential)

- Frag colonies (batch tracking)
- Growth rate graphs
- Profitability analytics
- Frag history/lineage tree
- Integration with livestock health tracking
- Automated reminders for measurements
- Export frag catalog for sales

## Notes

- Images are stored using the existing ImageService
- All frags are user-specific and tank-specific
- The feature respects existing authentication and authorization
- Soft deletes are not implemented; deletions are permanent
