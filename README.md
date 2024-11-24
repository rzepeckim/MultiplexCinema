
# Multiplex Cinema Management System

## Overview

The **Multiplex Cinema Management System** is a web-based application that allows administrators to manage movie screenings, auditoriums, ticket sales, and user accounts. Users can register, purchase tickets, and view their bookings. Administrators can monitor revenue and manage screenings.

---

## Features

1. **User Management**: Registration, login, and role-based access control using ASP.NET Identity.
2. **Movie Management**: Add, edit, and delete movies with descriptions and genres.
3. **Screening Management**: Manage screenings for movies, including timing, auditorium allocation, and seat availability.
4. **Ticket Sales**: Users can purchase tickets for available screenings.
5. **Revenue Monitoring**: Admins can view total revenue generated from ticket sales.
6. **Automatic Screening Status Updates**: Screenings automatically update to "Completed" once their scheduled time passes.
7. **Data Archiving**: Completed screenings are archived automatically.

---

## Database Schema

Below is a simplified schema of the database used in the system:

### Tables

#### `ApplicationUser`
- **Id** (PK): Unique identifier for users.
- **FullName**: The user's full name.
- **Email**: User's email address (used for login).
- **PasswordHash**: Hashed password for secure authentication.

#### `Movies`
- **Id** (PK): Unique identifier for movies.
- **Title**: Title of the movie.
- **Description**: Description of the movie.
- **Duration**: Duration in minutes.
- **Genre**: Genre of the movie.

#### `Auditoriums`
- **Id** (PK): Unique identifier for auditoriums.
- **Name**: Name of the auditorium.
- **SeatCount**: Total number of seats available.

#### `Screenings`
- **Id** (PK): Unique identifier for screenings.
- **MovieId** (FK): Reference to the `Movies` table.
- **AuditoriumId** (FK): Reference to the `Auditoriums` table.
- **ScreeningTime**: Date and time of the screening.
- **Status**: Current status (`Not Started`, `Ongoing`, `Completed`).

#### `Tickets`
- **Id** (PK): Unique identifier for tickets.
- **ScreeningId** (FK): Reference to the `Screenings` table.
- **UserId** (FK): Reference to the `ApplicationUser` table.
- **SeatNumber**: Seat number.
- **Price**: Ticket price.
- **TicketType**: Type of ticket (`Normal` or `Discounted`).

#### `ArchivedScreenings`
- **Id** (PK): Unique identifier for archived screenings.
- **MovieId**: Movie associated with the screening.
- **ScreeningTime**: Screening date and time.
- **AuditoriumId**: Auditorium used.
- **Status**: Status of the screening (`Completed`).
- **CreatedAt**: Timestamp of archive creation.
- **UpdatedAt**: Timestamp of last update.

---

## Stored Procedures and Functions

### 1. **Stored Procedure to Update Screening Status**
Updates the status of a screening to `Completed` if the scheduled time has passed.

```sql
CREATE PROCEDURE UpdateScreeningStatus
    @ScreeningId INT
AS
BEGIN
    UPDATE Screenings
    SET Status = 'Completed'
    WHERE Id = @ScreeningId AND ScreeningTime < GETDATE();
END;
```

### 2. **Function to Calculate Total Revenue**
Calculates the total revenue generated from ticket sales.

```sql
CREATE FUNCTION dbo.GetTotalRevenue()
RETURNS DECIMAL(18, 2)
AS
BEGIN
    DECLARE @TotalRevenue DECIMAL(18, 2);
    SELECT @TotalRevenue = SUM(Price)
    FROM Tickets;
    RETURN @TotalRevenue;
END;
```

### 3. **Trigger to Archive Completed Screenings**
Automatically moves completed screenings to the `ArchivedScreenings` table.

```sql
CREATE TRIGGER ArchiveCompletedScreenings
ON Screenings
AFTER UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM INSERTED i 
        INNER JOIN DELETED d 
        ON i.Id = d.Id 
        WHERE i.Status = 'Completed' AND d.Status != 'Completed'
    )
    BEGIN
        INSERT INTO ArchivedScreenings (Id, MovieId, ScreeningTime, AuditoriumId, Status, CreatedAt, UpdatedAt)
        SELECT i.Id, i.MovieId, i.ScreeningTime, i.AuditoriumId, i.Status, GETDATE(), GETDATE()
        FROM INSERTED i
        INNER JOIN DELETED d 
        ON i.Id = d.Id
        WHERE i.Status = 'Completed' AND d.Status != 'Completed';

        DELETE FROM Screenings
        WHERE Id IN (
            SELECT Id 
            FROM INSERTED 
            WHERE Status = 'Completed'
        );
    END;
END;
```

### **ArchivedScreenings Table**
Stores archived screening data.

```sql
CREATE TABLE ArchivedScreenings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MovieId INT NOT NULL,
    ScreeningTime DATETIME NOT NULL,
    AuditoriumId INT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MovieId) REFERENCES Movies(Id) ON DELETE CASCADE,
    FOREIGN KEY (AuditoriumId) REFERENCES Auditoriums(Id) ON DELETE CASCADE
);
```
### **5. Subquery to retrieve screenings for a specific movie**
I chose not to use a SQL subquery because the operation can be efficiently performed using Entity Framework's LINQ, which provides a more straightforward and readable approach.

```csharp
[HttpGet]
public IActionResult GetScreeningsForMovie(int movieId)
{
    var screenings = _context.Screenings
        .Where(s => s.MovieId == movieId)
        .Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.ScreeningTime:dd/MM/yyyy HH:mm} - Sala: {s.Auditorium.Name}"
        })
        .ToList();

return Json(screenings);
}
```
SQL Equivalent
```sql
SELECT 
    s.Id, 
    s.ScreeningTime,
    (SELECT a.Name FROM Auditoriums a WHERE a.Id = s.AuditoriumId) AS AuditoriumName
FROM Screenings s
WHERE s.MovieId = @movieId;

```
---

## API Endpoints

### Login and Registration Module

#### POST `/Account/Register`
- **Description**: Registers a new user.
- **Parameters**:
    - `Username`: string
    - `Password`: string
    - `ConfirmPassword`: string
- **Response**: HTTP status (201 on success).

#### POST `/Account/Login`
- **Description**: Logs in a user and creates a session.
- **Parameters**:
    - `Username`: string
    - `Password`: string
- **Response**: HTTP status (200 on success).

#### POST `/Account/Logout`
- **Description**: Logs out the user.
- **Response**: HTTP status (200).

---

### Data Management Module

#### GET `/Movies`
- **Description**: Retrieves a list of movies.
- **Response**: JSON with a list of movies.

#### POST `/Movies`
- **Description**: Adds a new movie.
- **Parameters**:
    - `Title`: string
    - `Director`: string
    - `ReleaseDate`: datetime
- **Response**: HTTP status (201).

#### PUT `/Movies/{id}`
- **Description**: Updates movie details.
- **Parameters**:
    - `Title`: string
    - `Director`: string
    - `ReleaseDate`: datetime
- **Response**: HTTP status (200).

#### DELETE `/Movies/{id}`
- **Description**: Deletes a movie with the specified `id`.
- **Response**: HTTP status (204).

#### GET `/Screenings`
- **Description**: Retrieves a list of screenings.
- **Response**: JSON with a list of screenings.

#### GET `/Screenings/GetScreeningsForMovie?movieId={id}`
- **Description**: Retrieves available screenings for a specific movie.
- **Parameters**:
    - `movieId`: integer (query parameter).
- **Response**: JSON containing the screenings for the movie, formatted for use in a dropdown (e.g., with screening time and auditorium name).

---



## Security Features
1. **User Authentication**: Implemented using **ASP.NET Identity** with password hashing.
2. **Role-Based Authorization**: Access to certain functionalities is restricted based on user roles (e.g., admin-only features).
3. **Data Validation**: Ensures that all user inputs are properly validated to prevent SQL injection and XSS attacks.
4. **Encrypted Connections**: Uses HTTPS for secure communication.
5. **Use of Cookies**: Cookies are used to manage user sessions and store authentication tokens after login.

---




---
