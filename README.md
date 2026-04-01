# Email OTP API

A simple and secure Email OTP (One-Time Password) API built using .NET.
This project demonstrates OTP generation, email sending, and verification using a clean layered architecture.

---

## Overview

This application provides functionality to:

* Generate a one-time password (OTP)
* Send the OTP to a user via email (simulated)
* Verify the OTP within a defined validity period

The project is structured to follow separation of concerns and can be extended for production use.

---

## Features

* 6-digit OTP generation
* OTP expiry (1 minute)
* Email sending (simulated via console output)
* OTP verification
* Layered architecture
* Unit testing support

---

## Technology Stack

* .NET 8 (ASP.NET Core Web API)
* C#
* xUnit for testing

---

## Project Structure

EmailOtpSolution

* EmailOtpApi
  API layer containing controllers and endpoints

* EmailOtpCore
  Business logic and domain services

* EmailOtpInfrastructure
  Email service and data handling

* EmailOtpModule.Tests
  Unit tests

* EmailOtpSolution.sln
  Solution file

---

## Application Flow

1. A user submits a request to generate an OTP
2. The system generates a random 6-digit OTP
3. The OTP is stored temporarily in memory with an expiry time
4. A simulated email is sent containing the OTP
5. The user submits the OTP for verification
6. The system validates the OTP and checks its expiry

---

## API Endpoints

### Generate OTP

POST /api/otp/generate

Request:

```json
{
  "email": "user@example.com"
}
```

Response:

```json
{
  "message": "OTP sent successfully"
}
```

---

### Verify OTP

POST /api/otp/verify

Request:

```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

Response:

```json
{
  "message": "OTP verified successfully"
}
```

---

## Sample Email Output

==== Simulated Email Sender ====
To: [user@example.com](mailto:user@example.com)
Body: Your OTP Code is 582143. The code is valid for 1 minute
=============================================================

---

## Running the Application

1. Clone the repository

git clone https://github.com/YOUR_USERNAME/EmailOtpSolution.git

2. Open the solution in Visual Studio

3. Set EmailOtpApi as the startup project

4. Run the application

5. Open Swagger in browser

[https://localhost:{port}/swagger](https://localhost:{port}/swagger)

---

## Running Tests

dotnet test

---

## Notes

* Email sending is currently simulated (console output)
* OTP data is stored in memory and will be lost when the application stops
* This project is intended for demonstration and learning purposes

---

## Future Enhancements

* Integrate with real email providers (SMTP, SendGrid)
* Use Redis or database for OTP storage
* Implement rate limiting
* Add authentication and authorization (JWT)
* Add logging and monitoring

---

## Author

Jayson Chua
