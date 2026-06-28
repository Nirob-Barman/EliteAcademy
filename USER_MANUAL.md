# EliteAcademy — User Manual

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Browsing Classes](#2-browsing-classes)
3. [Enrolling in a Class](#3-enrolling-in-a-class)
4. [Promo Codes & Offers](#4-promo-codes--offers)
5. [My Cart](#5-my-cart)
6. [My Enrolled Classes](#6-my-enrolled-classes)
7. [Wishlist](#7-wishlist)
8. [Reviews & Q&A](#8-reviews--qa)
9. [Notifications](#9-notifications)
10. [Account Settings](#10-account-settings)
11. [Become an Instructor](#11-become-an-instructor)
12. [Instructor Guide](#12-instructor-guide)
13. [Admin Guide](#13-admin-guide)

---

## 1. Getting Started

### Register

1. Click **Register** in the top navigation bar.
2. Fill in your first name, last name, email, and password.
3. Agree to the Terms and Conditions and click **Register**.
4. You are logged in immediately as a **Student** and redirected to the Student dashboard.

> **Want to teach?** All new accounts start as Student. Once registered, you can apply to become an Instructor from your dashboard — see [Section 11](#11-become-an-instructor).

### Login

1. Click **Login** in the top navigation bar.
2. Enter your email and password, then click **Sign In**.
3. You are redirected to your role's dashboard automatically.
4. Use the **show/hide** eye button to toggle password visibility.

> **Banned accounts:** If your account has been suspended by an admin, you will see an error toast and cannot log in.

### Forgot Password

1. On the Login page, click **Forgot password?** (top-right of the password field).
2. Enter your registered email address and click **Send Reset Link**.
3. A success toast confirms the email was sent (shown regardless of whether the email exists, for security).
4. Check your inbox for a branded email with a **Reset Password** button.
5. Click the button, enter your new password and confirm it, then click **Reset Password**.
6. On success you are redirected to Login with a confirmation toast.

### Confirmation Dialogs

Destructive or irreversible actions throughout the platform (approving, rejecting, banning, deleting) show a styled **SweetAlert2** confirmation popup before proceeding. Click **Cancel** to abort or the action button to confirm. Browser-native `confirm()` popups are never used.

### Post-Login Redirect

After a successful login, the app sends you to the correct area based on your role:

| Role | Destination |
|---|---|
| Admin | `/Admin/Dashboard` |
| Instructor | `/Instructor/Dashboard` |
| Student | `/Student/Dashboard` |

---

## 2. Browsing Classes

### Homepage (`/`)

The public homepage showcases the platform to visitors and logged-in users alike:

- **Hero banner** with "Browse Classes" and "Get Started" calls-to-action
- **Promo banner** — a dark rotating strip showing active discount codes (rotates every 4 seconds when 2+ codes exist); includes a "View All Offers →" link
- **Stats bar** — platform-wide totals (students, classes, instructors)
- **Available Classes** — up to 6 class cards with star ratings, seat availability, and price; "View All" link when more exist
- **Meet Our Instructors** — up to 4 instructor cards showing name, photo, class count, and student count; "View All Instructors →" links to `/Instructors`
- **Why Choose Us** section
- **Testimonials** — recent student reviews
- **Book Now** CTA section
- **404 / Not Found** page for unknown URLs

### Instructors (`/Instructors`)

Browse all instructors on the platform. Each card shows the instructor's name, photo (or initial avatar), and badges for number of classes and enrolled students.

### About (`/About`)

- Mission & Vision cards
- Live platform stats: active students, instructors, classes, and enrollments
- Feature overview: Expert-Led Classes, Flexible Enrollment, Progress Dashboard, Secure Payments, Q&A, Reviews
- Links to Browse Classes and Meet Our Instructors

### All Classes (`/Classes`)

All approved classes are listed on the main class index. A filter bar at the top lets you narrow results by:

- **Search** — class name keyword
- **Instructor** — filter by a specific instructor
- **Min / Max Price** — price range

Each card shows:

- Class image
- Class name and instructor name
- Star rating average and review count (if any reviews exist)
- Price
- Available seats remaining (shown in red when 0)
- **Select** button — adds the class to your cart (requires login as Student)
- **Wishlist** (heart) button — saves the class for later (requires login as Student)

Browsing requires no login — only selecting, enrolling, and wishlisting require a Student account.

**Pagination:** Classes are shown 12 per page. Use the page controls at the bottom to navigate. All filtering and pagination happens instantly in the browser with no page reload.

---

## 3. Enrolling in a Class

Enrollment is a two-step process: **Select → Pay**.

### Step 1 — Select a Class (Pre-Enroll)

1. On the class listing page, click **Select** on the class you want.
2. A **PreEnrollment** record is created with `PaymentStatus = Pending`.
3. You are redirected to **My Cart** (`/Student/Cart`).

> You can select multiple classes before paying. Each selected class is independent.

### Step 2 — Pay and Enroll

1. In **My Cart**, click **Pay Now** next to the class.
2. You are taken to the **Checkout** page showing the class, price, and a promo code field.
3. Optionally enter a promo code and click **Apply** to see the discounted price.
4. Select an available payment gateway (configured by the admin).
5. Click **Pay** — you are redirected to the gateway's payment page.
6. Complete payment on the gateway.
7. On success, you are returned to the app:
   - `PaymentStatus` is set to `Paid`
   - An **Enrollment** record is created
   - **Available seats** on the class are decremented by 1
   - You receive an in-app notification

### Payment Cancellation

If you cancel on the gateway, you are redirected to `/Payment/Cancel`. The PreEnrollment remains in `Pending` status — you can retry payment from **My Cart**.

---

## 4. Promo Codes & Offers

### Applying a Promo Code

- Enter the code in the **Promo Code** field on the Checkout page and click **Apply**.
- The discount percentage is applied to the class price.
- The discounted final price is shown before you confirm payment.

### Offers Page (`/Offer`)

- Publicly accessible — no login required.
- Lists all currently active promo codes.
- A rotating banner on the homepage cycles through active codes every 4 seconds with a **View All Offers** link when 2 or more codes are active.

---

## 5. My Cart

Navigate to **My Cart** via the student sidebar or `/Student/Cart`.

- Lists all classes you have selected but not yet paid for (`PaymentStatus = Pending`).
- Shows a thumbnail, class name, instructor, base price, and a coupon badge (e.g. `WELCOME10 −10%`) when a discount code has been applied.
- An **Order Summary** footer at the bottom shows total item count, total savings, and grand total.
- Each row has a **Pay Now** button to proceed to checkout.
- Use the trash icon button to remove a selection — this cancels the pre-enrollment at no charge.
- **Empty state:** if no selections exist, a cart icon is shown with a "Browse Classes" button to start shopping.

---

## 6. My Enrolled Classes

Navigate to **My Enrolled Classes** via the student sidebar or `/Student/EnrolledClasses`.

- Lists all classes you have successfully paid for (`PaymentStatus = Paid`) in a table view.
- Each row shows a 64×48 thumbnail (or a placeholder icon when no image is set), class name, instructor, price, and enrollment date.
- Once enrolled, your seat is confirmed and the available seat count for that class decreases.

Each row has per-class action buttons:

| Button | Action |
|---|---|
| **Q&A** | Open the Q&A thread for that class |
| **News** | Read announcements posted by the instructor |
| **Review** | Leave a star rating and comment |
| **Reviewed ✓** | Green badge shown once you have submitted a review (replaces the Review button) |

**Empty state:** if you have no enrollments, a graduation cap icon is shown with a "Browse Classes" button.

---

## 7. Wishlist

### Adding to Wishlist

- Click the **heart (♥)** button on any class card on the class listing page.
- The button turns solid yellow and is disabled — each class can only be wishlisted once.
- A confirmation toast appears immediately (no page reload).

### My Wishlist (`/Student/Wishlist`)

- Navigate via the **Wishlist** link in the student sidebar.
- Shows saved classes as cards (`border-0 shadow-sm`) with a 160px thumbnail (or a chalkboard-teacher placeholder icon), class name, instructor, available seat count, and price.
- **Add to Cart** — selects the class for payment. Only shown when seats are available; disabled and replaced with "Unavailable" when the class is full.
- **Remove** (broken-heart icon) — removes the class from your wishlist without affecting any enrollment.
- **Empty state:** if your wishlist is empty, a heart icon is shown with a "Browse Classes" button.

---

## 8. Reviews & Q&A

### Leaving a Review

1. Go to **My Enrolled Classes** and click the **Review** button on the class row.
2. Select a star rating (1–5) by clicking the stars.
3. Optionally write a comment.
4. Click **Submit Review**.
   - You can only submit one review per class.
   - Once submitted, the button in **My Enrolled Classes** changes to **Reviewed ✓**.
   - Your rating contributes to the class's average star display on the class listing.

> You must be enrolled in a class to leave a review.

### Q&A

Q&A is per-class and requires enrollment. Only the class instructor can post answers.

**Asking a question (Student):**

1. Go to **My Enrolled Classes** and click **Q&A** on the class row.
2. Type your question in the text field and click **Submit**.
3. Your question appears in the thread. If the instructor has answered, the answer is shown below it indented in green.
4. You can delete your own question with the ✕ button.

**Answering questions (Instructor):**

1. In **My Classes**, click **Q&A** on an approved class row.
2. Each unanswered question has an inline answer field — type an answer and click **Post Answer**.
3. Instructors can delete any question or answer using the ✕ / Delete button.

---

## 9. Notifications

### Notification Bell

- A bell icon in the navigation bar shows your unread count.
- Click the bell to open the **Notifications** page.
- Opening the notifications page marks all unread notifications as read automatically.
- You can also mark individual notifications read via **Mark as Read**, or clear all with **Mark All as Read**.

### What triggers notifications

| Event | In-app | Email |
|---|---|---|
| Class you selected is approved | Yes | — |
| Class you selected is rejected | Yes | — |
| Payment confirmed / enrollment created | Yes | — |
| Announcement posted by your instructor | Yes | — |
| Instructor application approved | Yes | Yes |
| Instructor application rejected | Yes | Yes |

---

## 10. Account Settings

Access account settings from the navigation:
- **Student** — sidebar bottom → **Profile** button, or topbar user dropdown → **Account Settings**
- **Instructor** — sidebar bottom or topbar dropdown → **Profile**
- **Admin** — topbar dropdown or sidebar bottom → **Profile**

All roles open the same dedicated settings area — a standalone page with its own top bar (showing a **← Dashboard** back-link and a logout button) and a left sidebar with four sections.

### Edit Profile (`/Account/Profile`)

- **Left sidebar** — your initial avatar, display name, and role badge.
- **Right form** — edit first name, last name, phone number, gender, date of birth, address, and profile photo.

To update:
1. Edit any fields you want to change.
2. Optionally upload a new profile photo — a live preview updates immediately.
3. Click **Save Changes**. A success toast confirms the update.

Your email address is shown but cannot be changed from this page.

### Change Password (`/Account/ChangePassword`)

1. Enter your **current password**.
2. Enter a **new password** (minimum 6 characters, must include one uppercase letter and one special character).
3. Re-enter the new password in **Confirm New Password**.
4. Click **Update Password**. A success toast confirms the change.

Use the eye icon on each field to show or hide the password as you type.

### Notification Preferences (`/Account/NotificationPreferences`)

Control which notifications you receive. Toggle each switch on or off and click **Save Preferences**.

**Email notifications:**

| Toggle | When it fires |
|---|---|
| Enrollment Confirmed | You successfully enroll in a class |
| Class Announcements | An instructor posts an announcement to your class |
| Class Status Updates | Your submitted class is approved or rejected (instructors) |
| Application Status | Your instructor application is reviewed |
| Password Changes | Your password is changed — security alert |

**In-app notifications:**

| Toggle | When it fires |
|---|---|
| Enrollment Confirmed | You successfully enroll in a class |
| Class Announcements | An instructor posts a new announcement |

All toggles default to **on** for new accounts.

### Login History (`/Account/LoginHistory`)

Displays the last 50 login attempts on your account — both successful logins and failed attempts.

Each row shows:
- **Date & Time** of the attempt
- **IP Address** the request came from
- **Browser / Device** detected from the user agent
- **Status** — green "Success" or red "Failed" badge (hover the failed badge to see the reason)

Review this page regularly. If you see logins from an IP address or location you don't recognise, change your password immediately.

---

## 11. Become an Instructor

All accounts start as Student. If you want to teach on Elite Academy, submit an instructor application for admin review.

### Apply (`/InstructorApplication/Apply`)

1. In the student sidebar, click **Become Instructor**.
2. If you have no active application, the application form opens.
3. Fill in:
   - **Area of Expertise** — your main subject area (e.g. Web Development, Photography)
   - **About You** — your background, experience, and qualifications (min. 50 characters)
   - **Why Do You Want to Teach?** — what value you'll bring to students (min. 50 characters)
4. Click **Submit Application**.

> You can only have one active application at a time. Re-applying is allowed after a rejection.

### My Application (`/InstructorApplication/MyApplication`)

After submitting, click **Become Instructor** in the sidebar to check your status.

| Status | Meaning |
|---|---|
| **Pending Review** | An admin has not yet reviewed your application |
| **Approved** | You are now an Instructor — log out and back in for the role to take effect |
| **Not Approved** | The admin rejected the application with written feedback |

- If approved: log out and log back in — your role will change to Instructor and you'll be redirected to the Instructor Dashboard.
- If rejected: the admin's feedback is shown. You can click **Re-apply** to submit a new application addressing the feedback.

---

## 12. Instructor Guide

Instructor accounts access a dedicated dashboard at `/Instructor/Dashboard`.

### Dashboard (`/Instructor/Dashboard`)

- KPI cards: total classes you have created, total students enrolled across your classes, pending class approvals.

### Add a Class (`/InstructorClass/Create`)

1. Click **New Class** in the sidebar or **Add New Class** on the dashboard.
2. Fill in the class name, description, price, and available seats.
3. Upload a class image (shown on the class card and listing page).
4. Click **Submit** — the class is saved with status `Pending`.
5. The class will not appear publicly until an Admin **approves** it.

### My Classes (`/InstructorClass/Index`)

- Lists all classes you have created with their current status (Pending / Approved / Rejected).
- **Status badges:**
  - `Pending` — awaiting admin review
  - `Approved` — live and visible to students
  - `Rejected` — not visible; admin feedback is shown on the row
- Click **Edit** to update a class (name, image, price, seats).
- If a class was rejected, review the admin feedback, make corrections, and resubmit by editing and saving.
- Approved classes show additional action buttons: **Students**, **Q&A**, **Announcements**.

### Edit a Class (`/InstructorClass/Edit/{id}`)

- All fields are pre-filled with current values.
- Upload a new image to replace the existing one, or leave the image field blank to keep it.

### Students (`/InstructorClass/Students/{id}`)

- View the full list of students enrolled in a specific approved class.
- Shows each student's name, email, and enrollment date.
- **Export CSV** — downloads the student list as a `.csv` file.

### Q&A (`/InstructorClass/Qa/{id}`)

- View all questions students have asked about the class.
- Each question shows the student's name and the date asked.
- Type an answer in the inline field and click **Post Answer**.
- Delete any question (and all its answers) or individual answers using the ✕ button. Deleting a question shows a confirmation dialog before proceeding.
- Access from **My Classes** → **Q&A** button on an approved class row.

### Announcements (`/InstructorClass/Announcements/{id}`)

- Post a title and message to all students enrolled in the class.
- Click **Post to All Students** — enrolled students receive an in-app notification.
- All previously posted announcements are listed below the form with a **Delete** button. Deleting shows a confirmation dialog before proceeding.
- Access from **My Classes** → **Announcements** button on an approved class row.

### Account Settings (`/Account/Profile`)

- Edit profile, change password, manage notification preferences, and review login history.
- See [Section 10](#10-account-settings) for full details.

---

## 13. Admin Guide

Admin accounts have access to a full management panel. Navigate to `/Admin/Dashboard` or use the **Admin** sidebar.

### Dashboard (`/Admin/Dashboard`)

- KPI cards: total users, total instructors, total students, total classes.
- Counts for pending, approved, and rejected classes.
- **Pending instructor applications alert** — a yellow banner appears when there are unreviewed applications, with a direct **Review Now** link.
- Quick links to manage classes, users, and instructor applications.

### Instructor Applications (`/Admin/InstructorApplications`)

Review applications from students who want to become instructors.

- Summary badges at the top show the total pending / approved / rejected counts across all pages.
- Applications are listed 15 per page — use the page controls at the bottom to navigate.
- Each row shows: applicant name, email, expertise, submission date, and status.
- **View** — opens a modal with the full application (bio, expertise, motivation, admin notes).
- **Approve** — shows a confirmation dialog; on confirm, changes the applicant's role to Instructor immediately and sends an email and in-app notification.
- **Reject** — opens a modal requiring a written reason; sends an email and in-app notification with the feedback.

> Approved and rejected rows are highlighted green/red. Only Pending applications can be acted on.

### Class Management (`/Admin/Classes`)

View all classes submitted by instructors. Classes are listed 15 per page — use the page controls at the bottom to navigate.

| Column | Description |
|---|---|
| Image | Thumbnail |
| Class Name | Name of the class |
| Instructor | Who created it |
| Seats | Available seats remaining |
| Price | Class price |
| Status | Pending / Approved / Rejected badge |

**Actions:**

- **Approve** — shows a confirmation dialog; on confirm, makes the class live and visible to students and sends an email and in-app notification to the instructor.
- **Reject** — opens a modal where you must enter feedback for the instructor; sends an email and in-app notification.
- **Enrollments** — view the full list of enrolled students for that class.

### Class Enrollments (`/Admin/ClassEnrollments/{id}`)

- Shows all students enrolled in a specific class: name, email, and enrollment date.
- **Export CSV** button downloads the enrollment list as a `.csv` file for offline use.

### Student Management (`/Admin/Students`)

- Lists all registered students with their enrollment count and account status, 15 per page. Use the page controls at the bottom to navigate.
- **Status badges:** Active (green) or Banned (red); banned rows are highlighted.
- **Ban** — shows a confirmation dialog; on confirm, suspends the student's account immediately so they cannot log in.
- **Unban** — shows a confirmation dialog; on confirm, restores access.

> Banning uses ASP.NET Identity's lockout mechanism — the ban takes effect on the student's next login attempt.

### User Management (`/Admin/Users`)

- Lists all platform users (Admin, Instructor, Student) with their current role, 15 per page. Use the page controls at the bottom to navigate.
- **Change Role** — reassign a user between Instructor and Student. Admin accounts are protected and cannot be changed here.

### Revenue Report (`/Admin/RevenueReport`)

Analyse platform revenue for any year.

**Year selector** — click a year button (current year back 4 years) to switch the report.

**KPI cards:**
- Total Revenue for the selected year
- Total successful payment transactions
- Number of classes with revenue
- Number of active instructors with earnings

**Monthly Breakdown** — table with an inline progress bar showing revenue and transaction count for each month of the year.

**Revenue by Class** — ranked table showing which classes generated the most revenue and how many students enrolled.

**Revenue by Instructor** — ranked table showing which instructors generated the most revenue.

**Export CSV** — downloads a multi-section `.csv` file covering the monthly breakdown, by-class, and by-instructor summaries.

**Print** — launches the browser's print dialog with sidebar and header hidden for a clean report printout.

### Coupon Management (`/Coupon`)

- Create discount coupons with a code, discount percentage, max usage count, and expiry date.
- Toggle coupons active/inactive.
- Edit or delete existing coupons. Deleting shows a confirmation dialog before proceeding.
- Active coupons appear on the public Offers page (`/Offer`) and in the homepage rotating banner.

### Payment Gateway Management (`/PaymentGateway`)

Add, edit, toggle sandbox/live mode, or delete payment gateways. Deleting shows a confirmation dialog — gateways with existing transactions cannot be deleted.

Supported processors: **Stripe**, **SSLCommerz**, **bKash**, **SurjoPay**, and the built-in **Mock** gateway (instant success, for testing).

Gateway credentials are stored **encrypted** in the database using ASP.NET Data Protection. Leaving a secret field blank when editing keeps the existing stored credential.

**Adding a gateway:**

1. Go to **Payment Gateways** → **Add New Gateway**.
2. Select the **Gateway** family from the first dropdown (e.g. "Stripe", "bKash").
3. If the gateway has multiple integration methods, a second **Service Type** dropdown appears — select the variant (e.g. "Checkout", "Tokenized").
4. The **Display Name** field is auto-filled with a suggested name — you can customise it (e.g. "Stripe Checkout (Live)").
5. Fill in the credential fields that appear for your chosen gateway. Fields marked **secret** are masked with `•••` and stored encrypted.
6. Toggle **Sandbox / Test Mode** on if you want to use test credentials.
7. Toggle **Active** on to make the gateway available to students at checkout.
8. Click **Create Gateway**.

**Adding the Stripe gateway:**

1. Select **Stripe** → **Checkout** (service type).
2. Enter your **Secret Key** (`sk_live_...` for production, `sk_test_...` for sandbox).
3. Enter your **Publishable Key** (`pk_live_...` / `pk_test_...`).
4. Optionally enter your **Webhook Secret** (`whsec_...`) if you have configured Stripe webhooks.
5. Enable **Sandbox / Test Mode** when using test keys.
6. Save — students will be redirected to a Stripe-hosted payment page and returned to the platform on completion.

**Adding the Mock gateway (testing only):**

1. Select **Mock Gateway (Testing)** from the Gateway dropdown. No service type selection is needed.
2. The Display Name is auto-filled as "Mock Gateway (Testing)" — leave it or rename it.
3. No credential fields are required.
4. Save — students can complete payments instantly without a real card or gateway. Useful for development and demos.

**How the Stripe payment flow works for students:**

1. Student clicks **Pay Now** on a class in their cart.
2. On the checkout page, they select the Stripe gateway and click **Pay**.
3. They are redirected to a Stripe-hosted checkout page where they enter their card details.
4. On successful payment, Stripe redirects them back to the platform — enrollment is confirmed automatically.
5. If they cancel on the Stripe page, they are returned to the cancel page and can retry from **My Cart**.

### Audit Logs (`/AuditLogs`)

- Read-only log of all admin mutations.
- Each entry shows: entity type, action performed, admin email, timestamp, and a diff of old vs. new values.
- Filterable by entity type, action, and date range.

---

## Seeded Test Accounts

On first run, the following accounts are created automatically. Use them to explore each role.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@eliteacademy.com` | `Admin@123` |
| Instructor | `james@eliteacademy.com` | `Instructor@123` |
| Student | `alice@eliteacademy.com` | `Student@123` |

---

## Common Questions

**Why is my class still Pending?**
Classes require admin approval before they go live. An admin reviews submissions and either approves or rejects with feedback. Check the status on your **My Classes** page.

**I paid but I'm not enrolled — what happened?**
If the payment gateway redirected back without confirming, the transaction may still be Pending. Check **My Cart** — if the status is still Pending, retry payment. Contact support if the issue persists.

**I was approved as an Instructor but I still see the Student dashboard.**
Role changes take effect on your next login. Log out and log back in — you will be redirected to the Instructor Dashboard.

**My instructor application was rejected. Can I apply again?**
Yes. On the **My Application** page, your rejection reason is shown. Click **Re-apply** to submit a new application that addresses the admin's feedback.

**How do I remove a selected class I no longer want?**
Go to **My Cart** (`/Student/Cart`) and click the trash icon on the class row. This cancels the pre-enrollment without any charge.

**A student says they can't log in after a role change.**
Role changes and bans take effect on the user's next login. Ask them to log out and back in.
