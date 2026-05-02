# HireMinds Smart Interview Platform - Backend API

This is the robust, production-ready .NET 8 Web API for the HireMinds Smart Interview Platform. It features role-based access control, caching, global exception handling, and integrates with Google Gemini for automated AI feedback on candidate test performance.

## 🚀 Tech Stack
- **Framework:** ASP.NET Core 8 Web API
- **Database:** MySQL
- **ORM:** Entity Framework Core (Database-First)
- **Authentication:** JWT (JSON Web Tokens)
- **AI Integration:** Google Gemini API
- **Caching:** In-Memory Caching (`IMemoryCache`)

## 📋 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)

## 🛠️ Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd HireMindsAPI
   ```

2. **Database Setup:**
   - Open MySQL Workbench.
   - Run the provided `SmartInterviewDB.sql` script to generate the database schema and dummy data.

3. **Configure Secrets:**
   - Create a new file named `appsettings.Development.json` in the root directory.
   - Add your database credentials, JWT secret key, and Gemini API key:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;database=SmartInterviewDB;user=root;password=YOUR_PASSWORD;"
     },
     "Jwt": {
       "Key": "YOUR_SECRET_KEY",
       "Issuer": "HireMindsAPI",
       "Audience": "HireMindsUsers",
       "ExpiryInMinutes": 60
     },
     "Gemini": {
       "ApiKey": "YOUR_GEMINI_API_KEY",
       "Model": "gemini-2.0-flash"
     }
   }
   ```
   *(Note: `appsettings.Development.json` is git-ignored to protect your secrets).*

4. **Run the API:**
   ```bash
   dotnet run
   ```
   - The API will start on `http://localhost:5034` (or similar).
   - Navigate to `http://localhost:5034/swagger` to explore and test the endpoints using Swagger UI.

## 🔗 API Endpoints

### 🔐 Authentication
- `POST /api/auth/register` - Register a new Candidate or Recruiter
- `POST /api/auth/login` - Login and receive JWT token

### 👤 Profile (Candidate Only)
- `GET /api/profile` - Get your candidate profile
- `PUT /api/profile` - Update your profile details

### 💼 Jobs
- `POST /api/jobs` - Create a new job (Recruiter only)
- `GET /api/jobs` - View all active jobs (Cached)
- `GET /api/jobs/my` - View jobs you posted (Recruiter only)
- `GET /api/jobs/{id}` - View specific job details

### 📝 Tests & Assessments
- `POST /api/tests` - Create a new test (Recruiter only)
- `POST /api/tests/{id}/questions` - Add a question to a test (Recruiter only)
- `GET /api/tests/my` - View tests you created (Recruiter only)
- `GET /api/tests/{id}` - View a test (Correct answers hidden)

### 📄 Applications
- `POST /api/applications/{jobId}/apply` - Apply for a job (Checks eligibility)
- `GET /api/applications/my` - View jobs you applied to
- `GET /api/applications/job/{jobId}` - View candidates who applied to your job
- `PUT /api/applications/{id}/status` - Update application status (Selected/Rejected)

### 🎓 Test Submission & AI Feedback
- `POST /api/submit/{applicationId}` - Submit test answers (Auto-grades and calls AI)
- `GET /api/result/{applicationId}` - View score breakdown and AI feedback
