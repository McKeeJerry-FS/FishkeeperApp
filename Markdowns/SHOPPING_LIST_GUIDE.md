# Shopping List Feature Guide

## Overview

The Shopping List feature allows you to plan and track future purchases for your aquarium tanks. This is especially useful when setting up a new tank or planning upgrades.

## Features

### 1. Create Shopping List Items

- **Item Types**: Equipment, Supply, or Livestock
- **Priority Levels**: Low, Medium, or High
- **Estimated Cost Tracking**: Track budget planning
- **Purchase Links**: Save URLs to online stores
- **Type-Specific Fields**:
  - Equipment: Equipment type selection
  - Supply: Supply category selection
  - Livestock: Species information

### 2. Organize by Tank

- Each shopping list item is associated with a specific tank
- Filter items by tank or view all items across tanks
- Access shopping list directly from tank details page

### 3. Track Purchase Status

- Mark items as purchased with date tracking
- Separate views for pending and purchased items
- Track actual purchase dates

### 4. Add to Inventory

Shopping list items can be added directly to your inventory based on type:

#### Supply Items

- One-click addition to Supply Inventory
- Automatically populates fields from shopping list
- Item is removed from shopping list after addition

#### Equipment Items

- Redirects to Equipment creation with pre-filled data
- Allows you to select specific equipment type
- Marked as purchased and kept for reference

#### Livestock Items

- Redirects to Livestock creation with pre-filled data
- Allows you to add detailed livestock information
- Marked as purchased and kept for reference

## Access Points

### Navigation Menu

1. Go to **Aquarium** dropdown in main navigation
2. Click **Shopping List**

### Tank Details Page

1. View any tank details
2. Find "Shopping List" in the Quick Actions section
3. Click "Add to Shopping List" in the Add New section

### Direct Tank Shopping List

- Access tank-specific shopping list: `/ShoppingList?tankId={tankId}`

## Creating a Shopping List Item

1. Click "Add Item" button
2. Select the tank this item is for
3. Choose item type (Equipment, Supply, or Livestock)
4. Enter item details:
   - Name (required)
   - Brand
   - Model/Species
   - Quantity (default: 1)
   - Estimated cost
   - Priority level
   - Purchase link (optional)
   - Notes (optional)
5. Click "Add to Shopping List"

## Managing Shopping List Items

### Edit an Item

- Click the pencil icon next to the item
- Modify any fields
- Save changes

### Mark as Purchased

- Click the checkmark icon
- Item moves to "Purchased Items" section
- Purchase date is automatically recorded

### Delete an Item

- Click the trash icon
- Confirm deletion
- Item is permanently removed

### Add to Inventory

1. Click the "Add to" dropdown button
2. Choose appropriate action based on item type:
   - **Supply**: Adds directly to Supply Inventory
   - **Equipment**: Opens Equipment creation form
   - **Livestock**: Opens Livestock creation form
3. Complete any additional required information
4. Save to add to your inventory

## Shopping List Dashboard

### Summary Cards

- **Total Items**: Total number of items in your list
- **Pending**: Items not yet purchased
- **Purchased**: Items already bought
- **Est. Cost**: Total estimated cost of pending items

### Pending Items Section

Displays all items you still need to purchase with:

- Priority badges (High, Medium, Low)
- Type indicators (Equipment, Supply, Livestock)
- Full item details
- Quick action buttons

### Purchased Items Section

Shows historical record of purchased items with:

- Purchase date
- Final cost
- Option to remove from list

## Best Practices

1. **Set Priorities**: Mark essential items as "High" priority
2. **Track Costs**: Enter estimated costs for budget planning
3. **Save Links**: Add purchase links for easy ordering later
4. **Regular Updates**: Mark items as purchased promptly
5. **Add to Inventory**: Transfer items to inventory once received
6. **Clean Up**: Remove old purchased items periodically

## Tips

- Create a shopping list when planning a new tank setup
- Use priority levels to determine purchase order
- Track estimated vs. actual costs for future planning
- Keep purchase links for warranty reference
- Use notes field for size, color, or compatibility information
- Review shopping list before making purchase trips

## Integration with Other Features

### With Tanks

- Shopping list items are tank-specific
- Access from tank details page
- Plan equipment and livestock for specific tanks

### With Supply Inventory

- Seamlessly add supply items to inventory
- Maintain quantity tracking
- Automatic categorization

### With Equipment

- Pre-fill equipment creation forms
- Maintain brand and model information
- Track installation dates

### With Livestock

- Transfer planned livestock to actual inventory
- Maintain species information
- Track addition dates

## Future Enhancements

Potential additions for this feature:

- Shopping list templates for common tank types
- Cost comparison with online stores
- Budget alerts when costs exceed limits
- Share shopping lists with others
- Export shopping lists to PDF or CSV
- Integration with vendor APIs for price checking
