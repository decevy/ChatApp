# ChatApp - Application Specification

## Overview
ChatApp is a real-time chat application consisting of a .NET 9 backend API and a React TypeScript frontend. Users can create and join chat rooms, send messages, and interact in real-time through SignalR WebSocket connections.

## Architecture

**Backend Stack:**
- .NET 9 Web API
- PostgreSQL database with Entity Framework Core
- SignalR for real-time communication
- JWT Bearer token authentication
- Clean Architecture pattern with separation of concerns

**Frontend Stack:**
- React 19 with TypeScript
- Vite build tool
- Tailwind CSS 4 for styling
- SignalR client (@microsoft/signalr)
- React Router for navigation
- Axios for HTTP requests
- date-fns for date formatting

**Project Structure:**
- `ChatApp.Core` - Domain layer (entities, DTOs, interfaces)
- `ChatApp.Infrastructure` - Data access layer (EF Core, repositories)
- `ChatApp.Services` - Business logic layer
- `ChatApp.Api` - Web API layer (controllers, SignalR hub)
- `chatapp-web` - React frontend application

## Core Features

### 1. Authentication & Authorization
- User registration with username, email, and password
- User login with JWT token generation
- JWT access tokens with 24-hour expiry
- Refresh token support with 7-day expiry
- Token refresh endpoint
- Logout functionality
- Protected routes requiring authentication
- JWT authentication for both REST API and SignalR connections

### 2. User Management
- User profile management
- User search by username or email
- Online/offline status tracking
- Last seen timestamp tracking
- Automatic status updates on connection/disconnection
- Get current user profile
- Get user by ID
- Update user profile

### 3. Chat Rooms
- Create chat rooms with name, description, and privacy setting
- List all rooms the user is a member of
- Get detailed room information
- Update room information (name, description) - admin/moderator only
- Delete rooms - admin only
- Support for both public and private rooms
- Room membership management

**Room Roles:**
- **Admin** - Full control over room settings and members
- **Moderator** - Can manage members and messages
- **Member** - Basic participation rights

### 4. Room Membership
- Add members to rooms (requires appropriate permissions)
- Remove members from rooms (requires appropriate permissions)
- Automatic room creator assignment as admin
- Unique membership per user per room
- Track join dates

### 5. Messaging
- Send text messages in rooms
- Real-time message delivery via SignalR
- Message history with pagination (default 50 messages per page)
- Edit own messages
- Delete own messages
- Message timestamps
- Edit timestamps for edited messages
- Support for message types: Text, Image, File, System (data model ready, UI implementation may vary)
- Message attachments support (AttachmentUrl, AttachmentFileName fields in data model)

### 6. Real-time Features (SignalR)
- Real-time message broadcasting
- User join/leave room notifications
- Typing indicators (start/stop typing)
- User status changes (online/offline) broadcast to all users
- Message edit notifications
- Message delete notifications
- Automatic reconnection handling
- Connection state management

### 7. Message Reactions (Data Model)
- MessageReaction entity exists in database
- Supports emoji reactions on messages
- Unique constraint: one reaction per user per emoji per message
- UI implementation may vary

## Database Schema

### Users Table
- `Id` (int, primary key)
- `Username` (string, unique, max 50 chars)
- `Email` (string, unique, max 255 chars)
- `PasswordHash` (string, max 255 chars)
- `CreatedAt` (datetime)
- `LastSeen` (datetime)
- `IsOnline` (boolean)
- `RefreshToken` (string, nullable)
- `RefreshTokenExpiry` (datetime, nullable)

### Rooms Table
- `Id` (int, primary key)
- `Name` (string, max 100 chars)
- `Description` (string, max 500 chars, nullable)
- `IsPrivate` (boolean)
- `CreatedBy` (int, foreign key to Users)
- `CreatedAt` (datetime)

### RoomMembers Table
- `Id` (int, primary key)
- `UserId` (int, foreign key to Users)
- `RoomId` (int, foreign key to Rooms)
- `Role` (enum: Member, Moderator, Admin)
- `JoinedAt` (datetime)
- Unique constraint on (UserId, RoomId)

### Messages Table
- `Id` (int, primary key)
- `Content` (string, max 2000 chars)
- `UserId` (int, foreign key to Users)
- `RoomId` (int, foreign key to Rooms)
- `Type` (enum: Text, Image, File, System)
- `CreatedAt` (datetime, indexed)
- `EditedAt` (datetime, nullable)
- `AttachmentUrl` (string, max 500 chars, nullable)
- `AttachmentFileName` (string, max 255 chars, nullable)
- Index on (RoomId, CreatedAt)

### MessageReactions Table
- `Id` (int, primary key)
- `MessageId` (int, foreign key to Messages)
- `UserId` (int, foreign key to Users)
- `Emoji` (string, max 10 chars)
- `CreatedAt` (datetime)
- Unique constraint on (MessageId, UserId, Emoji)

## API Endpoints

### Authentication (`/api/auth`)
- `POST /api/auth/register` - Register new user
  - Body: `RegisterRequest` (Username, Email, Password)
  - Returns: `AuthResponse` with tokens
  
- `POST /api/auth/login` - User login
  - Body: `LoginRequest` (Username/Email, Password)
  - Returns: `AuthResponse` with tokens
  
- `POST /api/auth/refresh` - Refresh access token
  - Body: `RefreshTokenRequest` (RefreshToken)
  - Returns: `AuthResponse` with new tokens
  
- `POST /api/auth/logout` - Logout (authenticated)
  - Returns: Success message
  
- `GET /api/auth/me` - Get current user from token (authenticated)
  - Returns: `CurrentUserResponse` (UserId, Username, Email)

### Rooms (`/api/rooms`)
- `GET /api/rooms` - Get all rooms for current user (authenticated)
  - Returns: `List<RoomSummaryDto>`
  
- `GET /api/rooms/{roomId}` - Get room details (authenticated)
  - Returns: `RoomDto`
  - Returns 403 if user is not a member
  - Returns 404 if room doesn't exist
  
- `POST /api/rooms` - Create new room (authenticated)
  - Body: `CreateRoomRequest` (Name, Description, IsPrivate)
  - Returns: `RoomDto` (201 Created)
  
- `PUT /api/rooms/{roomId}` - Update room (authenticated, requires permissions)
  - Body: `UpdateRoomRequest` (Name, Description)
  - Returns: `RoomDto`
  
- `DELETE /api/rooms/{roomId}` - Delete room (authenticated, admin only)
  - Returns: 204 No Content
  
- `GET /api/rooms/{roomId}/messages` - Get paginated messages (authenticated)
  - Query params: `page` (default: 1), `pageSize` (default: 50)
  - Returns: `PaginatedResponse<MessageDto>`
  
- `POST /api/rooms/{roomId}/members` - Add member to room (authenticated, requires permissions)
  - Body: `AddRoomMemberRequest` (UserId, Role)
  - Returns: Success message (201 Created)
  
- `DELETE /api/rooms/{roomId}/members/{userId}` - Remove member from room (authenticated, requires permissions)
  - Returns: 204 No Content

### Users (`/api/users`)
- `GET /api/users/me` - Get current user profile (authenticated)
  - Returns: `UserDto`
  
- `PUT /api/users/me` - Update current user profile (authenticated)
  - Body: `UpdateUserRequest`
  - Returns: `UserDto`
  
- `GET /api/users/{userId}` - Get user by ID (authenticated)
  - Returns: `UserDto`
  - Returns 404 if user doesn't exist
  
- `GET /api/users/search?query={query}` - Search users (authenticated)
  - Returns: `List<UserDto>`
  
- `GET /api/users` - Get all users (authenticated)
  - Returns: `List<UserDto>`

### SignalR Hub (`/chatHub`)

**Connection:**
- Requires JWT authentication via query parameter: `?access_token={token}`
- Automatic connection status tracking
- Automatic user status updates on connect/disconnect

**Client Methods (Send to Server):**
- `JoinRoom(int roomId)` - Join a room group
  - Verifies user membership before joining
  - Broadcasts `UserJoinedRoom` event to others in room
  
- `LeaveRoom(int roomId)` - Leave a room group
  - Broadcasts `UserLeftRoom` event to others in room
  
- `SendMessage(int roomId, string content)` - Send a message
  - Creates message in database
  - Broadcasts `ReceiveMessage` event to all in room
  
- `EditMessage(int messageId, string newContent)` - Edit a message
  - Only message owner can edit
  - Broadcasts `MessageEdited` event to all in room
  
- `DeleteMessage(int messageId)` - Delete a message
  - Only message owner can delete
  - Broadcasts `MessageDeleted` event to all in room
  
- `StartTyping(int roomId)` - Start typing indicator
  - Broadcasts `UserStartedTyping` event to others in room
  
- `StopTyping(int roomId)` - Stop typing indicator
  - Broadcasts `UserStoppedTyping` event to others in room

**Server Events (Received by Client):**
- `ReceiveMessage` - New message received
  - Payload: `MessageDto`
  
- `MessageEdited` - Message was edited
  - Payload: `MessageEditedDto` (Id, Content, EditedAt)
  
- `MessageDeleted` - Message was deleted
  - Payload: `MessageDeletedDto` (Id, RoomId)
  
- `UserJoinedRoom` - User joined the room
  - Payload: `RoomEventDto` (UserId, RoomId, Timestamp)
  
- `UserLeftRoom` - User left the room
  - Payload: `RoomEventDto` (UserId, RoomId, Timestamp)
  
- `UserStartedTyping` - User started typing
  - Payload: `TypingIndicatorDto` (UserId, Username, RoomId)
  
- `UserStoppedTyping` - User stopped typing
  - Payload: `TypingIndicatorDto` (UserId, RoomId)
  
- `UserStatusChanged` - User online/offline status changed
  - Payload: `UserStatusChangedDto` (UserId, IsOnline, LastSeen)
  - Broadcast to all connected clients

## Frontend Features

### Pages
- **Login Page** - User authentication
- **Registration Page** - New user registration
- **Chat Page** - Main application interface with room list and chat area

### Key Components
- **ChatLayout** - Main layout component with sidebar and chat area
- **RoomList** - Displays user's rooms in sidebar
- **MessageList** - Displays messages in current room
- **MessageInput** - Input component for sending messages
- **ProtectedRoute** - Route guard for authenticated routes

### Contexts
- **AuthContext** - Manages authentication state, tokens, and user information
- **ChatContext** - Manages chat state, rooms, messages, and SignalR connection

### Features
- JWT token storage and management in memory/context
- Automatic token refresh on expiry
- Real-time message updates via SignalR
- Room selection and switching
- Message sending with real-time delivery
- Connection status indication
- Automatic SignalR reconnection on disconnect
- Responsive UI with Tailwind CSS

## Security Features
- Password hashing with BCrypt
- JWT token authentication for REST API
- Refresh token rotation support
- CORS configuration for specific frontend origins
- Authorization checks for room operations
- User ownership verification for message operations
- Room membership verification for access control
- Exception handling middleware for consistent error responses

## Development Features
- Swagger/OpenAPI documentation (available in development environment)
- Database migrations with automatic application on startup
- Database seeding with sample data (if database is empty)
- Exception handling middleware
- Comprehensive logging throughout application

## Sample Data
The application seeds the database with:
- **3 sample users:**
  - Username: `aya`, Email: `aya@test.com`, Password: `test123`
  - Username: `bobby`, Email: `bobby@test.com`, Password: `test123`
  - Username: `carlos`, Email: `carlos@test.com`, Password: `test123`
  
- **3 sample rooms:**
  - "General" (public) - General discussion room
  - "Bachata" (public) - Bachata chat and fun
  - "Gym bros" (private) - Chat about gym and fitness
  
- Sample room memberships with different roles
- Sample messages across rooms

## Configuration

### Database
- PostgreSQL connection via connection string
- Automatic migrations on application startup
- Seeding runs automatically if database is empty

### JWT Settings
- Secret key configuration
- Issuer: `ChatApp.API.Dev`
- Audience: `ChatApp.Web.Dev`
- Access token expiry: 24 hours (1440 minutes)
- Refresh token expiry: 7 days

### CORS
- Allowed origins: `http://localhost:3000`, `http://localhost:5173`
- Allows credentials
- Allows any header and method

### Environment
- Development vs production environment configuration
- Swagger UI available in development only

## Technical Constraints

### Field Limits
- Maximum message content length: **2000 characters**
- Maximum username length: **50 characters**
- Maximum email length: **255 characters**
- Maximum room name length: **100 characters**
- Maximum room description length: **500 characters**
- Maximum emoji length (reactions): **10 characters**
- Maximum attachment URL length: **500 characters**
- Maximum attachment filename length: **255 characters**

### Defaults
- Default message pagination: **50 messages per page**
- JWT access token expiry: **24 hours**
- Refresh token expiry: **7 days**

### Database Relationships
- User deletion cascades to messages and room memberships
- Room deletion cascades to messages and room memberships
- Message deletion cascades to reactions
- Room creator deletion is restricted (prevents orphaned rooms)

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**Note:** This specification documents the current state of the application as implemented. Future planned features or changes are not included in this document.

