# Haven Mobile App Development Prompt

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

## Core Features & Requirements

### 1. Authentication & User Management

#### 1.1 Registration
- **Screen**: Registration form with validation
- **Fields**:
  - Username (required, unique)
  - Email (required, valid email format)
  - Password (required, minimum 8 characters)
  - First Name (optional)
  - Last Name (optional)
- **API Endpoint**: `POST /api/v1/auth/register`
- **Request Body**:
  ```json
  {
    "username": "string",
    "email": "string",
    "password": "string",
    "firstName": "string",
    "lastName": "string"
  }
  ```
- **Response**: Access token, refresh token, user object, expires in (3600 seconds)
- **Behavior**: Auto-login after successful registration

#### 1.2 Login
- **Screen**: Login form
- **Fields**: Username, Password
- **API Endpoint**: `POST /api/v1/auth/login`
- **Request Body**:
  ```json
  {
    "username": "string",
    "password": "string"
  }
  ```
- **Response**: Access token, refresh token, user object, expires in
- **Features**:
  - Remember me option (store refresh token securely)
  - Biometric authentication (Face ID/Touch ID/Fingerprint)
  - Auto-refresh token before expiration

#### 1.3 Token Refresh
- **API Endpoint**: `POST /api/v1/auth/refresh`
- **Request Body**:
  ```json
  {
    "refreshToken": "string"
  }
  ```
- **Implementation**: Automatic token refresh when access token expires
- **Storage**: Securely store tokens using platform keychain/keystore

#### 1.4 Logout
- **API Endpoint**: `POST /api/v1/auth/logout`
- **Headers**: `Authorization: Bearer {token}`
- **Behavior**: Clear stored tokens and user data

#### 1.5 User Profile
- **View Profile**:
  - **API Endpoint**: `GET /api/v1/users/profile`
  - **Headers**: `Authorization: Bearer {token}`
  - **Display**: First Name, Last Name, Email, Profile Image, Verification Status, Privacy Settings
  
- **Update Profile**:
  - **API Endpoint**: `PUT /api/v1/users/profile`
  - **Request Body**:
    ```json
    {
      "firstName": "string",
      "lastName": "string",
      "email": "string",
      "profileImageUrl": "string (optional)"
    }
    ```
  
- **Change Password**:
  - **API Endpoint**: `POST /api/v1/users/change-password`
  - **Request Body**:
    ```json
    {
      "currentPassword": "string",
      "newPassword": "string"
    }
    ```

---

### 2. Group Management

#### 2.1 Create Group
- **Screen**: Create group form
- **Fields**:
  - Group Name (required)
  - Description (optional)
  - Geofence Radius in meters (default: 100m, range: 10-5000m)
- **API Endpoint**: `POST /api/v1/groups`
- **Request Body**:
  ```json
  {
    "name": "string",
    "description": "string (optional)",
    "createdBy": "guid",
    "geofenceRadius": 100.0
  }
  ```
- **Response**: Group object with ID, Invite Code, Created Date
- **Behavior**: Creator automatically becomes group admin

#### 2.2 Join Group by Invite Code
- **Screen**: Join group form
- **Field**: Invite Code (6-8 character alphanumeric)
- **API Endpoint**: `POST /api/v1/groups/join`
- **Request Body**:
  ```json
  {
    "inviteCode": "string",
    "userId": "guid"
  }
  ```
- **Features**:
  - QR code scanner for invite codes
  - Validate invite code before joining
  - Show group preview before joining

#### 2.3 View Group Details
- **API Endpoint**: `GET /api/v1/groups/{id}`
- **Display**: Group name, description, member count, geofence radius, created date

#### 2.4 Get Group by Invite Code
- **API Endpoint**: `GET /api/v1/groups/by-invite/{inviteCode}`
- **Use Case**: Preview group before joining

#### 2.5 Get Group with Members
- **API Endpoint**: `GET /api/v1/groups/by-invite/{inviteCode}/with-members`
- **Display**: Group details + list of members with names and roles

#### 2.6 List Group Members
- **API Endpoint**: `GET /api/v1/groups/{id}/members`
- **Response**: Array of members with user details (username, email, first name, last name, role, joined date)
- **Display**: Member list screen with avatars, names, roles, join dates

#### 2.7 Update Group
- **API Endpoint**: `PUT /api/v1/groups/{id}`
- **Request Body**:
  ```json
  {
    "name": "string"
  }
  ```
- **Permission**: Only group admin can update

#### 2.8 Get User's Current Group
- **API Endpoint**: `GET /api/v1/groups/user/{userId}/current`
- **Use Case**: Display active group on dashboard

#### 2.9 Get User's All Groups
- **API Endpoint**: `GET /api/v1/groups/user/{userId}/all`
- **Display**: List of all groups user belongs to

---

### 3. Real-Time Location Tracking

#### 3.1 Location Service
- **Permissions**: Request location permissions (Always Allow for background tracking)
- **Accuracy**: High accuracy GPS (within 5-10 meters)
- **Update Frequency**: Every 5 meters or 10 seconds (whichever comes first)
- **Background Tracking**: Continue tracking when app is in background

#### 3.2 Send Location via SignalR
- **Hub**: `LocationHub`
- **Method**: `SendLocation(groupId, latitude, longitude)`
- **Connection**: Establish SignalR connection on app start
- **Implementation**:
  - Connect to SignalR hub: `/locationHub`
  - Join group: `JoinGroup(groupId)`
  - Send location updates: `SendLocation(groupId, lat, lng)`
  - Handle incoming locations: `ReceiveLocation(latitude, longitude)`

#### 3.3 Location Display
- **Map View**: 
  - Show all group members' locations on map
  - Different markers/colors for each member
  - Real-time position updates
  - Show user's own location with blue dot
  - Show other members with colored pins
  
- **List View**:
  - List of members with distance from user
  - Last updated timestamp
  - Address/location name (reverse geocoding)

#### 3.4 Location History
- **Storage**: Store location history locally (last 24 hours)
- **Display**: Timeline view of location history
- **Privacy**: Only show to group members

---

### 4. Emergency SOS System

#### 4.1 Trigger SOS Alert
- **Screen**: Emergency SOS button (prominent, always accessible)
- **API Endpoint**: `POST /api/v1/emergency`
- **Request Body**:
  ```json
  {
    "id": "string",
    "groupId": "string",
    "userId": "string",
    "latitude": 0.0,
    "longitude": 0.0,
    "emergencyType": "SOS",
    "timestamp": "datetime",
    "status": "Active"
  }
  ```
- **Features**:
  - Large, red SOS button on main screen
  - Confirmation dialog before sending
  - Display precise GPS coordinates in alert
  - Show address/location name
  - Send to all group members via SignalR
  - Send SMS/Email notifications (handled by backend)
  - Vibrate and sound alert for recipients

#### 4.2 Receive SOS Alerts via SignalR
- **Hub**: `EmergencyHub`
- **Event**: `SOSAlert`
- **Payload**:
  ```json
  {
    "userId": "string",
    "latitude": 0.0,
    "longitude": 0.0,
    "emergencyType": "string"
  }
  ```
- **Display**:
  - Full-screen emergency alert
  - Map showing emergency location
  - Navigation button (open in maps app)
  - User information (name, phone if available)
  - Call emergency services button

#### 4.3 Resolve Emergency
- **API Endpoint**: `POST /api/v1/emergency/{id}/resolve`
- **Request Body**:
  ```json
  {
    "groupId": "string"
  }
  ```
- **Permission**: Only emergency initiator or group admin can resolve
- **SignalR Event**: `SOSResolved` - notify all group members

#### 4.4 Emergency Coordinates Display
- **Feature**: Show precise GPS coordinates (6 decimal places)
- **Format**: `Latitude, Longitude` (e.g., `37.774929, -122.419418`)
- **Use Case**: Share with emergency services
- **Display**: Copy to clipboard functionality

---

### 5. Geofencing

#### 5.1 Set Geofence
- **API Endpoint**: `POST /api/v1/geofence/{groupId}`
- **Request Body**:
  ```json
  {
    "radiusMeters": 100.0
  }
  ```
- **Permission**: Only group admin can set geofence
- **Behavior**: Geofence center is always the creator's current location

#### 5.2 Get Geofence
- **API Endpoint**: `GET /api/v1/geofence/{groupId}`
- **Response**: `{ "radiusMeters": 100.0 }`
- **Display**: Show geofence circle on map

#### 5.3 Geofence Breach Detection
- **Backend**: Automatically checks when location updates are received
- **Notification**: Alert when member leaves geofence boundary
- **Display**: Visual indicator on map (red circle for breach)

---

### 6. SignalR Real-Time Communication

#### 6.1 Hubs Configuration
- **LocationHub**: `/locationHub`
  - `JoinGroup(groupId)` - Join location group
  - `LeaveGroup(groupId)` - Leave location group
  - `SendLocation(groupId, latitude, longitude)` - Send location update
  - `ReceiveLocation(latitude, longitude)` - Receive location update
  
- **EmergencyHub**: `/emergencyHub`
  - `JoinGroup(groupId)` - Join emergency group
  - `LeaveGroup(groupId)` - Leave emergency group
  - `SOSAlert` - Receive emergency alert
  - `SOSResolved` - Receive resolution notification
  
- **GroupHub**: `/groupHub`
  - `JoinGroup(groupId)` - Join group updates
  - `LeaveGroup(groupId)` - Leave group updates
  - `MemberJoined` - Notify when member joins
  - `GroupUpdated` - Notify when group is updated

#### 6.2 Connection Management
- **Auto-reconnect**: Implement exponential backoff retry
- **Connection State**: Show connection status indicator
- **Background**: Maintain connection in background (platform-specific)

---

## UI/UX Design Requirements

### 7.1 Design System

#### Color Palette
- **Primary**: Safety-focused colors (Blue: #2196F3, Red: #F44336 for emergencies)
- **Background**: Light mode default, dark mode support
- **Emergency**: Red (#F44336) for SOS, alerts
- **Success**: Green (#4CAF50) for confirmations
- **Warning**: Orange (#FF9800) for geofence breaches

#### Typography
- **Headers**: Bold, clear hierarchy
- **Body**: Readable font size (minimum 14pt)
- **Emergency Text**: Large, bold, high contrast

#### Components
- **Buttons**: 
  - Primary: Large, rounded corners, clear labels
  - Emergency SOS: Red, large, always visible
  - Secondary: Outlined style
  
- **Cards**: 
  - Group cards with member count, status
  - Location cards with map preview
  
- **Maps**: 
  - Clean, minimal UI
  - Custom markers for members
  - Geofence visualization

### 7.2 Screen Structure

#### Navigation
- **Bottom Navigation Bar** (4 tabs):
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
  - Large SOS button (red, prominent)
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

### 7.3 User Experience Guidelines

#### Accessibility
- **WCAG 2.1 AA compliance**
- **Screen reader support**
- **High contrast mode**
- **Large text support**
- **Voice-over/TalkBack compatibility**

#### Performance
- **App launch**: < 2 seconds
- **Location update**: < 1 second delay
- **Map rendering**: Smooth 60fps
- **Battery optimization**: Efficient location tracking

#### Error Handling
- **Network errors**: Clear error messages, retry options
- **Permission denied**: In-app permission request with explanation
- **Location unavailable**: Fallback to last known location
- **Token expired**: Auto-refresh, seamless re-authentication

---

## Technical Architecture

### 8.1 Technology Stack

#### Recommended Framework
- **Flutter/Dart** (cross-platform)
- **Alternative**: React Native or Native (Swift/Kotlin)

#### Key Dependencies
- **HTTP Client**: `dio` or `http` for REST API calls
- **SignalR**: `signalr_netcore` or `signalr_core` for real-time communication
- **Maps**: `google_maps_flutter` or `mapbox_maps_flutter`
- **Location**: `geolocator` for GPS tracking
- **State Management**: `provider`, `riverpod`, or `bloc`
- **Local Storage**: `shared_preferences`, `hive`, or `sqflite`
- **Secure Storage**: `flutter_secure_storage` for tokens
- **Image Picker**: `image_picker` for profile pictures
- **QR Scanner**: `qr_code_scanner` or `mobile_scanner`

### 8.2 Project Structure

```
lib/
├── main.dart
├── models/
│   ├── user.dart
│   ├── group.dart
│   ├── location.dart
│   └── emergency.dart
├── services/
│   ├── api_service.dart
│   ├── auth_service.dart
│   ├── location_service.dart
│   ├── signalr_service.dart
│   └── storage_service.dart
├── screens/
│   ├── auth/
│   │   ├── login_screen.dart
│   │   └── register_screen.dart
│   ├── dashboard/
│   │   └── dashboard_screen.dart
│   ├── groups/
│   │   ├── groups_list_screen.dart
│   │   ├── create_group_screen.dart
│   │   ├── join_group_screen.dart
│   │   └── group_details_screen.dart
│   ├── map/
│   │   └── map_screen.dart
│   └── profile/
│       └── profile_screen.dart
├── widgets/
│   ├── sos_button.dart
│   ├── member_marker.dart
│   └── group_card.dart
└── utils/
    ├── constants.dart
    └── helpers.dart
```

### 8.3 State Management

#### Authentication State
- Store JWT token securely
- Manage refresh token lifecycle
- Handle token expiration
- User session persistence

#### Location State
- Current location coordinates
- Location tracking status
- Group members' locations
- Location history

#### Group State
- User's groups list
- Current active group
- Group members
- Group settings

---

## API Integration Details

### 9.1 Base URL Configuration
- **Development**: `http://localhost:5000` or configured dev URL
- **Production**: Configured production URL
- **Environment**: Use environment variables for different builds

### 9.2 Authentication Headers
- **Format**: `Authorization: Bearer {accessToken}`
- **Storage**: Secure storage (keychain/keystore)
- **Refresh**: Automatic refresh before expiration

### 9.3 Error Handling
- **401 Unauthorized**: Refresh token or redirect to login
- **403 Forbidden**: Show permission denied message
- **404 Not Found**: Show not found message
- **500 Server Error**: Show generic error, retry option
- **Network Error**: Show offline message, queue requests

### 9.4 Request/Response Format
- **Content-Type**: `application/json`
- **Accept**: `application/json`
- **Date Format**: ISO 8601 (`yyyy-MM-ddTHH:mm:ssZ`)

---

## Security & Privacy

### 10.1 Data Protection
- **Encryption**: TLS 1.3 for all API calls
- **Token Storage**: Platform secure storage (Keychain/Keystore)
- **Local Data**: Encrypt sensitive data in local storage
- **Biometric Auth**: Use platform biometric APIs

### 10.2 Privacy Features
- **Privacy Toggle**: User can enable/disable location sharing
- **Group Privacy**: Location only shared within active group
- **Data Retention**: Clear location history after 24 hours
- **Permissions**: Request only necessary permissions

### 10.3 Location Privacy
- **Granular Control**: User controls when location is shared
- **Group-based**: Location only visible to group members
- **Geofence**: Respect geofence boundaries
- **Emergency Override**: Always share location during SOS

---

## Implementation Instructions

### 11.1 Setup & Configuration

1. **Initialize Project**
   - Create Flutter project: `flutter create haven_mobile`
   - Configure iOS/Android settings
   - Set up environment configuration

2. **Install Dependencies**
   ```yaml
   dependencies:
     flutter:
       sdk: flutter
     dio: ^5.0.0
     signalr_netcore: ^1.0.0
     google_maps_flutter: ^2.0.0
     geolocator: ^10.0.0
     provider: ^6.0.0
     flutter_secure_storage: ^9.0.0
     shared_preferences: ^2.0.0
     qr_code_scanner: ^1.0.0
   ```

3. **Configure Permissions**
   - **Android**: `AndroidManifest.xml`
     - `ACCESS_FINE_LOCATION`
     - `ACCESS_COARSE_LOCATION`
     - `ACCESS_BACKGROUND_LOCATION`
   - **iOS**: `Info.plist`
     - `NSLocationWhenInUseUsageDescription`
     - `NSLocationAlwaysAndWhenInUseUsageDescription`

4. **API Configuration**
   - Create `config.dart` with base URL
   - Set up API service with interceptors
   - Configure token refresh logic

### 11.2 Development Phases

#### Phase 1: Authentication (Week 1-2)
- [ ] Login screen
- [ ] Registration screen
- [ ] Token storage and management
- [ ] Auto-refresh token
- [ ] Biometric authentication

#### Phase 2: User Profile (Week 2-3)
- [ ] Profile view screen
- [ ] Edit profile functionality
- [ ] Change password
- [ ] Profile image upload

#### Phase 3: Group Management (Week 3-4)
- [ ] Groups list screen
- [ ] Create group screen
- [ ] Join group screen (with QR scanner)
- [ ] Group details screen
- [ ] Member list screen

#### Phase 4: Location Tracking (Week 4-5)
- [ ] Location service implementation
- [ ] Map screen with markers
- [ ] SignalR connection for location
- [ ] Background location tracking
- [ ] Location history

#### Phase 5: Emergency SOS (Week 5-6)
- [ ] SOS button implementation
- [ ] Emergency alert screen
- [ ] SignalR emergency hub
- [ ] Navigation to emergency location
- [ ] Emergency resolution

#### Phase 6: Geofencing (Week 6-7)
- [ ] Geofence visualization on map
- [ ] Geofence breach detection
- [ ] Geofence settings

#### Phase 7: Polish & Testing (Week 7-8)
- [ ] UI/UX refinements
- [ ] Error handling improvements
- [ ] Performance optimization
- [ ] Testing on iOS and Android
- [ ] Bug fixes

### 11.3 Testing Checklist

- [ ] Authentication flow (login, register, logout)
- [ ] Token refresh mechanism
- [ ] Group CRUD operations
- [ ] Location tracking accuracy
- [ ] SignalR real-time updates
- [ ] SOS alert functionality
- [ ] Geofence breach detection
- [ ] Offline mode handling
- [ ] Permission handling
- [ ] Error scenarios

### 11.4 Deployment

#### iOS
- [ ] Configure App Store Connect
- [ ] Set up certificates and provisioning profiles
- [ ] TestFlight beta testing
- [ ] App Store submission

#### Android
- [ ] Configure Google Play Console
- [ ] Generate signed APK/AAB
- [ ] Internal testing track
- [ ] Production release

---

## Additional Features (Future Enhancements)

### 12.1 Notifications
- Push notifications for SOS alerts
- Background location updates
- Group activity notifications

### 12.2 Offline Support
- Queue API requests when offline
- Sync when connection restored
- Local caching of group data

### 12.3 Advanced Features
- Location sharing history
- Emergency contacts (outside groups)
- Check-in functionality
- Battery optimization modes
- Custom emergency messages

---

## Support & Documentation

### 13.1 API Documentation
- Swagger/OpenAPI docs available at `/swagger`
- All endpoints documented with request/response examples

### 13.2 Backend Architecture
- Clean Architecture pattern
- PostgreSQL for relational data
- MongoDB for location data
- RabbitMQ for messaging
- SignalR for real-time communication

### 13.3 Contact & Support
- Backend API: Configure base URL in app settings
- SignalR Hub URLs: `/locationHub`, `/emergencyHub`, `/groupHub`
- Error logging: Implement crash reporting (Firebase Crashlytics/Sentry)

---

## Conclusion

This document provides comprehensive specifications for building the Haven mobile application. Follow the implementation phases, adhere to the design guidelines, and ensure security and privacy best practices are maintained throughout development.

**Key Priorities**:
1. Security and privacy
2. Real-time location accuracy
3. Reliable emergency SOS functionality
4. Smooth user experience
5. Battery efficiency

**Success Metrics**:
- App launch time < 2 seconds
- Location accuracy within 10 meters
- SOS alert delivery < 3 seconds
- 99%+ uptime for SignalR connections
- Positive user reviews (>4.5 stars)

