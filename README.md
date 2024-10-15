# Business ProgressSoft API
This API allows users to manage business cards, including functionalities for creating, retrieving, updating, and deleting cards. The API also supports importing and exporting business cards in CSV and XML formats.

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Implementation](#implementation)
- [Endpoints](#endpoints)
- [SQL code](#sqlcode)
- [Unit testing](#unit testing)
- [Images](#images)
- [Video](#video)

## Features
- CRUD operations for business cards.
- Import and export business cards in CSV and XML formats.
- Image upload functionality for card photos.
- Filtering business cards by email.

## Installation
- .NET 6.0 SDK
## Usage
- Use tools like Postman or swagger to interact with the API.

  Ex:
  Get all business cards
  ```
  Get /api/Bcards/GetCard
  ```
  Ex: import file CSV
    ```
  POST /api/Bcards/CreateCardFromCSV
  ```

## Implementation
The API is built using ASP.NET Core and follows a standard RESTful architecture. The key components of the implementation include:
- Data Access Layer: Utilizes Entity Framework Core for database interactions.
- Services: Interfaces for CSV and card operations are implemented to handle business logic and data parsing.
- Controllers: The BcardsController manages the HTTP requests and responses, coordinating with the services for data processing.

## Database Context
The API uses the BusinessProgressSoftContext class, which inherits from DbContext, to interact with the database.

## Model
The Bcard model defines the structure of a business card and includes properties such as Name, Gender, Birthdate, Email, Phone, Photo, and Address.

## Endpoints
Here's a summary of the available API endpoints:

| Method | Endpoint                       | Description                                   |
|--------|---------------------------------|-----------------------------------------------|
| GET    | `/api/Bcards/GetCard`           | Retrieve all business cards.                  |
| GET    | `/api/Bcards/GetById/{id}`      | Retrieve a business card by its ID.           |
| PUT    | `/api/Bcards/UpdateCard`        | Update a specific business card.              |
| POST   | `/api/Bcards/CreateCardFromCSV` | Create business cards from a CSV file.        |
| POST   | `/api/Bcards/InsertCard`        | Insert a new business card.                   |
| DELETE | `/api/Bcards/DeleteCard/{id}`   | Delete a business card by its ID.             |
| GET    | `/api/Bcards/ExportCardsToCSV`  | Export all business cards to a CSV file.      |
| GET    | `/api/Bcards/ExportCardsToXML`  | Export all business cards to an XML file.     |
| POST   | `/api/Bcards/InsertCardFromXML` | Create business cards from an XML file.       |


## SQL Code

```
CREATE TABLE BCard
(
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100),
    gender CHAR(1),
    birth DATE ,
    email VARCHAR(100) ,
    phone VARCHAR(15),
    photo VARCHAR(MAX), 
    address VARCHAR(255) 
);

```

## Unit testing
Implemented unit tests using XUnit, Moq, and FakeItEasy to ensure core functionalities work as expected:
- GetCards_ReturnsOkResult_WithListOfCards: Ensures that the GetBcards method returns a valid list of business cards.

- InsertCard_AddNewCard_ReturnNewCard: Tests successful insertion of a new business card.

- CreateCardFromCSV_ReturnsBadRequest_WhenFileIsInvalid: Verifies that invalid CSV file uploads return a BadRequest.

- CreateCardFromCSV_ReturnsOk_WhenFileIsValid: Ensures valid CSV files are processed and cards are added.

- CreateCardFromXML_ValidFile_ReturnsOkResult: Tests successful addition of cards from a valid XML file.

- DeleteCard_ReturnsNotFound_WhenCardsNull: Ensures deleting a non-existent card returns NotFound.

- ExportCardToCSV_ReturnsCSVFile: Verifies the export of business cards to CSV format.

- ExportCardsToXML_ReturnsXMLFile: Ensures that cards are exported in XML format properly.

