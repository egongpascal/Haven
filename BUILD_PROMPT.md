# Haven Mobile App - Build Prompt

## Project Overview

**Haven** is a safety and emergency response mobile application that enables users to create safety groups, share real-time locations, and trigger emergency SOS alerts. The app connects to a .NET Core backend API using REST endpoints and SignalR for real-time communication.

### Purpose
- Enable users to form safety groups with friends and family
- Share real-time location updates within groups
- Trigger emergency SOS alerts with precise GPS coordinates
- Monitor geofence boundaries for group members
- Provide secure, privacy-focused location sharing

### Target Platforms
- **Primary**: iOS and Android (Flutter/Dart recommended)
- **Minimum SDK**: iOS 13+, Android API 21+ (Android 5.0+)

---

## Core Features

### 1. Authentication & User Management

#### Registration
- Create account with username, email, and password
- Optional first name and last name
- Automatic login after successful registration
- Returns JWT access token and refresh token

#### Login
- Username/password authentication
- JWT token-based authentication
- Refresh token mechanism for seamless sessions
- Token expiration: 1 hour (auto-refreshable)
- Biometric authentication support (Face ID/Touch ID/Fingerprint)
- "Remember me" option

#### User Profile
- **View Profile**: Display user information (name, email, profile image, verification status, privacy settings)
- **Update Profile**: Modify name, email, and profile picture
- **Change Password**: Secure password change with current password verification
- **Privacy Settings**: Toggle privacy mode (automatically enabled when in groups, disabled when not)

---

### 2. Group Management

#### Create Safety Groups
- Create private groups with custom names and descriptions
- Set geofence radius (10-5000 meters) for boundary monitoring
- Automatic invite code generation (6-character alphanumeric)
- Group creator automatically becomes admin
- Groups stored with relationships and member tracking

#### Join Groups
- Join groups using invite codes
- QR code scanning support for easy joining
- Preview group details before joining
- View group members and their details
- Automatic member relationship creation

#### Group Operations
- **List Groups**: View all groups user belongs to
- **Get Current Group**: Retrieve active group user is currently in
- **Group Details**: View group information, member count, geofence settings, creation date
- **Member Management**: View all members with their details (name, email, role, join date)
- **Update Group**: Modify group name (admin only)
- **Group Invite Codes**: Unique codes for easy group sharing

#### Group Roles & Permissions
- **Admin/Creator**: Can update group settings, set geofence, manage members
- **Member**: Can view group, share location, trigger SOS
- Role-based access control for group operations

---

### 3. Real-Time Location Tracking

#### Location Services
- **High-Accuracy GPS**: Precise location tracking (within 5-10 meters)
- **Background Tracking**: Continuous location updates even when app is in background
- **Update Frequency**: Every 5 meters moved or 10 seconds (whichever comes first)
- **Location History**: Stored with geospatial indexing for efficient queries
- **Permission Handling**: Request location permissions (Always Allow for background tracking)

#### Real-Time Location Sharing
- **SignalR Integration**: Live location sharing via WebSocket connections
- **Location Hub**: Connect to `/locationHub` for real-time updates
- **Join/Leave Groups**: Automatically join location groups when entering groups
- **Send Location**: Broadcast location updates to all group members
- **Receive Location**: Get real-time location updates from other members

#### Location Display
- **Map View**: 
  - Show all group members' locations on interactive map
  - Different colored markers/pins for each member
  - Real-time position updates
  - Show user's own location with blue dot
  - Show other members with colored pins
  
- **List View**:
  - List of members with distance from user
  - Last updated timestamp
  - Address/location name (reverse geocoding)
  - Member status indicators

#### Location Features
- **Address Resolution**: Reverse geocoding for user-friendly location names
- **Distance Calculation**: Real-time distance between members
- **Location Privacy**: Location only shared within active group
- **Privacy Toggle**: Automatically enabled when joining groups, disabled when leaving
- **Location History**: View location history (last 24 hours)

---

### 4. Emergency SOS System

#### Trigger Emergency Alert
- **SOS Button**: Large, prominent emergency button (always accessible on main screen)
- **One-Tap Activation**: Quick SOS trigger with confirmation dialog
- **Precise Coordinates**: Sends exact GPS coordinates (6 decimal places) for emergency services
- **Emergency Types**: Support for different emergency types (SOS, Medical, etc.)
- **Instant Notification**: Immediate alert to all group members
- **Multi-Channel Alerts**:
  - SignalR real-time push to all group members
  - SMS notifications (via backend service)
  - Email notifications (via backend service)

#### Emergency Request Details
- User ID, Group ID, Latitude, Longitude, Emergency Type, Timestamp
- Emergency records stored in database
- Status tracking (Active, Resolved)

#### Receive Emergency Alerts
- **SignalR Hub**: Connect to `/emergencyHub` for emergency notifications
- **SOS Alert Event**: Receive `SOSAlert` broadcasts with emergency details
- **Alert Display**:
  - Full-screen emergency notification
  - Map showing emergency location
  - User information (name, contact if available)
  - Navigation button (open in maps app)
  - Call emergency services button
  - Vibrate and sound alerts

#### Resolve Emergency
- Resolve emergency alerts (only emergency initiator or group admin)
- `SOSResolved` event via SignalR to notify all members
- Emergency status updated in database

#### Emergency Features
- **Location Accuracy**: High-precision GPS coordinates for emergency services
- **Address Display**: Human-readable address alongside coordinates
- **Emergency History**: Track past emergencies
- **Quick Response**: One-tap SOS activation
- **Confirmation Dialog**: Prevent accidental triggers
- **Copy Coordinates**: Copy precise coordinates to clipboard for sharing

---

### 5. Geofencing

#### Geofence Management
- **Set Geofence**: Group admin can set geofence radius (10-5000 meters)
- **Dynamic Center**: Geofence center is always the group creator's current location
- **Radius Storage**: Geofence radius stored and retrieved via API

#### Geofence Breach Detection
- **Automatic Monitoring**: Checks member locations against geofence boundaries in real-time
- **Distance Calculation**: Uses Haversine formula for accurate distance calculation
- **Breach Alerts**: Real-time notification when member leaves geofence
- **SignalR Event**: `GeofenceBreached` event sent to all group members
- **Alert Details**: User ID, distance from center, radius information

#### Geofence Visualization
- **Map Overlay**: Visual circle showing geofence boundary on map
- **Color Coding**: Green (within boundary), Red (breached)
- **Real-Time Updates**: Geofence updates as creator moves
- **Member Status**: Visual indicators for each member's geofence status
- **Distance Display**: Show distance from geofence center for each member

---

### 6. Real-Time Communication (SignalR)

#### Location Hub (`/locationHub`)
- **Purpose**: Real-time location sharing
- **Methods**:
  - `JoinGroup(groupId)`: Join location group for real-time updates
  - `LeaveGroup(groupId)`: Leave location group
  - `SendLocation(groupId, latitude, longitude)`: Broadcast location to group
- **Events**:
  - `ReceiveLocation(latitude, longitude)`: Receive location updates from group members
  - `GeofenceBreached`: Alert when member breaches geofence boundary

#### Emergency Hub (`/emergencyHub`)
- **Purpose**: Emergency SOS alerts and notifications
- **Methods**:
  - `JoinGroup(groupId)`: Join emergency group
  - `LeaveGroup(groupId)`: Leave emergency group
- **Events**:
  - `SOSAlert`: Receive emergency alerts with user ID, coordinates, emergency type
  - `SOSResolved`: Notification when emergency is resolved

#### Group Hub (`/groupHub`)
- **Purpose**: Group activity notifications
- **Methods**:
  - `JoinGroup(groupId)`: Join group updates
  - `LeaveGroup(groupId)`: Leave group updates
- **Events**:
  - `MemberJoined`: Notify when new member joins group
  - `GroupUpdated`: Notify when group settings change

#### Connection Features
- **Authentication**: JWT token-based authentication for SignalR connections
- **Group Management**: Automatic group assignment based on user's active groups
- **Reconnection**: Automatic reconnection on connection loss with exponential backoff
- **Connection State**: Monitor and display connection status to users

---

### 7. Privacy & Security

#### Data Protection
- **Encryption**: AES-256 encryption for sensitive data
- **TLS 1.3**: All API communications encrypted
- **Token Security**: Secure token storage using platform keychain/keystore
- **Password Security**: BCrypt hashing with salt for password storage

#### Privacy Controls
- **Privacy Mode**: User-controlled privacy settings toggle
- **Group-Based Sharing**: Location only shared within active groups
- **Automatic Privacy**: Privacy automatically enabled when joining groups, disabled when leaving
- **Data Retention**: Location history cleared after 24 hours
- **Permission Control**: User controls when location is shared

#### Security Features
- **JWT Authentication**: Secure token-based authentication
- **Rate Limiting**: API rate limiting to prevent abuse
- **Input Validation**: Server-side validation for all inputs
- **Secure Storage**: Platform-specific secure storage for tokens and sensitive data

---

### 8. Notifications

#### Notification Types
- **Emergency Alerts**: Immediate notification for SOS triggers
- **Geofence Breaches**: Alert when member leaves geofence boundary
- **Group Updates**: Notifications for group changes (member joined, settings updated)
- **Member Activity**: Notifications when members join/leave groups

#### Notification Channels
- **In-App Notifications**: SignalR-based real-time alerts
- **SMS Notifications**: Send SMS alerts for emergencies (via backend)
- **Email Notifications**: Email alerts for emergency situations (via backend)
- **Push Notifications**: Native push notifications for iOS/Android (planned)

---

### 9. User Interface Features

#### Navigation
- **Bottom Navigation Bar** (4 main tabs):
  1. Dashboard/Home
  2. Groups
  3. Map/Location
  4. Profile

#### Dashboard Screen
- **Top Section**:
  - User greeting
  - Current group status (if in group)
  - Quick stats (members online, active emergencies)
  
- **Main Section**:
  - Large SOS button (red, prominent, always visible)
  - Current location display
  - Active group card
  - Recent activity feed
  
- **Bottom Section**:
  - Quick actions (Join group, Create group)

#### Groups Screen
- **List View**: 
  - All user's groups
  - Group name, member count, status
  - Swipe actions (leave group, view details)
  
- **Create/Join**:
  - Floating action button to create group
  - Search/scan invite code button

#### Map Screen
- **Full-screen Map**:
  - User's current location (blue dot)
  - Group members' locations (colored pins)
  - Geofence circle overlay
  - Tap member pin to see details
  
- **Bottom Sheet**:
  - Member list with distances
  - Last updated times
  - Navigation options

#### Profile Screen
- **User Info**:
  - Profile picture (circular)
  - Name, email
  - Verification badge
  
- **Settings**:
  - Edit profile
  - Change password
  - Privacy settings toggle
  - Notification preferences
  - About/Legal

---

### 10. Additional Features

#### Offline Support
- **Queue Actions**: Queue API requests when offline
- **Local Storage**: Cache data locally for offline access
- **Sync Mechanism**: Automatic sync when connection restored
- **Error Handling**: Graceful error messages for network issues

#### Location Features
- **Precise Coordinates**: Display GPS coordinates with 6 decimal places
- **Address Lookup**: Reverse geocoding for user-friendly addresses
- **Location History**: View location history timeline
- **Battery Optimization**: Efficient location tracking to preserve battery

#### Accessibility
- **WCAG 2.1 AA Compliance**: Accessibility standards
- **Screen Reader Support**: Voice-over/TalkBack compatibility
- **High Contrast Mode**: Support for accessibility preferences
- **Large Text Support**: Adjustable text sizes

---

## Summary

Haven is a comprehensive safety platform that combines:

✅ **Secure Authentication** with JWT tokens and biometric support  
✅ **Group Management** for creating and joining safety groups  
✅ **Real-Time Location Sharing** via SignalR WebSocket connections  
✅ **Emergency SOS System** with precise GPS coordinates and multi-channel alerts  
✅ **Geofencing** for automatic boundary monitoring and breach detection  
✅ **Privacy Controls** for user data protection  
✅ **Real-Time Notifications** via SignalR, SMS, and Email  
✅ **Interactive Maps** showing all group members' locations  
✅ **Offline Support** with local caching and sync  
✅ **User-Friendly Interface** with intuitive navigation and clear design

The platform is designed to provide **peace of mind** by ensuring users can always reach their trusted contacts in emergencies, while maintaining **privacy and security** as top priorities.

