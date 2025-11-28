# Budget Hotel Reservation System

A comprehensive web-based hotel reservation system built with ASP.NET Core MVC, featuring role-based access control, secure authentication, booking management, and administrative capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Features](#features)
- [Installation & Setup](#installation--setup)
- [Database Setup](#database-setup)
- [Login Credentials](#login-credentials)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Key Modules](#key-modules)
- [Testing Recommendations](#testing-recommendations)
- [Support](#support)

## 🎯 Overview

The Budget Hotel Reservation System is a full-featured web application designed for managing hotel reservations, room bookings, customer accounts, and administrative operations. The system provides separate interfaces for customers and administrators, with robust security features and a user-friendly interface.

## 🛠 Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 9.0)
- **Database**: SQL Server Express (LocalDB)
- **ORM**: Entity Framework Core 9.0.11
- **Authentication**: Cookie-based authentication (custom implementation)
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **QR Code Generation**: QRCoder 1.6.0
- **Frontend**: Bootstrap (responsive design)
- **Development Environment**: Visual Studio 2022 or later

## ✨ Features

### Core Functionality

- **User Management**
  - User registration with email verification
  - Secure login/logout with cookie-based authentication
  - Password reset functionality
  - Account lockout after 3 failed login attempts (15-minute lockout)
  - Password encryption using BCrypt
  - Role-based access control (Admin, Manager, Staff, Customer)

- **Room Management**
  - Browse available rooms with detailed information
  - AJAX-enabled search and filtering
  - Multiple images per room type
  - Room availability checking
  - Amenity management and display

- **Booking System**
  - Create bookings with date selection
  - Apply promotion codes for discounts
  - Multiple payment methods (Credit Card, PayPal, Bank Transfer)
  - Booking confirmation with QR code
  - View booking history
  - Cancel bookings
  - Download/view e-receipts
  - Auto-generated transaction IDs

- **Administrative Features**
  - Comprehensive admin dashboard with statistics
  - User management (CRUD operations)
  - Hotel management
  - Room and room type management
  - Booking management and status updates
  - Promotion management
  - Service and package management
  - Review and contact message management

### Security Features

- Password encryption using BCrypt
- Token-based email verification
- Secure password reset with tokens
- Login attempt tracking and blocking
- Security logging
- Role-based authorization
- Admin account protection (main admin cannot be deleted)
- Security headers middleware

### Additional Features

#### UI/UX Enhancements
- ✅ **Responsive Bootstrap UI design** - Modern, mobile-friendly interface
- ✅ **Loading indicators** - AJAX loading spinners for better UX
- ✅ **Toast notifications** - Beautiful notification system for user feedback
- ✅ **Image preview** - Preview images before upload
- ✅ **Print-optimized CSS** - Professional print styles for receipts
- ✅ **Custom error pages** - User-friendly 404, 403, 400 error pages

#### Functional Features
- ✅ **AJAX-powered room search and filtering** - Real-time search without page refresh
- ✅ **QR code generation** - Booking confirmations with QR codes (QRCoder library)
- ✅ **Price breakdown display** - Detailed pricing (base price, discount, final price)
- ✅ **Soft delete functionality** - All entities support soft delete
- ✅ **Professional payment forms** - Comprehensive validation
- ✅ **Newsletter subscription** - Email subscription system
- ✅ **Contact form** - With rate limiting protection
- ✅ **Review system** - Complete review and rating system
- ✅ **Email confirmation simulation** - Booking confirmation emails (logged)
- ✅ **Rate limiting** - Protection against spam on registration and contact forms
- ✅ **Admin dashboard charts** - Visual statistics with Chart.js
- ✅ **Export to CSV** - Export bookings data
- ✅ **Package system** - Bundle deals with services
- ✅ **Promotion system** - Discount codes with validation

## 🚀 Installation & Setup

### Prerequisites

1. **.NET 9 SDK** - Download and install from [Microsoft's official website](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **SQL Server Express** or **LocalDB** - Included with Visual Studio or download separately
3. **Visual Studio 2022** or later (recommended) or any IDE with .NET 9 support

### Setup Steps

1. **Clone or extract the project**
   ```bash
   # If using git
   git clone <repository-url>
   cd Budget-Hotel-Reservation-System-master
   ```

2. **Open the solution**
   - Open `Assignment.slnx` in Visual Studio 2022 or later
   - Or use command line: `dotnet restore`

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Configure connection string** (if needed)
   - Open `Assignment/appsettings.json`
   - Update the `DefaultConnection` string if your SQL Server instance differs
   - Default uses LocalDB: `Server=(localdb)\\mssqllocaldb;Database=BMIT2023_HotelReservation;...`

5. **Run the application**
   ```bash
   dotnet run --project Assignment
   ```
   - Or press F5 in Visual Studio
   - The application will automatically:
     - Create the database if it doesn't exist
     - Apply migrations
     - Seed initial data

6. **Access the application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - The exact port will be shown in the console output

## 💾 Database Setup

The system supports two methods for database initialization:

### Option 1: Automatic Setup (Recommended for Quick Start)

The application automatically creates the database and seeds initial data on first run:

1. Run the application once
2. The `DbInitializer` will create:
   - 2 Hotels (Kuala Lumpur, Malaysia)
   - 5 Users (1 Admin, 1 Manager, 3 Customers)
   - 4 Room Types
   - 10 Rooms
   - 10 Amenities
   - 3 Active Promotions
   - 2 Sample Bookings

### Option 2: SQL Scripts (Comprehensive Sample Data)

For a more extensive dataset (15+ records per table):

1. **Run the application once** to create the database structure
2. **Open SQL Server Management Studio (SSMS)** or Azure Data Studio
3. **Connect to your SQL Server instance** (LocalDB or SQL Server Express)
4. **Select your database**: `BMIT2023_HotelReservation`
5. **Run cleanup script** (optional, removes existing data):
   ```sql
   -- Execute: Assignment/Scripts/01_ClearAllData.sql
   ```
   ⚠️ **Warning**: This deletes all existing data irreversibly!

6. **Run sample data script**:
   ```sql
   -- Execute: Assignment/Scripts/02_InsertSampleData.sql
   ```
   This inserts:
   - 15 Hotels
   - 15 Users
   - 15 Amenities
   - 20 Room Types
   - 25 Rooms
   - 30 Room Images
   - 50 Room Type-Amenity relationships
   - 15 Services
   - 15 Packages
   - 30 Package Items
   - 12 Promotions
   - 15 Bookings
   - 12 Reviews
   - 12 Contact Messages
   - 12 Newsletter Subscriptions

7. **Set admin passwords** (if using SQL scripts):
   - After running SQL scripts, admin passwords are placeholders
   - Run the application again - `DbInitializer` will recreate admin accounts with proper passwords
   - Or use the password reset feature

## 🔐 Login Credentials

### Admin Accounts

| Email | Password | Role | Access |
|-------|----------|------|--------|
| `admin@hotel.com` | `Admin123!` | Admin | Full system access |
| `manager@hotel.com` | `Manager123!` | Manager | Full system access (same as Admin) |
| `staff@hotel.com` | `Password123!` | Staff | Limited admin access |

**Accessing Admin Panel:**
1. Login with admin or manager account
2. Click "Admin" dropdown in navigation bar
3. Or navigate directly to `/Admin/Index`

### Customer Accounts

| Email | Password | Role | Status | Notes |
|-------|----------|------|--------|-------|
| `ahmad@example.com` | `Ahmad123!` | Customer | Email Verified, Active | Has existing bookings |
| `siti@example.com` | `Siti123!` | Customer | Email Verified, Active | Has pending bookings |
| `charlie@example.com` | `Charlie123!` | Customer | Email NOT Verified, Active | Test email verification flow |

## 📁 Project Structure

```
Budget-Hotel-Reservation-System-master/
├── Assignment/
│   ├── Controllers/          # MVC Controllers
│   │   ├── AdminController.cs
│   │   ├── BookingController.cs
│   │   ├── CustomerController.cs
│   │   ├── HomeController.cs
│   │   ├── RoomController.cs
│   │   └── SecurityController.cs
│   ├── Models/              # Data Models
│   │   ├── Data/
│   │   │   ├── HotelDbContext.cs
│   │   │   └── DbInitializer.cs
│   │   ├── User.cs
│   │   ├── Hotel.cs
│   │   ├── Room.cs
│   │   ├── Booking.cs
│   │   └── ...
│   ├── Views/               # Razor Views
│   │   ├── Admin/
│   │   ├── Booking/
│   │   ├── Home/
│   │   ├── Room/
│   │   └── Security/
│   ├── Services/            # Business Logic Services
│   │   ├── EncryptionService.cs
│   │   ├── PasswordService.cs
│   │   └── PromotionValidationService.cs
│   ├── Helpers/             # Helper Classes
│   │   └── AuthenticationHelper.cs
│   ├── Middleware/          # Custom Middleware
│   │   └── SecurityHeadersMiddleware.cs
│   ├── ViewModels/          # View Models
│   ├── Migrations/          # EF Core Migrations
│   ├── Scripts/             # SQL Scripts
│   ├── wwwroot/             # Static Files
│   ├── Program.cs           # Application Entry Point
│   └── appsettings.json     # Configuration
└── Assignment.slnx          # Solution File
```

## ⚙️ Configuration

### appsettings.json

Key configuration settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BMIT2023_HotelReservation;..."
  },
  "EncryptionKey": "E546C8DF278CD5931069B522E695D4F2",
  "SecuritySettings": {
    "MaxLoginAttempts": 3,
    "LockoutMinutes": 15
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@hotel.com",
    "FromName": "Budget Hotel Reservation System",
    "BaseUrl": "https://localhost:5001"
  }
}
```

### Important Configuration Notes

- **Connection String**: Update if using a different SQL Server instance
- **EncryptionKey**: Used for encrypting sensitive data (change in production!)
- **SecuritySettings**: Configure login attempt limits and lockout duration
- **EmailSettings**: Configure SMTP settings for sending verification emails and OTP codes

#### Email Configuration Guide

The application uses MailKit to send emails for:
- **Email Verification**: Users receive a verification link when registering
- **Password Reset OTP**: Users receive a 6-digit OTP code for password reset

**For Gmail:**
1. Enable 2-Factor Authentication on your Google account
2. Generate an App Password: https://myaccount.google.com/apppasswords
3. Use the App Password (not your regular password) in `SmtpPassword`
4. Set `SmtpHost` to `"smtp.gmail.com"` and `SmtpPort` to `587`

**For Other Email Providers:**
- **Outlook/Hotmail**: `smtp-mail.outlook.com`, Port `587`
- **Yahoo**: `smtp.mail.yahoo.com`, Port `587`
- **Custom SMTP**: Use your provider's SMTP settings

**Development/Testing:**
- If email settings are not configured, the system will show fallback links/OTPs in the UI
- For production, ensure all email settings are properly configured

## 🔑 Key Modules

### Module Assignments (Team of 4)

Each team member is responsible for **at least 2 core modules** as per assignment requirements:

#### **Team Member 1 (PIC)**
1. **Security Module**
   - User registration with email verification
   - Login/logout with cookie-based authentication
   - Password reset with secure tokens
   - Login attempt tracking and blocking (3 attempts, 15-minute lockout)
   - Role-based authorization
   - Rate limiting on registration form
   - Math captcha for registration
   - Security logging

2. **Customer Module**
   - Profile management (view and edit)
   - Profile picture upload with preview
   - Change password
   - View booking history
   - Cancel bookings
   - User preferences (language, theme)

#### **Team Member 2 (PIC)**
1. **Admin Module - User & Hotel Management**
   - Admin dashboard with statistics and charts
   - User management (CRUD operations)
   - Hotel management (CRUD operations)
   - Admin account protection
   - Export bookings to CSV

2. **Review Module**
   - Submit reviews for bookings
   - View reviews on room details
   - Review moderation (admin)
   - Review pagination
   - Average rating calculation

#### **Team Member 3 (PIC)**
1. **Admin Module - Room & Amenity Management**
   - Room type management (CRUD operations)
   - Room management (CRUD operations)
   - Amenity management
   - Room-amenity relationship management
   - Multiple images per room type
   - Service management
   - Package management

2. **Room Catalog Module**
   - Browse available rooms
   - AJAX-enabled search and filtering
   - View room details with amenities
   - Check room availability
   - Room image carousel
   - Pagination

#### **Team Member 4 (PIC)**
1. **Booking Module**
   - Create bookings with date selection
   - Apply promotion codes
   - Multiple payment methods (Credit Card, PayPal, Bank Transfer)
   - Booking confirmation with QR code
   - View booking history
   - Download/view e-receipts
   - Cancel bookings
   - Booking status management
   - Email confirmation simulation
   - Package bookings

2. **Contact & Home Module**
   - Contact form with rate limiting
   - Newsletter subscription
   - Home page with featured rooms
   - Package browsing
   - Public pages (About, Blog, Help Center, etc.)

---

### Detailed Module Breakdown

#### 1. Security Module (Team Member 1)
- User registration with email verification
- Login/logout with cookie-based authentication
- Password reset with secure tokens
- Login attempt tracking and blocking
- Role-based authorization
- Rate limiting on registration form
- Math captcha for registration
- Security logging

#### 2. Admin Module (Team Members 2 & 3)
- **Dashboard**: Statistics and overview with Chart.js
- **User Management**: Create, read, update, delete users
- **Hotel Management**: Manage hotel information
- **Room Management**: Manage rooms and room types
- **Booking Management**: View and update booking statuses
- **Promotion Management**: Create and manage discount codes
- **Review Management**: Moderate customer reviews
- **Contact Messages**: View and respond to inquiries
- **Service & Package Management**: Manage services and packages
- **Amenity Management**: Manage amenities and relationships
- **Export Functionality**: Export bookings to CSV

#### 3. Customer Module (Team Member 1)
- Profile management
- View and edit personal information
- Change password
- View booking history
- Cancel bookings
- **Favorites/Wishlist**: Save favorite rooms
- User preferences (language, theme)

#### 4. Room Catalog Module (Team Member 3)
- Browse available rooms
- AJAX-enabled search and filtering
- View room details with amenities
- Check room availability
- Multiple images per room type
- Room reviews display
- Pagination

#### 5. Booking Module (Team Member 4)
- Create bookings with date selection
- Apply promotion codes
- Multiple payment methods
- Booking confirmation with QR code
- View booking history
- Download/view e-receipts
- Cancel bookings
- **Email confirmation simulation**
- Package bookings

#### 6. Review Module (Team Member 2)
- Submit reviews for bookings
- View reviews on room details
- Review moderation (admin)
- Review pagination
- Average rating calculation

#### 7. Contact & Home Module (Team Member 4)
- Contact form with rate limiting
- Newsletter subscription
- Home page with featured rooms
- Package browsing
- Public pages (About, Blog, Help Center, Terms, Privacy)

## 🆕 Recent Improvements

### UI/UX Enhancements
- ✅ **Custom Error Pages** - User-friendly 404, 403, and 400 error pages
- ✅ **Loading Indicators** - Global AJAX loading spinners for better user experience
- ✅ **Toast Notifications** - Modern notification system replacing basic alerts
- ✅ **Image Preview** - Preview images before upload (profile pictures)
- ✅ **Print-Optimized CSS** - Professional print styles for booking receipts

### New Features
- ✅ **Email Confirmation Simulation** - Booking confirmation emails are logged
- ✅ **Rate Limiting** - Protection against spam on registration and contact forms
- ✅ **Enhanced Error Handling** - Consistent error handling across all controllers

### Technical Improvements
- ✅ **Timezone Fixes** - Fixed date handling to prevent timezone mismatches
- ✅ **Avatar Display Fixes** - Fixed duplicate avatar display in reviews
- ✅ **Date Validation** - Improved date validation for package bookings
- ✅ **Database Schema Updates** - Merged PromotionUsage into Booking, removed FavoriteRoomType feature
- ✅ **Email Integration** - Real email sending with MailKit for verification and password reset OTP
- ✅ **Schema Simplification** - Removed separate PromotionUsage table (tracking now in Booking), removed FavoriteRoomType feature, simplified Review model (linked to Booking only)

## 🧪 Testing Recommendations

### Security Testing
1. ✅ Test login with different user roles
2. ✅ Test registration and email verification flow
3. ✅ Test password reset functionality
4. ✅ Test login blocking (try wrong password 3 times)
5. ✅ Test authorization (try accessing admin pages as customer)

### Functionality Testing
1. ✅ Test room search and filtering with AJAX
2. ✅ Test booking creation and payment
3. ✅ Test QR code generation
4. ✅ Test receipt view/download
5. ✅ Test promotion code application
6. ✅ Test booking cancellation
8. ✅ Test image preview on profile upload
9. ✅ Test toast notifications
10. ✅ Test loading indicators during AJAX calls
11. ✅ Test rate limiting (try 3+ registrations quickly)
12. ✅ Test error pages (404, 403)

### Admin Testing
1. ✅ Test user management (CRUD operations)
2. ✅ Test hotel/room management
3. ✅ Test booking status updates
4. ✅ Test promotion management
5. ✅ Test review moderation

## 📝 Important Notes

- **Authentication**: Uses custom cookie-based authentication (NOT ASP.NET Core Identity)
- **Password Storage**: All passwords are hashed using BCrypt
- **Email Functionality**: Real email sending using MailKit (SMTP). Configure email settings in `appsettings.json` for email verification and password reset OTP
- **Email Verification**: Users receive a real verification email with a clickable link when registering
- **Password Reset**: Uses OTP (One-Time Password) sent via email - users receive a 6-digit code to reset their password
- **Database**: Uses file-based SQL Server Express (LocalDB)
- **Currency**: Displayed in RM (Malaysian Ringgit)
- **Soft Delete**: All entities support soft delete functionality
- **Admin Protection**: Main admin account (`admin@hotel.com`) cannot be deleted
- **Database Schema**: PromotionUsage merged into Booking table, FavoriteRoomType feature removed, Review linked to Booking only
- **Module Assignments**: Each of 4 team members is responsible for at least 2 core modules (see Key Modules section above)

## 🐛 Troubleshooting

### Database Connection Issues
- Ensure SQL Server Express or LocalDB is installed
- Check connection string in `appsettings.json`
- Verify SQL Server service is running

### Migration Issues
- Delete the database and let the application recreate it
- Or manually apply migrations: `dotnet ef database update`

### Port Already in Use
- Change the port in `Properties/launchSettings.json`
- Or stop the process using the port

## 📞 Support

For issues, questions, or contributions:
- Review the project documentation
- Check the code comments
- Contact the development team

## 📊 Database Models

This section explains each model in the system and their purpose.

### Recent Schema Changes

**Important**: The database schema has been updated with the following changes:

1. **PromotionUsage Merged into Booking**
   - The separate `PromotionUsages` table has been removed
   - Promotion usage tracking fields are now stored directly in the `Bookings` table:
     - `PromotionPhoneNumberHash` (encrypted phone number)
     - `PromotionCardIdentifier` (hashed card identifier)
     - `PromotionDeviceFingerprint` (device identifier)
     - `PromotionIpAddress` (IP address)
     - `PromotionUsedAt` (timestamp when promotion was used)

2. **Review Model Simplified**
   - Removed `UserId` field from `Reviews` table
   - Reviews are now linked only to `Booking`
   - User information is obtained from `Booking.UserId` (via `Review.Booking.User`)

3. **FavoriteRoomType Feature Removed**
   - The `FavoriteRoomTypes` table and feature have been completely removed
   - All related code, views, and functionality have been removed

**Migration Required**: Run `dotnet ef migrations add <MigrationName>` and `dotnet ef database update` to apply these schema changes.

### Core Models

#### **User**
Represents a user account in the system (admin, staff, or customer).
- **Purpose**: Stores user authentication information, profile data, and preferences
- **Key Fields**: Email, PasswordHash (BCrypt), Role (Admin/Manager/Staff/Customer), IsEmailVerified, IsActive
- **Special Features**: 
  - Phone numbers are encrypted using AES encryption
  - Supports soft delete
  - Tracks user preferences (language, theme, profile picture, bio)
- **Relationships**: Has many Bookings

#### **Hotel**
Represents a hotel property in the reservation system.
- **Purpose**: Stores hotel location information, contact details, and descriptive information
- **Key Fields**: Name, Address, City, Country, ContactNumber, ContactEmail, Description, ImageUrl
- **Relationships**: Has many RoomTypes

#### **RoomType**
Represents a type/category of room (e.g., "Standard Single", "Deluxe Double", "Executive Suite").
- **Purpose**: Defines the characteristics, pricing, and amenities for a category of rooms
- **Key Fields**: Name, Description, Occupancy (max guests), BasePrice (per night), HotelId
- **Relationships**: 
  - Belongs to one Hotel
  - Has many Rooms (physical rooms of this type)
  - Has many RoomImages
  - Has many Amenities (through RoomTypeAmenity)

#### **Room**
Represents a physical room in a hotel.
- **Purpose**: Tracks individual rooms and their current status
- **Key Fields**: RoomNumber (unique identifier), RoomTypeId, Status (Available/Occupied/UnderMaintenance/Cleaning)
- **Relationships**: 
  - Belongs to one RoomType
  - Has many Bookings (booking history)

#### **RoomImage**
Represents an image associated with a room type.
- **Purpose**: Stores multiple images per room type to showcase rooms to customers
- **Key Fields**: RoomTypeId, ImageUrl, Caption
- **Relationships**: Belongs to one RoomType

#### **Amenity**
Represents a facility or feature that can be associated with room types.
- **Purpose**: Defines amenities like "Free Wi-Fi", "Air Conditioning", "Flat-Screen TV", etc.
- **Key Fields**: Name, ImageUrl
- **Relationships**: Has many RoomTypes (through RoomTypeAmenity - many-to-many)

#### **RoomTypeAmenity**
Junction entity linking RoomTypes to Amenities (many-to-many relationship).
- **Purpose**: Associates amenities with room types
- **Key Fields**: RoomTypeId, AmenityId

### Booking Models

#### **Booking**
Represents a hotel room booking made by a user.
- **Purpose**: Stores booking details, payment information, cancellation data, and promotion usage tracking
- **Key Fields**: 
  - UserId, RoomId, CheckInDate, CheckOutDate, BookingDate
  - TotalPrice, Status (Pending/Confirmed/Cancelled/CheckedIn/CheckedOut/NoShow)
  - PaymentAmount, PaymentMethod (CreditCard/PayPal/BankTransfer), PaymentStatus, TransactionId, PaymentDate
  - CancellationDate, CancellationReason, RefundAmount
  - PromotionId (optional)
  - **Promotion Usage Tracking** (merged from PromotionUsage table):
    - PromotionPhoneNumberHash (encrypted phone number)
    - PromotionCardIdentifier (hashed card identifier)
    - PromotionDeviceFingerprint (device identifier)
    - PromotionIpAddress (IP address)
    - PromotionUsedAt (timestamp when promotion was used)
- **Special Features**: 
  - Payment information is merged into this entity (previously separate Payment table)
  - Cancellation information is merged into this entity (previously separate BookingCancellation table)
  - Promotion usage tracking is merged into this entity (previously separate PromotionUsage table)
  - Supports soft delete
- **Relationships**: 
  - Belongs to one User, one Room, and optionally one Promotion
  - Has many Reviews

#### **Promotion**
Represents a promotion code that can be applied to bookings for discounts.
- **Purpose**: Manages discount codes with validation rules and abuse prevention
- **Key Fields**: 
  - Code (e.g., "WELCOME10"), Description, Type (Percentage/FixedAmount), Value
  - StartDate, EndDate, IsActive
  - MinimumNights, MinimumAmount, MaxTotalUses
  - Abuse prevention settings: LimitPerPhoneNumber, LimitPerPaymentCard, LimitPerDevice, LimitPerUserAccount, MaxUsesPerLimit
- **Special Features**: 
  - Comprehensive validation rules (dates, minimum requirements, usage limits)
  - Multiple abuse prevention mechanisms
  - Automatically deactivates when expired or max uses reached
- **Relationships**: Has many Bookings (promotion usage tracking stored in Booking table)


### Service & Package Models

#### **Service**
Represents an additional service that can be purchased or included in packages.
- **Purpose**: Defines services like "Airport Transfer", "Breakfast Buffet", "Spa Treatment", etc.
- **Key Fields**: Name, Description, Price
- **Relationships**: Can be included in Packages (through PackageItem)

#### **Package**
Represents a package deal that bundles room types and services together at a discounted price.
- **Purpose**: Creates bundled offers (e.g., "Kuala Lumpur City Explorer", "Honeymoon Bliss")
- **Key Fields**: Name, Description, TotalPrice, IsActive, ImageUrl
- **Relationships**: Has many PackageItems (room types and services included in the package)

#### **PackageItem**
Junction entity linking Packages to RoomTypes and Services.
- **Purpose**: Defines what items (room types and services) are included in a package and their quantities
- **Key Fields**: PackageId, RoomTypeId (optional), ServiceId (optional), Quantity
- **Relationships**: Belongs to one Package, optionally one RoomType, and optionally one Service

### Review & Feedback Models

#### **Review**
Represents a review/rating submitted by a user for a completed booking.
- **Purpose**: Allows customers to rate their stay experience and provide feedback
- **Key Fields**: BookingId, Rating (1-5 stars), Comment, ReviewDate
- **Special Features**: 
  - Reviews are linked to specific bookings (only customers who have stayed can review)
  - User information is obtained from Booking.UserId (no direct UserId field)
  - Supports soft delete for review moderation
- **Relationships**: Belongs to one Booking (user info accessed via Booking.User)

#### **ContactMessage**
Represents a contact message submitted through the contact form.
- **Purpose**: Stores customer inquiries, feedback, and support requests
- **Key Fields**: Name, Email, Subject, Message, SentAt, IsRead
- **Special Features**: 
  - Tracks read/unread status for admin management
  - Supports soft delete
- **Relationships**: None (standalone entity)

#### **Newsletter**
Represents a newsletter subscription entry.
- **Purpose**: Stores email addresses of users subscribed to promotional emails and newsletters
- **Key Fields**: Email, SubscribedAt, IsActive
- **Special Features**: 
  - IsActive flag allows unsubscribing without deleting the record
  - Supports soft delete
- **Relationships**: None (standalone entity)

### Security Models

#### **SecurityToken**
Represents a security token used for password reset and email verification.
- **Purpose**: Provides time-limited, single-use tokens for secure operations
- **Key Fields**: TokenValue (unique token), UserId, Type (PasswordReset/EmailVerification), ExpiryDate, IsUsed
- **Special Features**: 
  - Tokens expire after a set period (typically 24 hours)
  - Tokens are single-use (cannot be reused after IsUsed = true)
- **Relationships**: Belongs to one User

#### **LoginAttempt**
Tracks login attempts for security and audit purposes.
- **Purpose**: Records all login attempts (successful and failed) for security monitoring
- **Key Fields**: Email, Timestamp, WasSuccessful, IpAddress
- **Special Features**: 
  - Used to implement account lockout after multiple failed attempts
  - Helps detect brute-force attacks
- **Relationships**: None (standalone entity)

#### **SecurityLog**
Represents a security event log entry for audit and monitoring.
- **Purpose**: Records security-related actions (login, logout, password changes, etc.)
- **Key Fields**: UserId (optional), Action, IPAddress, Timestamp, Details
- **Special Features**: 
  - Comprehensive security audit trail
  - Supports soft delete
- **Relationships**: Optionally belongs to one User

### User Preference Models


### View Models

#### **ErrorViewModel**
View model used for displaying error pages (404, 500, etc.).
- **Purpose**: Provides error information to error pages
- **Key Fields**: RequestId (for tracking and debugging)

### Database Context

#### **HotelDbContext**
Entity Framework Core database context for the application.
- **Purpose**: Manages database connections and entity configurations
- **Key Features**: 
  - Configures all entity relationships and constraints
  - Implements global query filters for soft delete (automatically filters out deleted records)
  - Prevents cascade deletes to avoid circular dependencies

### Model Relationships Summary

```
User
  ├── Bookings (1-to-many)
  └── SecurityTokens (1-to-many)

Hotel
  └── RoomTypes (1-to-many)

RoomType
  ├── Hotel (many-to-1)
  ├── Rooms (1-to-many)
  ├── RoomImages (1-to-many)
  └── Amenities (many-to-many via RoomTypeAmenity)

Room
  ├── RoomType (many-to-1)
  └── Bookings (1-to-many)

Booking
  ├── User (many-to-1)
  ├── Room (many-to-1)
  ├── Promotion (many-to-1, optional)
  ├── Reviews (1-to-many)
  └── Promotion Usage Tracking (stored directly in Booking):
      - PromotionPhoneNumberHash (encrypted)
      - PromotionCardIdentifier (hashed)
      - PromotionDeviceFingerprint
      - PromotionIpAddress
      - PromotionUsedAt

Promotion
  └── Bookings (1-to-many, promotion usage tracked in Booking table)

Package
  └── PackageItems (1-to-many)

PackageItem
  ├── Package (many-to-1)
  ├── RoomType (many-to-1, optional)
  └── Service (many-to-1, optional)
```

### Soft Delete Pattern

All major entities support **soft delete**:
- `IsDeleted` flag (boolean) - marks entity as deleted
- `DeletedAt` timestamp (nullable DateTime) - records when deletion occurred
- Global query filters automatically exclude soft-deleted records from queries
- Allows data recovery and maintains referential integrity

### Security Features

- **Password Hashing**: All passwords are hashed using BCrypt (never stored in plain text)
- **Data Encryption**: Phone numbers are encrypted using AES-256 encryption
- **Token Security**: Security tokens are time-limited and single-use
- **Audit Logging**: Security events are logged for compliance and monitoring

## 📄 License

This project is part of a BMIT2023 Web and Mobile Systems Assignment.

---

**Built with ❤️ using ASP.NET Core MVC**

