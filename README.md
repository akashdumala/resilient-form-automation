
# Resilient Form Automation Using C# and Selenium

## Overview

This project automates three form fields on the CloudQA Automation Practice Form using C# and Selenium WebDriver.

The solution focuses on identifying web elements even when their HTML attributes and page structure change.

## Technologies Used

- C#
- .NET 10
- Selenium WebDriver 4.52.0
- Selenium Support
- Google Chrome

## Features

- Automated First Name text entry.
- Automated Gender radio button selection.
- Automated State dropdown selection.
- Semantic label-based element identification.
- Configurable target URL.
- HTML attribute mutation testing.
- Additional wrapper and DOM structure mutation testing.
- Automated verification of entered values.

## Project Structure

Task1-Selenium-CSharp/
|
|-- Program.cs
|-- Task1-Selenium-CSharp.csproj
|-- README.md

## Installation

1. Install .NET 10 SDK.
2. Install Google Chrome.
3. Clone this repository.
4. Open the project folder in VS Code or a terminal.

Install dependencies:

```powershell
dotnet restore
```

## Run the Project

Build:

```powershell
dotnet build
```

Run using the default URL:

```powershell
dotnet run
```

Run using a configurable URL:

```powershell
dotnet run -- "https://app.cloudqa.io/home/AutomationPracticeForm"
```

You can also configure the URL using the TARGET_URL environment variable.

## Testing Approach

The application performs the following tests:

1. Opens the target webpage.
2. Changes selected HTML attributes.
3. Adds additional HTML wrappers.
4. Reorganizes selected form groups.
5. Identifies fields using visible semantic labels.
6. Enters the First Name.
7. Selects the Gender radio button.
8. Selects India in the State dropdown.
9. Verifies all three field values.

## Limitations

- The current mutation tests operate on the CloudQA practice form.
- Semantic identification depends on visible label text and nearby DOM relationships.
- Different websites may require additional locator strategies.
- Arbitrary changes to page semantics or inaccessible controls may require manual adaptation.
- The mutation test is not a substitute for testing against independently modified websites.

## Author

Dumala Akash
