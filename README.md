# Product Description API

This API provides functionalities to generate and assess product descriptions. It includes endpoints for constraint assessment, ethical assessment, language assessment, and generating product descriptions using OpenAI.

## Table of Contents

- [Product Description API](#product-description-api)
  - [Table of Contents](#table-of-contents)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
  - [Configuration](#configuration)
  - [Endpoints](#endpoints)
    - [Constraint Assessment](#constraint-assessment)
    - [Ethical Assessment](#ethical-assessment)
    - [Language Assessment](#language-assessment)
    - [Generate Product Description](#generate-product-description)
  - [Error Handling](#error-handling)
  - [Front-End Integration](#front-end-integration)
    - [Prerequisites](#prerequisites-1)
    - [Installation](#installation-1)
    - [Usage](#usage)
    - [Configuration](#configuration-1)
    - [Components](#components)
    - [Services](#services)

## Getting Started

### Prerequisites

- .NET SDK (version 8.0 or later)
- OpenAI API key

### Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/yourusername/product-description-api.git
   cd product-description-api
   ```

2. Restore the dependencies:
   ```sh
   dotnet restore
   ```

3. Build the project:
   ```sh
   dotnet build
   ```

4. Run the project:
   ```sh
   dotnet run
   ```

## Configuration

The application requires configuration for the OpenAI API and base URL. Add the following settings to your `appsettings.json` file:

```json
{
  "APIBaseUrl": "http://localhost:5001",
  "OpenAIServiceOptions": {
    "ApiKey": "your_openai_api_key"
  }
}
```

## Endpoints

### Constraint Assessment

Assess the product description against provided product attributes.

- **URL:** `/constraints-assessment/assess`
- **Method:** `POST`
- **Request Body:**
  ```json
  {
    "Description": "Product description text",
    "Attributes": ["attribute1", "attribute2", ...]
  }
  ```
- **Responses:**
  - `200 OK`: Returns the assessment result.
  - `400 Bad Request`: Invalid request payload.
  - `500 Internal Server Error`: Error assessing product description.

### Ethical Assessment

Assess the product description for ethical marketing.

- **URL:** `/ethics-assessment/assess`
- **Method:** `POST`
- **Request Body:**
  ```json
  {
    "Description": "Product description text"
  }
  ```
- **Responses:**
  - `200 OK`: Returns the assessment result.
  - `400 Bad Request`: Invalid request payload.
  - `500 Internal Server Error`: Error assessing product description.

### Language Assessment

Assess the product description for language correctness.

- **URL:** `/language-assessment/assess`
- **Method:** `POST`
- **Request Body:**
  ```json
  {
    "Description": "Product description text"
  }
  ```
- **Responses:**
  - `200 OK`: Returns the assessment result.
  - `400 Bad Request`: Invalid request payload.
  - `500 Internal Server Error`: Error assessing product description.

### Generate Product Description

Generate a product description based on provided attributes.

- **URL:** `/product-description/generate`
- **Method:** `POST`
- **Request Body:**
  ```json
  {
    "productAttributes": "___Product Name___Keywords",
    "temperature": "1.0"
  }
  ```
- **Responses:**
  - `200 OK`: Returns the generated product description.
  - `400 Bad Request`: Invalid request payload.
  - `500 Internal Server Error`: Error generating product description.

## Error Handling

The API provides detailed error responses to help diagnose issues:

- **400 Bad Request:** Indicates that the request payload is invalid.
- **500 Internal Server Error:** Indicates an error occurred while processing the request. The error is logged for further analysis.

## Front-End Integration

The front-end part of this project is built using Blazor and integrates with the API to generate product descriptions. Follow these steps to set up and run the front-end project:

### Prerequisites

- .NET SDK (version 8.0 or later)

### Installation

1. Navigate to the front-end project directory:
   ```sh
   cd ProductDescriptionFrontend
   ```

2. Restore the dependencies:
   ```sh
   dotnet restore
   ```

3. Build the project:
   ```sh
   dotnet build
   ```

4. Run the project:
   ```sh
   dotnet run
   ```

### Usage

- Navigate to `http://localhost:5233/generate-product-description` in your web browser.
- Enter the product name and attributes in the provided fields.
- Click the "Generate Description" button to get the generated product description.

### Configuration

The front-end project requires the API base URL to be set in the configuration. Add the following settings to your `appsettings.json` file in the `ProductDescriptionFrontend` project:

```json
{
  "APIBaseUrl": "http://localhost:5001"
}
```

### Components

The front-end project includes the following key components:

- `GenerateDescriptionBtn`: Component for generating product descriptions.
- `TextInput`: Component for input fields.
- `TextArea`: Component for text areas.
- `LoadingSpinner`: Component for displaying a loading spinner while the request is being processed.

### Services

- `ProductDescriptionService`: Service for making API calls to generate product descriptions.

