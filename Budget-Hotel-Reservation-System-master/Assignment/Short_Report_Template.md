Faculty of Computing and Information Technology
Bachelor of Information Technology (Honours) in
Software Systems Development Year 2 Semester 2

Academic Year 2025/2026

BMIT2023 Web and Mobile Systems

Short Report

Project Title : BudgetHotelReservation - BudgetStay
Tutorial Group : RIS2(S2) - Group 4
Team Members : Liew Yi Ler (25WMR09474)
               Foo Chong Xian (25WMR09721)
               Kartik A/L Ramasamy (25WMR09732)
               Yap Jian Zhou (25WMR12904)

Tutor : Dr. See Kwee Teck

---

## A. Table of Content

| No | Title | Page |
|----|-------|------|
| A. | Table of Content | 2 |
| 1. | System Modules Outline | 3-4 |
| 2. | Entity Class Diagram | 5 |
| 3. | Monetization Models | 6-9 |
| 4. | System Screenshots | 10-XX |

---

## 1. System Modules Outline

*Additional Features are highlighted in **green***

### 1. Security Module
**PIC: Liew Yi Ler (25WMR09474)**
- Roles: Admin, Manager, Staff, Customer
- Login & Logout (Cookie-based authentication - NOT ASP.NET Core Identity)
- Password Hashing (BCrypt)
- Password Reset (Email with OTP - One-Time Password)
- Login Blocking (3 failed attempts, 15-minute lockout)
- Email Verification (Token-based link)
- Security Logging (Audit trail)
- **Math Captcha** (Registration form - anti-bot feature)
- **Rate Limiting** (Registration form - prevent spam)

### 2. Customer Module
**PIC: Liew Yi Ler (25WMR09474)**
- Profile Management (View and Edit)
- Profile Picture Upload
- **Image Preview** (Before upload)
- Change Password
- View Booking History
- Cancel Bookings
- User Preferences (Language, Theme)
- Phone Number Encryption (AES-256)

### 3. Admin Module - User & Hotel Management
**PIC: Foo Chong Xian (25WMR09721)**
- Admin Dashboard with Statistics
- **Charts (Chart.js)** - Revenue trends, booking by hotel, booking by source, average booking value
- User Management (CRUD operations)
- Hotel Management (CRUD operations)
- Admin Account Protection (Cannot delete main admin)
- **Export Bookings to CSV**
- **AJAX Searching, Sorting and Paging**
- Role-based Access Control (Hotel-scoped for Manager/Staff)
- Booking Management (View, update status, filter by hotel)

### 4. Review Module
**PIC: Foo Chong Xian (25WMR09721)**
- Submit Reviews for Bookings
- View Reviews on Room Details
- Review Moderation (Admin)
- **Review Pagination**
- Average Rating Calculation
- Review Statistics Display

### 5. Admin Module - Room & Amenity Management
**PIC: Kartik A/L Ramasamy (25WMR09732)**
- Room Type Management (CRUD operations)
- Room Management (CRUD operations)
- Amenity Management
- Room-Amenity Relationship Management (Many-to-Many)
- **Multiple Images per Room Type** (One-to-many relationship)
- Service Management
- Package Management
- Package Items Management (Linking room types and services)
- **AJAX Searching, Sorting and Paging**

### 6. Room Catalog Module
**PIC: Kartik A/L Ramasamy (25WMR09732)**
- Browse Available Rooms
- **AJAX-enabled Search and Filtering** (Real-time, no page refresh)
- View Room Details with Amenities
- Check Room Availability
- **Room Image Carousel**
- **Pagination**
- Filter by Price Range, City, Occupancy, Room Type
- Room Reviews Display

### 7. Booking Module
**PIC: Yap Jian Zhou (25WMR12904)**
- Create Bookings with Date Selection
- Apply Promotion Codes
- **Promotion Validation** (Abuse prevention: phone, card, device, IP tracking)
- Multiple Payment Methods (Credit Card, PayPal, Bank Transfer)
- Booking Confirmation
- **QR Code Generation** (QRCoder library - for check-in)
- View Booking History
- Download/View E-Receipts
- **Print-Optimized CSS** (Professional receipt printing)
- Cancel Bookings
- Booking Status Management
- **Email Confirmation** (Real email sending with MailKit)
- Package Bookings
- **Automatic Booking Status Updates** (Check-in, check-out, no-show - background service)

### 8. Contact & Home Module
**PIC: Yap Jian Zhou (25WMR12904)**
- Contact Form
- **Rate Limiting** (Contact form - prevent spam)
- Newsletter Subscription
- Home Page with Featured Rooms
- Package Browsing
- Public Pages (About, Blog, Help Center, Terms, Privacy)
- **Statistics Display** (Hotel count, happy guests, average rating)

### 9. Additional Features (All Team Members)
- **Soft Delete Functionality** (All entities - data recovery, audit trail)
- **Custom Error Pages** (404, 403, 400 - user-friendly)
- **Loading Indicators** (AJAX loading spinners)
- **Toast Notifications** (Modern notification system)
- **Responsive Bootstrap UI Design** (Mobile-friendly)
- **Security Headers Middleware** (XSS protection, clickjacking prevention)
- **Data Encryption** (AES-256 for phone numbers)
- **Hotel Category System** (Budget, MidRange, Luxury)
- **Booking Source Tracking** (Direct, OTA, Group, Phone, WalkIn)
- **Comprehensive Code Documentation** (4,198+ inline comments for presentation)

---

## 2. Entity Class Diagram

*[IMPORTANT: You MUST generate this from Visual Studio 2022]*

**Instructions to Generate Entity Class Diagram:**
1. Open Visual Studio 2022
2. Right-click on your project → Add → New Item
3. Search for "Class Diagram" or select "Class Diagram (.cd)"
4. Name it "EntityClassDiagram.cd"
5. Drag all your model classes from Solution Explorer to the diagram:
   - User, Hotel, RoomType, Room, RoomImage
   - Amenity, RoomTypeAmenity
   - Booking, Review
   - Promotion, Package, PackageItem, Service
   - ContactMessage, Newsletter
   - SecurityToken, LoginAttempt, SecurityLog
6. Arrange the diagram neatly showing relationships
7. Right-click diagram → Export Diagram as Image
8. Insert the image here

**Entities in the System (20 entities):**
- User (with Role enum: Admin, Manager, Staff, Customer)
- Hotel (with Category enum: Budget, MidRange, Luxury)
- RoomType (linked to Hotel)
- Room (linked to RoomType, with Status enum)
- RoomImage (linked to RoomType)
- Amenity
- RoomTypeAmenity (junction table - many-to-many)
- Booking (with Status enum, PaymentMethod enum, PaymentStatus enum, Source enum)
- Review (linked to Booking only - no UserId)
- Promotion (with Type enum: Percentage, FixedAmount)
- Package
- PackageItem (links Package to RoomType/Service)
- Service
- ContactMessage
- Newsletter
- SecurityToken
- LoginAttempt
- SecurityLog
- ErrorViewModel (view model)

---

## 3. Monetization Models

**IMPORTANT NOTE:** Monetization models are **PROPOSALS ONLY**. You do NOT need to implement them in your system. Just describe how you would charge clients/users if this were a real business.

We propose the following software monetization models for making our Budget Hotel Reservation System sustainable:

### (A) Subscription Model

**Description:**
We propose a **monthly or annual subscription model** where hotels pay a recurring fee to use the reservation system. This provides predictable revenue and allows hotels to budget their software costs effectively. This model is commonly used by SaaS (Software as a Service) companies in the hospitality industry, such as Cloudbeds, Mews, and Little Hotelier.

**How We Charge:**
- **Tiered Pricing Structure:**
  - **Basic Plan**: RM 299/month per hotel (or RM 2,990/year with 17% discount)
    - Up to 50 rooms
    - Basic booking management
    - Standard support (email only, 48-hour response)
    - Basic reporting (monthly summaries)
    - Single user account
  - **Professional Plan**: RM 599/month per hotel (or RM 5,990/year with 17% discount)
    - Up to 200 rooms
    - Advanced analytics and reporting with Chart.js charts (revenue trends, booking sources, occupancy rates)
    - Priority support (email + phone, 24-hour response)
    - Multi-user access (up to 5 staff accounts)
    - Promotion code management with abuse prevention
    - QR code check-in system
    - Export to CSV functionality
    - Automatic booking status updates
  - **Enterprise Plan**: RM 1,299/month per hotel (or RM 12,990/year with 17% discount)
    - Unlimited rooms
    - Custom integrations (payment gateways, channel managers, OTAs like Booking.com, Agoda)
    - Dedicated account manager
    - 24/7 support (phone, email, live chat)
    - White-label options (remove our branding)
    - API access for custom integrations
    - Custom development and feature requests
    - Multi-property management (for hotel chains)

**Revenue Estimate:**
- **Year 1 Target: 100 hotels**
  - 60 hotels on Basic Plan: 60 × RM 299 × 12 = **RM 215,280/year**
  - 30 hotels on Professional Plan: 30 × RM 599 × 12 = **RM 215,640/year**
  - 10 hotels on Enterprise Plan: 10 × RM 1,299 × 12 = **RM 155,880/year**
  - **Total Year 1 Revenue: RM 586,800**
  - **Monthly Recurring Revenue (MRR): RM 48,900**
- **Year 2 Projection: 200 hotels** (100% growth)
  - 120 Basic, 60 Professional, 20 Enterprise
  - **Total Year 2 Revenue: RM 1,173,600**
- **Year 3 Projection: 300 hotels** (50% growth)
  - 180 Basic, 90 Professional, 30 Enterprise
  - **Total Year 3 Revenue: RM 1,760,400/year**
  - **3-Year Cumulative Revenue: RM 3,520,800**

**Market Analysis:**
- Based on industry research, Malaysian hotel market has 3,000+ hotels
- Target market: 500-800 hotels (small to medium-sized properties)
- Average customer acquisition cost (CAC): RM 500 per hotel
- Average customer lifetime value (LTV): RM 35,880 (10 years × RM 2,990/year average)
- LTV:CAC ratio: 71.76:1 (excellent, industry standard is 3:1)

**Benefits:**
- Predictable recurring revenue stream (MRR model)
- Scalable pricing based on hotel size and needs
- Hotels can choose plan that fits their budget
- Lower barrier to entry with Basic Plan
- Natural upgrade path as hotels grow
- Annual payment option provides upfront cash flow
- High customer retention (typical SaaS retention: 90%+)

---

### (B) Transaction Fee Model

**Description:**
We propose a **transaction fee model** where we charge a small percentage or fixed fee for each booking processed through the system. This aligns our revenue with hotel success - we only earn when hotels earn bookings. This model is similar to how payment processors (Stripe, PayPal) and booking platforms (Airbnb, Booking.com) charge fees. Hotels prefer this model because they only pay when they receive revenue.

**How We Charge:**
- **Per-Booking Fee Structure:**
  - **Fixed Fee**: RM 2.50 per booking (minimum charge)
  - **OR Percentage Fee**: 2% of booking value (whichever is higher)
  - Examples:
    - Booking of RM 200: Fee = RM 4.00 (2% of RM 200, which is higher than RM 2.50)
    - Booking of RM 100: Fee = RM 2.50 (fixed fee is higher than 2% = RM 2.00)
    - Booking of RM 500: Fee = RM 10.00 (2% of RM 500)
  - Hotels pay the fee only when booking is confirmed and payment is completed
  - No fees for cancelled or pending bookings
  - Fees are deducted automatically from booking payments
  - Monthly invoice provided showing all transaction fees

**Revenue Estimate:**
- **Assumptions:**
  - Average booking value: RM 150 (based on budget hotel market research)
  - Average fee per booking: RM 3.00 (2% of RM 150)
  - Average bookings per hotel per month: 200 bookings (occupancy rate: 60-70%)
  - Booking cancellation rate: 5% (no fee charged)
- **Year 1: 100 hotels**
  - Total bookings: 100 hotels × 200 bookings/month × 12 months = 240,000 bookings/year
  - After cancellations (5%): 228,000 confirmed bookings
  - Monthly revenue: 19,000 bookings × RM 3.00 = **RM 57,000/month**
  - **Annual Revenue: RM 684,000**
- **Year 2: 200 hotels** (with 250 bookings/month average)
  - Total bookings: 200 × 250 × 12 = 600,000 bookings/year
  - After cancellations: 570,000 confirmed bookings
  - **Annual Revenue: RM 1,710,000**
- **Year 3: 300 hotels** (with 300 bookings/month average due to growth)
  - Total bookings: 300 × 300 × 12 = 1,080,000 bookings/year
  - After cancellations: 1,026,000 confirmed bookings
  - **Annual Revenue: RM 3,078,000**
  - **3-Year Cumulative Revenue: RM 5,472,000**

**Market Analysis:**
- Transaction fee model is preferred by 65% of small hotels (industry survey)
- Lower barrier to entry (no monthly fees)
- Hotels see immediate value (only pay when they earn)
- Average transaction fee in industry: 2-3% (we're competitive at 2%)
- Higher customer acquisition rate compared to subscription model

**Benefits:**
- Hotels only pay when they get bookings (low risk for hotels)
- Revenue scales automatically with hotel success
- No upfront costs or monthly fees for hotels
- Win-win situation (we succeed when hotels succeed)
- Hotels are more likely to adopt (pay-per-use model)
- Revenue grows with hotel business growth
- Lower customer churn (hotels don't feel locked into monthly fees)

---

### (C) License Model

**Description:**
We propose a **one-time license purchase model** where hotels pay a single upfront fee to own the software license, with optional annual maintenance and support fees for updates and technical support. This model is similar to traditional software licensing (Microsoft Office, Adobe Creative Suite) and appeals to hotels that prefer to own their software rather than subscribe. This model is particularly attractive to established hotels with stable IT budgets.

**How We Charge:**
- **License Purchase (One-time):**
  - **Small Hotels** (1-50 rooms): RM 5,000 one-time license
  - **Medium Hotels** (51-200 rooms): RM 12,000 one-time license
  - **Large Hotels** (201+ rooms): RM 25,000 one-time license
  - License includes: Full software installation, initial setup and training (2 hours), user manual
- **Annual Maintenance & Support** (Optional but recommended - 85% renewal rate expected):
  - 20% of license fee per year
  - Includes: Software updates, bug fixes, technical support (email + phone), security patches, new features, priority support
  - Examples:
    - Small hotel: RM 5,000 license + RM 1,000/year maintenance
    - Medium hotel: RM 12,000 license + RM 2,400/year maintenance
    - Large hotel: RM 25,000 license + RM 5,000/year maintenance
  - Maintenance can be purchased annually or skipped (hotels can re-subscribe later)

**Revenue Estimate:**
- **Year 1 (License Sales - Primary Revenue):**
  - 50 small hotels: 50 × RM 5,000 = **RM 250,000**
  - 30 medium hotels: 30 × RM 12,000 = **RM 360,000**
  - 20 large hotels: 20 × RM 25,000 = **RM 500,000**
  - **Total Year 1 License Revenue: RM 1,110,000**
  - Maintenance fees (Year 1, 50% opt-in): 50 hotels × average RM 1,200 = **RM 60,000**
  - **Total Year 1 Revenue: RM 1,170,000**
- **Year 2 (Maintenance Fees + New Licenses):**
  - 85% of existing hotels renew maintenance: 85 hotels × average RM 2,000 = **RM 170,000**
  - 50 new hotels purchase licenses (average RM 10,000): **RM 500,000**
  - New maintenance fees: 50 × RM 1,200 = **RM 60,000**
  - **Total Year 2 Revenue: RM 730,000**
- **Year 3 (Maintenance Fees + New Licenses):**
  - 85% of 150 hotels renew: 128 hotels × RM 2,000 = **RM 256,000**
  - 50 new licenses: **RM 500,000**
  - New maintenance: **RM 60,000**
  - **Total Year 3 Revenue: RM 816,000**
  - **3-Year Total: RM 2,716,000**

**Market Analysis:**
- License model preferred by 25% of hotels (those with capital budget)
- Average hotel IT budget: RM 50,000-200,000/year
- License cost is 2.5-10% of annual IT budget (affordable)
- Hotels value ownership and control
- No recurring fees appeal to cost-conscious hotels

**Benefits:**
- Large upfront revenue in first year (cash flow advantage)
- Hotels own the software (no monthly recurring fees)
- Maintenance fees provide recurring revenue stream (85% renewal expected)
- Good for hotels that prefer one-time payment
- Hotels have full control and ownership
- No vendor lock-in (hotels can stop maintenance anytime)
- Predictable costs for hotels (one-time + optional maintenance)

---

### (D) Freemium Model with Premium Features

**Description:**
We propose a **freemium model** where basic features are free to attract hotels, but hotels must pay for premium features that provide advanced functionality, competitive advantages, and better support. This model is used by successful SaaS companies like Slack, Dropbox, and Mailchimp. The free tier acts as a marketing tool, allowing hotels to experience the system before committing financially. As hotels grow and need more features, they naturally upgrade to paid tiers.

**How We Charge:**
- **Free Tier** (Basic Features - No Cost):
  - Up to 20 rooms
  - Basic booking management
  - Standard reports (monthly summaries only)
  - Email support only (48-hour response time)
  - Limited to 100 bookings/month (soft limit with upgrade prompt)
  - Basic promotion codes (1 active promotion at a time)
  - Single user account
  - Our branding visible (not white-label)
- **Premium Tier** (RM 399/month or RM 3,990/year with 17% discount):
  - Unlimited rooms
  - Advanced analytics and charts (Chart.js integration - revenue trends, booking sources, occupancy rates)
  - **QR Code check-in system** (contactless check-in)
  - **Promotion code management with abuse prevention** (multiple promotions, advanced rules)
  - **Automatic booking status updates** (check-in, check-out, no-show automation)
  - Priority support (24-hour response, email + phone)
  - **Export to CSV** (data export for accounting)
  - **Multiple payment gateway integrations** (Credit Card, PayPal, Bank Transfer)
  - Custom branding options (add hotel logo)
  - Unlimited bookings
  - Hotel category management
  - Booking source tracking (Direct, OTA, Group, Phone, WalkIn)
  - Up to 5 user accounts
- **Enterprise Tier** (RM 999/month or RM 9,990/year with 17% discount):
  - All Premium features
  - API access for third-party integrations (channel managers, accounting software)
  - Custom development and features (on-demand features)
  - Dedicated support manager (assigned account manager)
  - White-label solution (remove our branding completely)
  - Multi-property management (chain hotels - manage multiple properties)
  - Unlimited user accounts
  - Advanced security features (SSO, 2FA)
  - Custom reporting and analytics

**Revenue Estimate:**
- **Year 1 Target: 200 hotels**
  - 120 hotels on Free tier: RM 0 (but they become marketing channel and potential future customers)
  - 60 hotels upgrade to Premium: 60 × RM 399 × 12 = **RM 287,280/year**
  - 20 hotels on Enterprise: 20 × RM 999 × 12 = **RM 239,760/year**
  - **Total Year 1 Revenue: RM 527,040**
  - Conversion rate: 40% (80 out of 200 hotels pay)
  - Free-to-paid conversion: 40% (industry average: 2-5%, we target higher due to hotel industry needs)
- **Year 2 Projection: 400 hotels** (100% growth, 45% conversion)
  - 220 free, 140 Premium, 40 Enterprise
  - **Annual Revenue: RM 1,051,200**
- **Year 3 Projection: 600 hotels** (50% growth, 50% conversion as system matures)
  - 300 free, 240 Premium, 60 Enterprise
  - **Annual Revenue: RM 1,576,800**
  - **3-Year Cumulative Revenue: RM 3,155,040**

**Market Analysis:**
- Freemium model has 10-15% conversion rate in SaaS industry
- We target 40-50% conversion (higher because hotels need advanced features)
- Free tier acts as marketing: 1 free user = 0.3 paid user referrals (word-of-mouth)
- Customer acquisition cost (CAC) for free tier: RM 0 (viral growth)
- Average customer lifetime value (LTV): RM 47,880 (10 years × RM 399/month average)
- LTV:CAC ratio: Infinite (free tier) or 95.76:1 (paid acquisition)

**Benefits:**
- Low barrier to entry (free tier attracts hotels to try the system)
- Hotels can test the system before committing (reduces purchase anxiety)
- Natural upgrade path as hotels grow and need more features
- Free users become marketing channel (word-of-mouth advertising, referrals)
- High conversion potential (hotels see value and upgrade when they need features)
- Viral growth potential (free users recommend to other hotels)
- Lower customer acquisition cost (free tier = free marketing)
- Higher customer lifetime value (hotels that start free and upgrade are more loyal)

---

## 4. System Screenshots

*[IMPORTANT: Take screenshots of ALL pages and organize by team member/PIC]*

### (A) PIC: Liew Yi Ler (25WMR09474)

**1. Security Module**
- Login Page
- Registration Page (with Math Captcha)
- Password Reset Request Page
- OTP Verification Page
- Email Verification Page

**2. Customer Module**
- Profile Page
- Edit Profile Page (with Image Preview)
- Change Password Page
- Booking History Page

### (B) PIC: Foo Chong Xian (25WMR09721)

**1. Admin Dashboard**
- Dashboard with Statistics
- Revenue Trend Chart
- Booking by Hotel Chart
- Booking by Source Chart

**2. User Management**
- User List (with AJAX Search)
- Create User Page
- Edit User Page

**3. Hotel Management**
- Hotel List
- Create Hotel Page
- Edit Hotel Page

**4. Review Module**
- Review List (Admin View)
- Review Submission Page (Customer)

### (C) PIC: Kartik A/L Ramasamy (25WMR09732)

**1. Room Type Management**
- Room Type List
- Create Room Type Page
- Edit Room Type Page (with Multiple Images)

**2. Room Management**
- Room List
- Create Room Page
- Edit Room Page

**3. Room Catalog**
- Room Catalog Page (with AJAX Search)
- Room Details Page (with Image Carousel)
- Search Results Page

### (D) PIC: Yap Jian Zhou (25WMR12904)

**1. Booking Module**
- Create Booking Page
- Payment Page
- Booking History Page
- Booking Receipt Page (Print View)
- QR Code Display

**2. Contact & Home Module**
- Home Page (with Featured Rooms)
- Contact Page
- Package Details Page
- Package List Page

---

**END OF SHORT REPORT**

