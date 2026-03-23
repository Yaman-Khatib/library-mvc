
-- Library system task

CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO


-- LOOKUP TABLES
CREATE TABLE Languages (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Genres (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);


-- USERS
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT CK_Users_Role CHECK (Role IN ('User', 'Admin'))
);

alter table users 
Add 
Email nvarchar(100) not null unique,
PasswordHash nvarchar(255) not null;

CREATE UNIQUE INDEX IDX_USERS_EMAIL
ON USERS(EMAIL);

use LibraryDB;


insert into books (tite
-- BOOKS
CREATE TABLE Books (
    Id INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(200) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL,
    TotalCopies INT NOT NULL,
    AvailableCopies INT NOT NULL,
    Description NVARCHAR(MAX) NULL,
    LanguageId INT NOT NULL,
    GenreId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Books_ISBN UNIQUE (ISBN),

    CONSTRAINT FK_Books_Language FOREIGN KEY (LanguageId)
        REFERENCES Languages(Id),

    CONSTRAINT FK_Books_Genre FOREIGN KEY (GenreId)
        REFERENCES Genres(Id),

    -- constraints
    CONSTRAINT CK_Books_TotalCopies CHECK (TotalCopies >= 0),
    CONSTRAINT CK_Books_AvailableCopies CHECK (AvailableCopies >= 0),
    CONSTRAINT CK_Books_Available_NotMoreThanTotal 
        CHECK (AvailableCopies <= TotalCopies)
);


-- BORROWINGS

CREATE TABLE Borrowings (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    BorrowDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DueDate DATETIME2 NOT NULL,
    ReturnDate DATETIME2 NULL,

    CONSTRAINT FK_Borrowings_User FOREIGN KEY (UserId)
        REFERENCES Users(Id),

    CONSTRAINT FK_Borrowings_Book FOREIGN KEY (BookId)
        REFERENCES Books(Id),

    -- Smart constraints
    CONSTRAINT CK_Borrowings_DueDate 
        CHECK (DueDate > BorrowDate),

    CONSTRAINT CK_Borrowings_ReturnDate 
        CHECK (ReturnDate IS NULL OR ReturnDate >= BorrowDate)
);


-- INDEXES (IMPORTANT)

-- Fast search by ISBN
CREATE INDEX IX_Books_ISBN ON Books(ISBN);

-- Search by title/author
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Author ON Books(Author);

-- Borrow lookup
CREATE INDEX IX_Borrowings_UserId ON Borrowings(UserId);
CREATE INDEX IX_Borrowings_BookId ON Borrowings(BookId);


-- ADVANCED CONSTRAINT
-- Prevent duplicate active borrow
-- (same user cannot borrow same book twice without returning)

CREATE UNIQUE INDEX UX_User_Book_ActiveBorrow
ON Borrowings(UserId, BookId)
WHERE ReturnDate IS NULL;

INSERT INTO Languages (Name) VALUES
('English'),
('Arabic'),
('German');

INSERT INTO Genres (Name) VALUES
('Programming'),
('Science'),
('History');

INSERT INTO Users 
(FirstName, LastName, DateOfBirth, Role, CreatedAt, Email, PasswordHash)
VALUES 
(
    'Zeid',
    'Hatem',
    '2000-01-01 00:00:00.0000000',
    'Admin',
    GETDATE(),
    'admin@example.com',
    'AQAAAAIAAYagAAAAEK7RbzdFDk6U8GRFptmHBQyzoPO+U0AONi5E2SqfsqQuNEsYBj7IW8Zdm7wsNU+g+Q=='
);

INSERT INTO Books 
(Title, Author, ISBN, TotalCopies, AvailableCopies, Description, LanguageId, GenreId, CreatedAt)
VALUES

('Clean Code', 'Robert C. Martin', '9780132350884', 10, 8, 'A handbook of agile software craftsmanship.', 1, 1, GETDATE()),

('The Pragmatic Programmer', 'Andrew Hunt', '9780201616224', 12, 10, 'Journey to mastery for modern software developers.', 1, 1, GETDATE()),

('Design Patterns', 'Erich Gamma', '9780201633610', 8, 5, 'Elements of reusable object-oriented software.', 1, 1, GETDATE()),

('Refactoring', 'Martin Fowler', '9780201485677', 7, 6, 'Improving the design of existing code.', 1, 1, GETDATE()),

('Introduction to Algorithms', 'Thomas H. Cormen', '9780262033848', 15, 12, 'Comprehensive guide to algorithms.', 1, 2, GETDATE()),

('You Don’t Know JS', 'Kyle Simpson', '9781491904244', 9, 7, 'Deep dive into JavaScript core mechanisms.', 1, 2, GETDATE()),

('Eloquent JavaScript', 'Marijn Haverbeke', '9781593279509', 11, 9, 'Modern introduction to JavaScript programming.', 1, 2, GETDATE()),

('Deep Work', 'Cal Newport', '9781455586691', 6, 4, 'Rules for focused success in a distracted world.', 2, 3, GETDATE()),

('Atomic Habits', 'James Clear', '9780735211292', 14, 11, 'An easy & proven way to build good habits.', 2, 3, GETDATE()),

('The Lean Startup', 'Eric Ries', '9780307887894', 10, 8, 'How today’s entrepreneurs use innovation.', 3, 3, GETDATE()),

('Zero to One', 'Peter Thiel', '9780804139298', 9, 6, 'Notes on startups and building the future.', 3, 3, GETDATE());

