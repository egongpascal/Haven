# Haven Project Overview

## What is Haven?

**Haven** is a comprehensive **safety and emergency response platform** designed to help people stay connected and safe. It enables users to create private safety groups with friends and family, share real-time locations, and trigger emergency SOS alerts with precise GPS coordinates. The platform consists of a robust .NET Core backend API and a mobile application (iOS/Android).

### Core Purpose
Haven addresses the critical need for reliable, real-time safety communication. Whether you're traveling, walking home late, or in an emergency situation, Haven ensures your trusted contacts know your exact location and can respond quickly when needed.

---

## Architecture Overview

Haven follows **Clean Architecture** principles with a layered structure:

### Backend Architecture
- **API Layer**: RESTful API controllers and SignalR hubs for real-time communication
- **Application Layer**: Business logic, use cases, and service interfaces
- **Domain Layer**: Core business models, entities, and domain services
- **Infrastructure Layer**: Data access (PostgreSQL, MongoDB), messaging (RabbitMQ), and external integrations

### Technology Stack
- **Backend**: ASP.NET Core (.NET 8)
- **Database**: PostgreSQL (user/group data), MongoDB (location data with geospatial indexing)
- **Real-time**: SignalR hubs for live location updates and emergency alerts
- **Messaging**: RabbitMQ for offline sync and queuing
- **Security**: JWT authentication, AES-256 encryption, TLS 1.3
- **Mobile**: Flutter/Dart (cross-platform iOS & Android)

---

## Complete Feature List

### 1. 🔐 Authentication & User Management

#### User Registration
- Create account with username, email, and password
- Optional first name and last name
- Automatic account creation with secure password hashing (BCrypt)
- Returns JWT access token and refresh token

#### User Login
- Username/password authentication
- JWT token-based authentication
- Refresh token mechanism for seamless sessions
- Token expiration: 1 hour (auto-refreshable)

#### User Profile Management
- **View Profile**: Display user information (name, email, profile image, verification status)
- **Update Profile**: Modify name, email, and profile picture
- **Change Password**: Secure password change with current password verification
- **Privacy Settings**: Toggle privacy mode (enabled when in groups, disabled when not)

#### Security Features
- JWT tokens with user claims (ID, email, name, verification status, privacy settings)
- Secure password storage using BCrypt hashing
- Refresh token rotation for enhanced security
- Rate limiting on API endpoints
- CORS configuration for cross-origin requests

---

### 2. 👥 Group Management

#### Create Safety Groups
- Create private groups with custom names and descriptions
- Set geofence radius (10-5000 meters) for boundary monitoring
- Automatic invite code generation (6-character alphanumeric)
- Group creator automatically becomes admin
- Groups stored in PostgreSQL with relationships

#### Join Groups
- Join groups using invite codes
- QR code scanning support (mobile app)
- Preview group details before joining
- View group members and their details
- Automatic member relationship creation

#### Group Operations
- **List Groups**: View all groups user belongs to
- **Get Current Group**: Retrieve active group user is currently in
- **Group Details**: View group information, member count, geofence settings
- **Member Management**: View all members with their details (name, email, role, join date)
- **Update Group**: Modify group name (admin only)
- **Group Invite Codes**: Unique codes for easy group sharing

#### Group Roles & Permissions
- **Admin/Creator**: Can update group settings, set geofence, manage members
- **Member**: Can view group, share location, trigger SOS
- Role-based access control for group operations

---

### 3. 📍 Real-Time Location Tracking

#### Location Services
- **High-Accuracy GPS**: Precise location tracking (within 5-10 meters)
- **Background Tracking**: Continuous location updates even when app is in background
- **Update Frequency**: Every 5 meters moved or 10 seconds (whichever comes first)
- **Location History**: Stored in MongoDB with geospatial indexing for efficient queries

#### SignalR Location Hub
- **Real-Time Updates**: Live location sharing via SignalR WebSocket connections
- **Hub Endpoint**: `/locationHub`
- **Methods**:
  - `JoinGroup(groupId)`: Join location group for real-time updates
  - `LeaveGroup(groupId)`: Leave location group
  - `SendLocation(groupId, latitude, longitude)`: Broadcast location to group
  - `ReceiveLocation(latitude, longitude)`: Receive location updates from members

#### Location Features
- **Map Display**: Show all group members' locations on interactive map
- **Member Markers**: Different colored pins for each member
- **Distance Calculation**: Real-time distance between members
- **Address Resolution**: Reverse geocoding for user-friendly location names
- **Location Privacy**: Location only shared within active group
- **Privacy Toggle**: Automatically enabled when joining groups, disabled when leaving

#### Location Data Storage
- **MongoDB**: Geospatial database optimized for location queries
- **Indexing**: Geospatial indexes for fast location-based searches
- **History**: Track location history for 24 hours
- **Latest Location**: Quick access to most recent location for each user

---

### 4. 🚨 Emergency SOS System

#### Trigger Emergency Alert
- **SOS Button**: Large, prominent emergency button (always accessible)
- **Precise Coordinates**: Sends exact GPS coordinates (6 decimal places)
- **Emergency Types**: Support for different emergency types (SOS, Medical, etc.)
- **Instant Notification**: Immediate alert to all group members
- **Multi-Channel Alerts**:
  - SignalR real-time push to all group members
  - SMS notifications (via backend service)
  - Email notifications (via backend service)

#### Emergency Request
- **API Endpoint**: `POST /api/v1/emergency`
- **Payload**: User ID, Group ID, Latitude, Longitude, Emergency Type, Timestamp
- **Storage**: Emergency records stored in database
- **Status Tracking**: Track emergency status (Active, Resolved)

#### Receive Emergency Alerts
- **SignalR Hub**: `/emergencyHub`
- **Event**: `SOSAlert` - Broadcasts emergency to all group members
- **Alert Display**:
  - Full-screen emergency notification
  - Map showing emergency location
  - User information (name, contact)
  - Navigation button (open in maps app)
  - Call emergency services button

#### Resolve Emergency
- **API Endpoint**: `POST /api/v1/emergency/{id}/resolve`
- **Permission**: Only emergency initiator or group admin can resolve
- **Notification**: `SOSResolved` event via SignalR to notify all members
- **Status Update**: Emergency status updated in database

#### Emergency Features
- **Location Accuracy**: High-precision GPS coordinates for emergency services
- **Address Display**: Human-readable address alongside coordinates
- **Emergency History**: Track past emergencies
- **Quick Response**: One-tap SOS activation
- **Confirmation Dialog**: Prevent accidental triggers

---

### 5. 🗺️ Geofencing

#### Geofence Management
- **Set Geofence**: Group admin can set geofence radius (10-5000 meters)
- **API Endpoint**: `POST /api/v1/geofence/{groupId}`
- **Dynamic Center**: Geofence center is always the group creator's current location
- **Radius Storage**: Geofence radius stored in MongoDB

#### Geofence Breach Detection
- **Automatic Monitoring**: Checks member locations against geofence boundaries
- **Distance Calculation**: Uses Haversine formula for accurate distance calculation
- **Breach Alerts**: Real-time notification when member leaves geofence
- **SignalR Event**: `GeofenceBreached` event sent to all group members
- **Alert Details**: User ID, distance from center, radius information

#### Geofence Visualization
- **Map Overlay**: Visual circle showing geofence boundary
- **Color Coding**: Green (within), Red (breached)
- **Real-Time Updates**: Geofence updates as creator moves
- **Member Status**: Visual indicators for each member's geofence status

---

### 6. 🔄 Real-Time Communication (SignalR)

#### Location Hub (`/locationHub`)
- **Purpose**: Real-time location sharing
- **Events**:
  - `ReceiveLocation`: Receive location updates from group members
  - `GeofenceBreached`: Alert when member breaches geofence boundary
- **Connection Management**: Auto-reconnect with exponential backoff

#### Emergency Hub (`/emergencyHub`)
- **Purpose**: Emergency SOS alerts and notifications
- **Events**:
  - `SOSAlert`: Receive emergency alerts
  - `SOSResolved`: Notification when emergency is resolved
- **Real-Time**: Instant delivery to all connected group members

#### Group Hub (`/groupHub`)
- **Purpose**: Group activity notifications
- **Events**:
  - `MemberJoined`: Notify when new member joins group
  - `GroupUpdated`: Notify when group settings change
- **Activity Feed**: Real-time updates on group changes

#### Connection Features
- **Authentication**: JWT token-based authentication for SignalR connections
- **Group Management**: Automatic group assignment based on user's active groups
- **Reconnection**: Automatic reconnection on connection loss
- **Connection State**: Monitor connection status

---

### 7. 🔒 Privacy & Security

#### Data Protection
- **Encryption**: AES-256 encryption for sensitive data
- **TLS 1.3**: All API communications encrypted
- **Token Security**: Secure token storage (platform keychain/keystore)
- **Password Security**: BCrypt hashing with salt

#### Privacy Controls
- **Privacy Mode**: User-controlled privacy settings
- **Group-Based Sharing**: Location only shared within active groups
- **Automatic Privacy**: Privacy enabled when joining groups, disabled when leaving
- **Data Retention**: Location history cleared after 24 hours

#### Security Features
- **JWT Authentication**: Secure token-based authentication
- **Rate Limiting**: API rate limiting to prevent abuse
- **CORS Configuration**: Controlled cross-origin access
- **Input Validation**: Server-side validation for all inputs
- **SQL Injection Protection**: Parameterized queries

---

### 8. 📱 Offline Capabilities

#### Offline Support (Planned)
- **RabbitMQ Integration**: Queue actions when offline
- **Local Storage**: Cache data locally for offline access
- **Sync Mechanism**: Automatic sync when connection restored
- **Queue Management**: Priority queue for emergency actions

#### Current Offline Handling
- **Error Handling**: Graceful error messages for network issues
- **Retry Logic**: Automatic retry for failed requests
- **Connection Monitoring**: Detect offline state
- **User Feedback**: Clear messaging about connection status

---

### 9. 🔔 Notifications

#### Notification Service
- **SMS Notifications**: Send SMS alerts for emergencies
- **Email Notifications**: Email alerts for emergency situations
- **Push Notifications**: Real-time push notifications (mobile app)
- **In-App Notifications**: SignalR-based real-time alerts

#### Notification Types
- **Emergency Alerts**: Immediate notification for SOS triggers
- **Geofence Breaches**: Alert when member leaves geofence
- **Group Updates**: Notifications for group changes
- **Member Activity**: Notifications when members join/leave

---

### 10. 📊 Data Management

#### Database Architecture
- **PostgreSQL**: 
  - User accounts and authentication
  - Group data and relationships
  - Group memberships and roles
  - Emergency records
  
- **MongoDB**:
  - Location history with geospatial indexing
  - Geofence configurations
  - Location-based queries and analytics

#### Data Models
- **User**: ID, Username, Email, Password Hash, Name, Profile Image, Verification Status, Privacy Settings
- **Group**: ID, Name, Description, Creator ID, Geofence Radius, Invite Code, Member Count
- **Location**: User ID, Group ID, Latitude, Longitude, Timestamp
- **Emergency**: ID, Group ID, User ID, Coordinates, Emergency Type, Status, Timestamp

---

## API Endpoints Summary

### Authentication (`/api/v1/auth`)
- `POST /register` - User registration
- `POST /login` - User login
- `POST /refresh` - Refresh access token
- `POST /logout` - User logout

### Users (`/api/v1/users`)
- `GET /profile` - Get user profile
- `PUT /profile` - Update user profile
- `POST /change-password` - Change password

### Groups (`/api/v1/groups`)
- `POST /` - Create group
- `GET /{id}` - Get group by ID
- `PUT /{id}` - Update group
- `POST /join` - Join group by invite code
- `GET /by-invite/{inviteCode}` - Get group by invite code
- `GET /by-invite/{inviteCode}/with-members` - Get group with members
- `GET /{id}/members` - Get group members
- `GET /user/{userId}/current` - Get user's current group
- `GET /user/{userId}/all` - Get all user's groups

### Emergency (`/api/v1/emergency`)
- `POST /` - Trigger SOS alert
- `POST /{id}/resolve` - Resolve emergency

### Geofence (`/api/v1/geofence`)
- `POST /{groupId}` - Set geofence radius
- `GET /{groupId}` - Get geofence settings

---

## Use Cases

### 1. Family Safety Group
- **Scenario**: Family members create a group to track each other's locations
- **Features Used**: Group creation, location sharing, geofencing
- **Benefit**: Parents can monitor children's locations, get alerts if they leave safe zones

### 2. Travel Safety
- **Scenario**: Friends traveling together want to stay connected
- **Features Used**: Group creation, real-time location, SOS alerts
- **Benefit**: Quick response in case of emergency, always know where friends are

### 3. Emergency Response
- **Scenario**: User triggers SOS in dangerous situation
- **Features Used**: Emergency SOS, real-time alerts, precise coordinates
- **Benefit**: Immediate notification to trusted contacts with exact location

### 4. Geofence Monitoring
- **Scenario**: Monitor when group members leave designated safe area
- **Features Used**: Geofencing, breach detection, alerts
- **Benefit**: Automatic alerts when members stray from safe zones

### 5. Night Safety
- **Scenario**: Person walking home late wants friends to track location
- **Features Used**: Location sharing, emergency SOS, group communication
- **Benefit**: Friends can monitor journey, respond quickly if needed

---

## Technical Highlights

### Performance
- **Geospatial Indexing**: MongoDB geospatial indexes for fast location queries
- **Connection Pooling**: Efficient database connection management
- **Caching**: Memory caching for frequently accessed data
- **Rate Limiting**: Prevent API abuse and ensure fair usage

### Scalability
- **Clean Architecture**: Modular design for easy scaling
- **Microservices Ready**: Can be split into microservices if needed
- **Database Optimization**: Separate databases for different data types
- **SignalR Scaling**: Supports multiple SignalR instances with backplane

### Reliability
- **Error Handling**: Comprehensive error handling and logging
- **Connection Resilience**: Auto-reconnect for SignalR connections
- **Data Validation**: Server-side validation for all inputs
- **Backup & Recovery**: Database backup strategies

---

## Future Enhancements

### Planned Features
- **Push Notifications**: Native push notifications for iOS/Android
- **Location History**: Extended location history with analytics
- **Emergency Contacts**: Add emergency contacts outside groups
- **Check-In Feature**: Manual check-in at specific locations
- **Battery Optimization**: Smart location tracking to preserve battery
- **Custom Emergency Messages**: Add custom messages to SOS alerts
- **Group Chat**: In-app messaging within groups
- **Location Sharing Links**: Share location via temporary links

### Technical Improvements
- **End-to-End Encryption**: Signal Protocol integration
- **Advanced Analytics**: Location analytics and insights
- **Machine Learning**: Predictive safety features
- **Integration APIs**: Third-party integrations (smart home, wearables)

---

## Summary

Haven is a **comprehensive safety platform** that combines:
- ✅ **Secure Authentication** with JWT tokens
- ✅ **Group Management** for trusted contacts
- ✅ **Real-Time Location Sharing** via SignalR
- ✅ **Emergency SOS System** with precise GPS coordinates
- ✅ **Geofencing** for boundary monitoring
- ✅ **Privacy Controls** for user data protection
- ✅ **Multi-Channel Notifications** (SMS, Email, Push)
- ✅ **Offline Support** with message queuing
- ✅ **Scalable Architecture** built with Clean Architecture principles

The platform is designed to provide **peace of mind** by ensuring users can always reach their trusted contacts in emergencies, while maintaining **privacy and security** as top priorities.

