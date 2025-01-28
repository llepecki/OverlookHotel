# Welcome to the Overlook Hotel

**Overlook Hotel** is a command-line application designed to manage hotel room availability and bookings. It allows users to query room availability and search for available rooms over a specified number of days.

## Features

- **Query room availability**: Check available rooms for specific dates and room types.
- **Search for rooms**: Search for available rooms over a range of days.
- **Data initialization**: Load hotel and booking data from JSON files into a SQLite database.
- **Structure**: Modular projects for scalability and maintainability.

## Usage

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Installation

1. **Clone the repository**:

```shell
    git clone https://github.com/llepecki/OverlookHotel.git
    cd OverlookHotel
```

2. **Restore dependencies**:

```shell
  dotnet restore
```

### Running the application

1. **Navigate to the Runner project**:

```shell
  cd src/OverlookHotel.Runner
```

2. **Run the application**:

```shell
  dotnet run --hotels hotels.json --bookings bookings.json
```

### Executing commands

Once the application is running, you can execute the following commands:

```shell
    Command > Availability(H22, 20250301, SGL) [enter]
    Command > Availability(H22, 20250301-20250305, DBL) [enter]
    Command > Search(H22, 10, SGL) [enter]
```

## Architectural decisions

### Loading data into SQLite

JSON format isn’t ideal for querying data efficiently. Although the additional step of reading data from JSON files and inserting them into the database introduces some complexity, it proves beneficial in the long run by simplifying data queries.

### MediatR handlers

While the application is relatively simple and using MediatR might initially seem like an overkill, it enhances readability and encapsulates business logic into easily testable components.

## Design

### 1. **DbInit**

- **Purpose**: Handles the initialization of the SQLite database.
- **Responsibilities**:
  - **Data loading**: Reads input data from JSON files (`hotels.json`, `bookings.json`).
  - **Database setup**: Creates necessary tables and inserts data using Dapper.
  - **Scripts management**: Contains SQL scripts for table creation and data insertion.

### 2. **Runner**

- **Purpose**: Application host and the entry point.
- **Responsibilities**:
  - **Command-line interface**: Provides an interface for users to input commands.
  - **Command execution**: Parses and executes user commands by interacting with the Application layer.
  - **Output display**: Displays the results of commands to the user.

### 3. **Application**

- **Purpose**: Implements the business logic and core functionalities.
- **Responsibilities**:
  - **Business logic**: Contains handlers for `Availability` and `Search` commands.
  - **Data access**: Interacts with the SQLite database to fetch and manipulate data.
  - **Validation**: Includes validators to ensure the integrity of incoming queries.
  - **Testing**: Has unit tests to validate the correctness of the business logic.

### **Data Flow**

1. **Initialization**:
    - `DbInit` reads `hotels.json` and `bookings.json` and populates the SQLite database.

2. **User interaction**:
    - User runs the application via the `Runner` project.
    - Command is entered through the command-line interface.

3. **Command processing**:
    - The `Application` layer processes commands, performs validations and executes business logic.
    - Results are fetched from the database and returned to the user through the `Runner`.

### **Technologies Used**

- **.NET 8.0**
- **SQLite**: Lightweight relational database to store data.
- **Dapper**: Simple ORM for database interactions.
- **MediatR**: Implements the Mediator pattern for handling queries.
- **xUnit**: Testing framework for unit tests.
- **Moq**: Mocking library for creating mock objects in tests.
- **CSharpFunctionalExtensions**: Provides the `Result` type for functional error handling. I prefer returning results to throwing exceptions.

## AI Support

I've used **ChatGPT o1** to assist me in writing tedious pieces of code.

- **Generating DTOs**: Created initial structures for deserializing JSON input data.
- **Database schema design**: SQL queries for table creation and data insertion compatible with Dapper.
- **Command parsing logic**: Developed parsers for interpreting user input commands and mapping them to corresponding actions.
- **Test data generation**: Produced sample JSON files (`testHotels.json`, `testBookings.json`) based on specified constraints.

*Note: All AI-generated code (including this sentence *:wink:*) was thoroughly reviewed.*

## Future Improvements

While the **Overlook Hotel** currently meets the core requirements, there are several areas for enhancement:

### 1. **Case insensitivity in commands**

- **Current state**: The application treats user input as case-sensitive.
- **Enhancement**: Modify the command parser to handle case-insensitive inputs.

### 2. **Optimized database connection management**

- **Current state**: Database connections are opened and closed for each operation, which may impact performance.
- **Enhancement**: Implement a connection pooling mechanism to reuse open connections.
