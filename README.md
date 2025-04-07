## Backend Setup and Running Instructions

### Prerequisites
1. Install [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0).
2. Install a SQL Server instance (e.g., [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)).

### Steps to Start the Backend
1. Navigate to the `backend` directory:
   ```bash
   cd backend
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the backend project:
   ```bash
   dotnet run
   ```

5. The backend will start on `https://localhost:7170` (HTTPS) or `http://localhost:5162` (HTTP).

### API Documentation
- Open your browser and navigate to `https://localhost:7170/swagger` or `http://localhost:5162/swagger` to access the API documentation.

### Frontend Setup and Running Instructions

### Prerequisites
1. Install [Node.js](https://nodejs.org/) (LTS version recommended).
2. Install Angular CLI globally:
   ```bash
   npm install -g @angular/cli
   ```

### Steps to Start the Frontend
1. Navigate to the `frontend` directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   ng serve
   ```

4. Open your browser and navigate to `http://localhost:4200` to view the application.

### Database Diagram
The database diagram for this project can be found in:
- Location: `CodeTemplates/DbContext.mermaid.t4`
- Format: Mermaid diagram
- This diagram provides a visual representation of the database schema including tables, relationships, and fields.

To view the diagram:
1. Install a Mermaid viewer extension in VS Code
2. Open the `DbContext.mermaid.t4` file
3. Use the preview feature to visualize the database structure