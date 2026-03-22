
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

use LibraryDb;
BACKUP DATABASE LibraryDB
TO DISK = 'C:\learning_coding\companies-assestments\Library-MVC\Database\LibraryDB.bak'
WITH INIT;