# LearnitySchool (Clean Architecture Skeleton)

Це скелет рішення з Clean Architecture:
- LearnitySchool.Domain
- LearnitySchool.Application
- LearnitySchool.Infrastructure (EF Core + Identity)
- LearnitySchool.Web (MVC)

## Ролі
- Student
- Teacher
- Manager

## Layout
Вибір layout автоматично відбувається у `Views/_ViewStart.cshtml` залежно від ролі.

## Demo users (створюються при старті)
- student@learnity.local / Student123!
- teacher@learnity.local / Teacher123!
- manager@learnity.local / Manager123!

> Потрібно зробити `dotnet restore`, `dotnet ef migrations add InitialCreate` (опціонально) і `dotnet run` з Web проєкту.
