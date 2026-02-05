# Supply Tracking & Inventory Management

## Overview

The Supply Tracking feature provides comprehensive inventory management for aquarium supplies, with automated low stock warnings and predictive notifications. This system helps aquarium hobbyists maintain adequate supplies and avoid running out of critical items.

## Features Implemented

### 1. **Supply Item Model** (`SupplyItem.cs`)

Complete data model for tracking aquarium supplies including:

- **Basic Information**: Name, brand, category, description
- **Inventory Tracking**: Current quantity, minimum threshold, optimal quantity, unit of measurement
- **Stock Status**: Automatically calculated (InStock, LowStock, OutOfStock)
- **Purchase Information**: Vendor, price, purchase date, product URL
- **Usage Tracking**: Average usage per week, last used date, expiration date
- **Smart Features**: Estimated days remaining calculation

### 2. **Supply Categories** (`SupplyCategory.cs` enum)

Pre-defined categories for organizing supplies:

- Food
- Water Treatment
- Test Kits
- Medications
- Supplements
- Filter Media
- Chemicals
- Cleaning
- Other

### 3. **Stock Status** (`StockStatus.cs` enum)

Automatic status calculation based on current vs. minimum quantities:

- InStock: Adequate supply available
- LowStock: Below minimum threshold
- OutOfStock: Quantity is zero
- Discontinued: Item no longer used

### 4. **Supply Service** (`SupplyService.cs`)

Complete service layer implementing:

- **CRUD Operations**: Create, read, update, delete supplies
- **Inventory Management**:
  - Add stock (with optional purchase price)
  - Remove stock (usage tracking)
  - Update quantities
- **Low Stock Alerts**:
  - Automatic notifications when items fall below minimum
  - Expiring items tracking (30-day lookout)
  - Real-time status monitoring
- **Statistics**: Total items, low stock count, inventory value
- **Search**: Full-text search across name, brand, description

### 5. **Supply Controller** (`SupplyController.cs`)

MVC controller with actions for:

- **Index**: List all supplies with filtering (by tank, category, status)
- **Details**: View supply details with quick stock actions
- **Create/Edit**: Full CRUD operations with validation
- **Delete**: Soft delete (archives items)
- **Stock Management**: Add/Remove stock via modals
- **Low Stock View**: Dedicated page for items needing attention
- **Search**: Find supplies by keywords

### 6. **Views**

#### **Index View** (`Views/Supply/Index.cshtml`)

- Summary cards showing total items, low stock, out of stock, total value
- Filter by tank, category, and status
- Table view with stock levels and status badges
- Quick actions for viewing and editing
- Visual status indicators with color coding

#### **Details View** (`Views/Supply/Details.cshtml`)

- Large stock status card with progress bar
- Estimated days remaining display
- Quick action modals for adding/removing stock
- Purchase information panel
- Complete item details with expiration warnings

#### **Create/Edit Views**

- Organized form sections:
  - Basic Information
  - Inventory Information
  - Purchase Information
  - Usage & Expiration
  - Settings & Notes
- Tank association (optional)
- Unit selection dropdown
- Low stock alert toggle

#### **Low Stock View** (`Views/Supply/LowStock.cshtml`)

- Summary of low stock, out of stock, and expiring items
- Expiring items table (within 30 days)
- Priority-sorted list of items needing attention
- Estimated days remaining for each item
- Printable shopping list with needed quantities
- Quick reorder links to product URLs

### 7. **Navigation Integration**

Added "Supplies" menu item to main navigation between Equipment and Expenses

### 8. **Database Migration**

Created migration `AddSupplyTracking` to add `SupplyItems` table with all fields and relationships

## Key Features

### Automatic Low Stock Notifications

When a supply item's quantity falls below the minimum threshold:

1. Status automatically changes to `LowStock` or `OutOfStock`
2. System creates a notification for the user
3. Item appears in the Low Stock Alerts page
4. Badge counter updates in navigation

### Smart Stock Management

- **Add Stock**: Records purchase date and price, updates quantity
- **Remove Stock**: Tracks usage, updates "last used" date
- **Estimated Depletion**: Calculates days remaining based on average weekly usage

### Expiration Tracking

- Set expiration dates for perishable items
- Visual warnings when items are expiring soon (< 30 days)
- Red badges for expired items
- Separate tracking in Low Stock Alerts page

### Shopping List Generator

The Low Stock view includes an auto-generated shopping list that:

- Calculates needed quantities (optimal - current)
- Groups items by vendor when available
- Provides printable format
- Includes product URLs for easy reordering

## Usage Examples

### Adding a New Supply

```
1. Navigate to Supplies > Add Supply
2. Fill in:
   - Name: "Prime Water Conditioner"
   - Category: Water Treatment
   - Brand: "Seachem"
   - Current Quantity: 500
   - Minimum Quantity: 100
   - Unit: ml
   - Preferred Vendor: "Local Fish Store"
   - Last Purchase Price: $15.99
3. Click "Add Supply"
```

### Recording Supply Usage

```
1. Go to Supply Details page
2. Click "Use Supply" button
3. Enter amount removed (e.g., 50 ml)
4. System automatically:
   - Updates quantity (500 → 450 ml)
   - Records last used date
   - Recalculates days remaining
   - Checks if now below minimum
```

### Viewing Low Stock Items

```
1. Click "Low Stock" button in Supplies index
2. View three sections:
   - Expiring Soon (30-day window)
   - Low/Out of Stock Items (priority sorted)
   - Shopping List (calculated needs)
3. Click product links to reorder
4. Print shopping list for store visits
```

## Technical Implementation

### Model Relationships

- `SupplyItem` → `AppUser` (many-to-one)
- `SupplyItem` → `Tank` (many-to-one, optional)

### Computed Properties

- `Status`: Calculated based on CurrentQuantity vs. MinimumQuantity
- `EstimatedDaysRemaining`: Calculated from CurrentQuantity / AverageUsagePerWeek

### Service Layer Pattern

- Inherits from `BaseService` for context access
- Implements `ISupplyService` interface
- Uses dependency injection for `INotificationService`
- Includes comprehensive logging

### Security

- All actions require authentication (`[Authorize]` attribute)
- User ID validation on all operations
- Prevents users from accessing other users' supplies
- Soft delete maintains data integrity

## Future Enhancements

Potential improvements for future versions:

1. **Barcode Scanning**: Quick add/remove via barcode
2. **Reorder Automation**: Auto-generate orders to vendors
3. **Usage Analytics**: Detailed consumption trends and forecasting
4. **Bulk Operations**: Import/export supply lists
5. **Shared Shopping Lists**: Collaborate with other hobbyists
6. **Price Tracking**: Historical price trends and best deals
7. **Recipe Management**: Track dosing recipes and schedules
8. **Mobile App**: Dedicated mobile interface for quick updates

## API Endpoints

The SupplyController provides these routes:

```
GET    /Supply                     - List all supplies (with filters)
GET    /Supply/Details/{id}        - View supply details
GET    /Supply/Create              - Show create form
POST   /Supply/Create              - Create new supply
GET    /Supply/Edit/{id}           - Show edit form
POST   /Supply/Edit/{id}           - Update supply
GET    /Supply/Delete/{id}         - Show delete confirmation
POST   /Supply/Delete/{id}         - Delete (archive) supply
POST   /Supply/UpdateQuantity/{id} - Set exact quantity
POST   /Supply/AddStock/{id}       - Add to stock (purchase)
POST   /Supply/RemoveStock/{id}    - Remove from stock (use)
GET    /Supply/LowStock            - View low stock alerts
GET    /Supply/Search?term={term}  - Search supplies
```

## Database Schema

```sql
CREATE TABLE SupplyItems (
    Id INT PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    TankId INT NULL,
    Name NVARCHAR(255) NOT NULL,
    Category INT NOT NULL,
    Brand NVARCHAR(255),
    Description TEXT,
    CurrentQuantity FLOAT NOT NULL,
    MinimumQuantity FLOAT NOT NULL,
    OptimalQuantity FLOAT,
    Unit NVARCHAR(50) NOT NULL,
    PreferredVendor NVARCHAR(255),
    LastPurchasePrice DECIMAL(18,2),
    LastPurchaseDate TIMESTAMP,
    ProductUrl NVARCHAR(500),
    AverageUsagePerWeek FLOAT,
    LastUsedDate TIMESTAMP,
    ExpirationDate TIMESTAMP,
    StorageLocation NVARCHAR(255),
    Notes TEXT,
    EnableLowStockAlert BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (TankId) REFERENCES Tanks(Id)
);
```

## Contributing

When extending this feature:

1. Add new supply categories to `SupplyCategory` enum
2. Update views if adding new fields
3. Maintain soft delete pattern
4. Include comprehensive logging
5. Add unit tests for service methods
6. Update this documentation

---

**Version**: 1.0.0  
**Date**: February 3, 2026  
**Status**: Production Ready ✅
