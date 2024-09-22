CREATE DATABASE MarkersDB;

USE MarkersDB;

CREATE TABLE Markers (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50),
    Latitude FLOAT,
    Longitude FLOAT
);

-- Пример наполнения таблицы данными
INSERT INTO Markers (Name, Latitude, Longitude)
VALUES ('Vehicle 1', 55.751244, 37.618423), -- Москва
       ('Vehicle 2', 59.931058, 30.360909), -- Санкт-Петербург
       ('Vehicle 3', 56.838926, 60.605702); -- Екатеринбург
