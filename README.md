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
- ✅ **Favorites/Wishlist** - Save favorite rooms for later
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
  }
}
```

### Important Configuration Notes

- **Connection String**: Update if using a different SQL Server instance
- **EncryptionKey**: Used for encrypting sensitive data (change in production!)
- **SecuritySettings**: Configure login attempt limits and lockout duration

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
   - Favorites/Wishlist feature
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
- ✅ **Favorites/Wishlist** - Users can save favorite rooms for later viewing
- ✅ **Email Confirmation Simulation** - Booking confirmation emails are logged
- ✅ **Rate Limiting** - Protection against spam on registration and contact forms
- ✅ **Enhanced Error Handling** - Consistent error handling across all controllers

### Technical Improvements
- ✅ **Timezone Fixes** - Fixed date handling to prevent timezone mismatches
- ✅ **Avatar Display Fixes** - Fixed duplicate avatar display in reviews
- ✅ **Date Validation** - Improved date validation for package bookings
- ✅ **Database Migration** - Added FavoriteRoomTypes table

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
7. ✅ Test favorites/wishlist feature
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
- **Email Functionality**: Email sending is simulated - confirmation emails are logged for demonstration purposes
- **Database**: Uses file-based SQL Server Express (LocalDB)
- **Currency**: Displayed in RM (Malaysian Ringgit)
- **Soft Delete**: All entities support soft delete functionality
- **Admin Protection**: Main admin account (`admin@hotel.com`) cannot be deleted
- **Favorites Feature**: Requires database migration - run `dotnet ef migrations add AddFavoriteRoomTypes` then `dotnet ef database update`
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

## 📄 License

This project is part of a BMIT2023 Web and Mobile Systems Assignment.

---

**Built with ❤️ using ASP.NET Core MVC**

