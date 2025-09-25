# MemeGen

MemeGen is a distributed application for meme generation and management.  
It combines multiple .NET microservices, React-based frontends, and shared infrastructure into a single solution orchestrated with [ASP.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/).

---

## 📂 Project Structure

- **MemeGen.sln** – main solution file  
- **MemeGen.AppHost** – Aspire host project for orchestrating services  
- **MemeGen.AdminApiService** – API for admin features  
- **MemeGen.ClientApiService** – API for client-facing features  
- **MemeGen.ImageProcessor** – service responsible for image processing and meme generation  
- **MemeGen.AzureTablesConfigurationService** – configuration storage using Azure Tables  
- **MemeGen.MongoDbService** – persistence service backed by MongoDB  
- **MemeGen.Domain / Common / Contracts** – shared domain models and contracts  
- **MemeGen.Lcm / MemeGen.ServiceDefaults** – configuration management and service defaults  
- **MemeGen.Playground** – sandbox for experiments and testing  
- **meme-gen-admin-frontend** – admin web application (React)  
- **meme-gen-client-frontend** – client-facing web application (React)  

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- [Node.js](https://nodejs.org/) & npm/yarn (for frontends)  
- [Docker](https://www.docker.com/) (for local infrastructure like MongoDB, Azure Tables emulator)  

### Clone & Build
```bash
git clone https://github.com/bice4/memeGen.git
cd memeGen
```
# Restore and build backend
`dotnet build MemeGen.sln`

# Run Backend
`dotnet run --project MemeGen.AppHost`
