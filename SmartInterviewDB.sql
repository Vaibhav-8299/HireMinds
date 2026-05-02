DROP DATABASE IF EXISTS SmartInterviewDB;
CREATE DATABASE SmartInterviewDB;
USE SmartInterviewDB;

CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(500) NOT NULL,
    Role ENUM('Candidate', 'Recruiter') NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE CandidateProfiles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    Degree VARCHAR(50),
    Branch VARCHAR(50),
    GraduationYear INT,
    Skills VARCHAR(500),
    CollegeName VARCHAR(200),
    SGPA DECIMAL(4,2),
    TenthSchoolName VARCHAR(200),
    TenthScore DECIMAL(5,2),
    TwelfthSchoolName VARCHAR(200),
    TwelfthScore DECIMAL(5,2),
    ResumeUrl VARCHAR(500),
    Certifications TEXT,
    Projects TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Tests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RecruiterId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    TimeLimitMinutes INT NOT NULL DEFAULT 30,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RecruiterId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Questions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TestId INT NOT NULL,
    Text VARCHAR(1000) NOT NULL,
    Topic VARCHAR(100) NOT NULL,
    Difficulty ENUM('Easy', 'Medium', 'Hard') DEFAULT 'Medium',
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
);

CREATE TABLE `Options` (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuestionId INT NOT NULL,
    Text VARCHAR(500) NOT NULL,
    IsCorrect TINYINT(1) NOT NULL DEFAULT 0,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);

CREATE TABLE Jobs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RecruiterId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(2000),
    CompanyName VARCHAR(200) NOT NULL,
    Location VARCHAR(200) NOT NULL,
    RequiredDegree VARCHAR(50),
    RequiredBranch VARCHAR(50),
    TestId INT,
    IsActive TINYINT(1) DEFAULT 1,
    PostedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RecruiterId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE SET NULL
);

CREATE TABLE Applications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CandidateId INT NOT NULL,
    JobId INT NOT NULL,
    Status ENUM('Applied', 'TestPending', 'TestTaken', 'Selected', 'Rejected') DEFAULT 'Applied',
    Score INT DEFAULT NULL,
    TotalQuestions INT DEFAULT NULL,
    AIFeedback TEXT,
    SelectionMessage TEXT DEFAULT NULL,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    TestTakenAt DATETIME DEFAULT NULL,
    FOREIGN KEY (CandidateId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_application (CandidateId, JobId)
);

CREATE TABLE Answers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationId INT NOT NULL,
    QuestionId INT NOT NULL,
    SelectedOptionId INT,
    IsCorrect TINYINT(1) DEFAULT 0,
    FOREIGN KEY (ApplicationId) REFERENCES Applications(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
    FOREIGN KEY (SelectedOptionId) REFERENCES `Options`(Id) ON DELETE SET NULL
);


INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES
('Rahul Sharma', 'rahul@gmail.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Candidate'),
('Priya Patel', 'priya@gmail.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Candidate'),
('Amit Kumar', 'amit@gmail.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Candidate'),
('Sneha Gupta', 'sneha@gmail.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Candidate'),
('Vikram Singh', 'vikram@gmail.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Candidate'),
('Anita Desai', 'anita@techcorp.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Recruiter'),
('Rajesh Mehta', 'rajesh@infosys.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Recruiter'),
('Kavita Nair', 'kavita@wipro.com', '$2a$11$dummyhashpassword1234567890abcdefghij', 'Recruiter');


INSERT INTO CandidateProfiles (UserId, Degree, Branch, GraduationYear, Skills) VALUES
(1, 'B.Tech', 'CS', 2024, 'C#,Angular,.NET Core,SQL Server'),
(2, 'B.Tech', 'IT', 2024, 'Java,React,Spring Boot,MySQL'),
(3, 'M.Tech', 'CS', 2023, 'Python,Django,Machine Learning,PostgreSQL'),
(4, 'BCA', 'CS', 2025, 'HTML,CSS,JavaScript,Angular'),
(5, 'B.Tech', 'ECE', 2024, 'Embedded C,MATLAB,IoT,Arduino');


INSERT INTO Tests (RecruiterId, Title, TimeLimitMinutes) VALUES
(6, 'ASP.NET Core Fundamentals', 30),
(6, 'Angular Developer Assessment', 25),
(7, 'Full Stack .NET + Angular Test', 45),
(7, 'SQL Server & Database Concepts', 20),
(8, 'C# Programming Basics', 30);


INSERT INTO Questions (TestId, Text, Topic, Difficulty) VALUES
(1, 'What is the purpose of middleware in ASP.NET Core?', 'ASP.NET Core', 'Medium'),
(1, 'Which method is used to register services in ASP.NET Core?', 'Dependency Injection', 'Easy'),
(1, 'What does the [Authorize] attribute do?', 'Authentication', 'Easy'),
(1, 'What is the difference between AddScoped and AddSingleton?', 'Dependency Injection', 'Medium'),
(1, 'Which HTTP status code represents Unauthorized?', 'REST API', 'Easy'),
(2, 'What is the purpose of Angular Signals?', 'Angular', 'Medium'),
(2, 'Which decorator is used to create a component in Angular?', 'Angular', 'Easy'),
(2, 'What is the purpose of HttpInterceptor?', 'Angular', 'Medium'),
(2, 'What is two-way data binding in Angular?', 'Angular', 'Easy'),
(2, 'Which Angular feature is used for form validation?', 'Angular Forms', 'Medium'),
(3, 'What is Entity Framework Core?', 'EF Core', 'Easy'),
(3, 'What is the purpose of JWT in web applications?', 'Authentication', 'Medium'),
(3, 'What is dependency injection?', 'Design Patterns', 'Easy'),
(3, 'What is the difference between Observable and Promise?', 'RxJS', 'Medium'),
(3, 'What is CORS and why is it needed?', 'Web Security', 'Medium'),
(4, 'What is a primary key in SQL?', 'SQL Basics', 'Easy'),
(4, 'What is the difference between INNER JOIN and LEFT JOIN?', 'SQL Joins', 'Medium'),
(4, 'What is normalization in databases?', 'Database Design', 'Medium'),
(4, 'What does the GROUP BY clause do?', 'SQL Queries', 'Easy'),
(4, 'What is an index in SQL Server?', 'Performance', 'Hard'),
(5, 'What is the difference between value type and reference type in C#?', 'C# Basics', 'Easy'),
(5, 'What is an interface in C#?', 'OOP', 'Medium'),
(5, 'What is LINQ in C#?', 'LINQ', 'Medium'),
(5, 'What is the purpose of async/await in C#?', 'Async Programming', 'Medium'),
(5, 'What are generics in C#?', 'C# Advanced', 'Hard');


INSERT INTO `Options` (QuestionId, Text, IsCorrect) VALUES
(1, 'To handle HTTP requests in a pipeline', 1),
(1, 'To create database tables', 0),
(1, 'To style the frontend', 0),
(1, 'To manage file uploads only', 0),
(2, 'ConfigureServices or builder.Services', 1),
(2, 'app.UseRouting()', 0),
(2, 'Main() method', 0),
(2, 'appsettings.json', 0),
(3, 'Restricts access to authenticated users', 1),
(3, 'Makes the API faster', 0),
(3, 'Enables CORS', 0),
(3, 'Logs all requests', 0),
(4, 'AddScoped creates one instance per request, AddSingleton creates one for the entire app', 1),
(4, 'They are the same', 0),
(4, 'AddScoped is for databases only', 0),
(4, 'AddSingleton creates a new instance every time', 0),
(5, '401', 1),
(5, '404', 0),
(5, '500', 0),
(5, '200', 0),
(6, 'To provide reactive state management without RxJS', 1),
(6, 'To make HTTP calls', 0),
(6, 'To create animations', 0),
(6, 'To manage routing', 0),
(7, '@Component', 1),
(7, '@Injectable', 0),
(7, '@NgModule', 0),
(7, '@Directive', 0),
(8, 'To intercept and modify HTTP requests and responses', 1),
(8, 'To create database connections', 0),
(8, 'To validate forms', 0),
(8, 'To handle routing', 0),
(9, 'Binding data between component and template in both directions', 1),
(9, 'Binding only from component to template', 0),
(9, 'Binding only from template to component', 0),
(9, 'Binding between two components', 0),
(10, 'Reactive Forms', 1),
(10, 'HttpClient', 0),
(10, 'Router', 0),
(10, 'Signals', 0),
(11, 'An ORM for .NET to work with databases using C# objects', 1),
(11, 'A JavaScript framework', 0),
(11, 'A CSS preprocessor', 0),
(11, 'A testing tool', 0),
(12, 'To securely transmit information between parties as a JSON object', 1),
(12, 'To store data in the database', 0),
(12, 'To format HTML pages', 0),
(12, 'To compress files', 0),
(13, 'A design pattern where dependencies are provided to a class rather than created inside it', 1),
(13, 'A way to inject SQL into databases', 0),
(13, 'A method to deploy applications', 0),
(13, 'A type of database', 0),
(14, 'Observable can emit multiple values over time, Promise resolves once', 1),
(14, 'They are exactly the same', 0),
(14, 'Promise is faster than Observable', 0),
(14, 'Observable is only for Angular', 0),
(15, 'A security mechanism that allows controlled access to resources from different origins', 1),
(15, 'A database management system', 0),
(15, 'A CSS framework', 0),
(15, 'A type of encryption', 0),
(16, 'A unique identifier for each row in a table', 1),
(16, 'The first column in any table', 0),
(16, 'A type of database', 0),
(16, 'A backup key', 0),
(17, 'INNER JOIN returns only matching rows, LEFT JOIN returns all rows from left table', 1),
(17, 'They are the same', 0),
(17, 'LEFT JOIN is faster', 0),
(17, 'INNER JOIN returns all rows', 0),
(18, 'The process of organizing data to reduce redundancy', 1),
(18, 'Making the database bigger', 0),
(18, 'Deleting duplicate tables', 0),
(18, 'Converting data to binary', 0),
(19, 'Groups rows that have the same values in specified columns', 1),
(19, 'Sorts the data alphabetically', 0),
(19, 'Deletes grouped data', 0),
(19, 'Creates a new table', 0),
(20, 'A data structure that improves the speed of data retrieval operations', 1),
(20, 'A type of primary key', 0),
(20, 'A backup of the database', 0),
(20, 'A stored procedure', 0),
(21, 'Value types hold data directly, reference types hold a reference to data in memory', 1),
(21, 'They are the same', 0),
(21, 'Reference types are faster', 0),
(21, 'Value types can only hold numbers', 0),
(22, 'A contract that defines a set of methods and properties that a class must implement', 1),
(22, 'A type of variable', 0),
(22, 'A database table', 0),
(22, 'A CSS class', 0),
(23, 'Language Integrated Query - allows querying data in C# using SQL-like syntax', 1),
(23, 'A database management tool', 0),
(23, 'A type of loop', 0),
(23, 'A networking protocol', 0),
(24, 'To write asynchronous code that looks and behaves like synchronous code', 1),
(24, 'To make code run faster', 0),
(24, 'To handle errors', 0),
(24, 'To create threads manually', 0),
(25, 'Classes and methods that work with any data type while providing type safety', 1),
(25, 'A type of database', 0),
(25, 'A way to generate HTML', 0),
(25, 'A testing framework', 0);


INSERT INTO Jobs (RecruiterId, Title, Description, CompanyName, Location, RequiredDegree, RequiredBranch, TestId, IsActive) VALUES
(6, 'Junior .NET Developer', 'We are looking for a passionate junior .NET developer to join our team. You will work on building web APIs using ASP.NET Core and collaborate with the frontend team.', 'TechCorp Solutions', 'Bangalore', 'B.Tech', 'CS', 1, 1),
(6, 'Angular Frontend Developer', 'Join our UI team to build modern web applications using Angular. Experience with reactive forms and RxJS is a plus.', 'TechCorp Solutions', 'Bangalore', 'B.Tech', 'IT', 2, 1),
(7, 'Full Stack .NET Developer', 'Looking for a full stack developer proficient in ASP.NET Core and Angular. Must have strong understanding of REST APIs and database design.', 'Infosys', 'Hyderabad', 'B.Tech', 'CS', 3, 1),
(7, 'Database Administrator', 'We need a DBA with strong SQL Server skills. Knowledge of performance tuning and query optimization required.', 'Infosys', 'Pune', 'M.Tech', 'CS', 4, 1),
(8, 'C# Developer Intern', 'Internship opportunity for fresh graduates who know C# basics. Great learning environment with mentorship.', 'Wipro', 'Mumbai', 'BCA', 'CS', 5, 1),
(8, 'Software Engineer Trainee', 'Entry level position for engineering graduates. Training will be provided on .NET technologies.', 'Wipro', 'Chennai', 'B.Tech', 'CS', 1, 1),
(6, 'React Developer', 'Frontend developer position for React.js. Must know JavaScript, HTML, CSS and React hooks.', 'TechCorp Solutions', 'Delhi', 'B.Tech', 'IT', 2, 1),
(7, 'Backend Developer', 'Backend developer role working with microservices architecture. Strong C# and SQL skills needed.', 'Infosys', 'Bangalore', 'B.Tech', 'CS', 3, 0);


INSERT INTO Applications (CandidateId, JobId, Status, Score, TotalQuestions, AIFeedback, AppliedAt, TestTakenAt) VALUES
(1, 1, 'TestTaken', 4, 5, '✅ What you did well: You showed strong understanding of REST APIs and basic ASP.NET Core concepts.\n⚠️ Where to improve: Focus on Dependency Injection patterns - understanding AddScoped vs AddSingleton is crucial.\n📋 Action plan:\n1. Practice building small APIs with different DI lifetimes\n2. Read Microsoft docs on middleware pipeline', '2025-04-28 10:00:00', '2025-04-28 10:30:00'),
(1, 3, 'TestPending', NULL, NULL, NULL, '2025-04-29 14:00:00', NULL),
(2, 2, 'TestTaken', 3, 5, '✅ What you did well: Good grasp of Angular components and decorators.\n⚠️ Where to improve: Signals and Forms need more practice.\n📋 Action plan:\n1. Build a small project using Angular Signals\n2. Practice Reactive Forms with validation', '2025-04-28 11:00:00', '2025-04-28 11:25:00'),
(3, 4, 'Selected', 5, 5, '✅ What you did well: Excellent performance across all database topics!\n⚠️ Where to improve: Nothing major - keep practicing advanced indexing.\n📋 Action plan:\n1. Explore execution plans in SQL Server\n2. Study partitioning strategies', '2025-04-27 09:00:00', '2025-04-27 09:20:00'),
(4, 5, 'TestPending', NULL, NULL, NULL, '2025-04-30 16:00:00', NULL),
(1, 6, 'Applied', NULL, NULL, NULL, '2025-05-01 08:00:00', NULL);


INSERT INTO Answers (ApplicationId, QuestionId, SelectedOptionId, IsCorrect) VALUES
(1, 1, 1, 1),
(1, 2, 5, 1),
(1, 3, 9, 1),
(1, 4, 14, 0),
(1, 5, 17, 1),
(3, 6, 21, 1),
(3, 7, 25, 1),
(3, 8, 30, 0),
(3, 9, 33, 1),
(3, 10, 38, 0),
(4, 16, 61, 1),
(4, 17, 65, 1),
(4, 18, 69, 1),
(4, 19, 73, 1),
(4, 20, 77, 1);

SELECT 'Database created successfully!' AS Status;
SELECT CONCAT('Tables: ', COUNT(*)) AS Info FROM information_schema.tables WHERE table_schema = 'SmartInterviewDB';
SELECT CONCAT('Users: ', COUNT(*)) AS Info FROM Users;
SELECT CONCAT('Tests: ', COUNT(*)) AS Info FROM Tests;
SELECT CONCAT('Questions: ', COUNT(*)) AS Info FROM Questions;
SELECT CONCAT('Options: ', COUNT(*)) AS Info FROM `Options`;
SELECT CONCAT('Jobs: ', COUNT(*)) AS Info FROM Jobs;
SELECT CONCAT('Applications: ', COUNT(*)) AS Info FROM Applications;
