# HairdressingSalon

HairdressingSalon is an REST application written in Web Api 2. The application allows to manage a hairdressing salon and users in system.
It is possible to book visits, display free dates and services in the salon. The application generates statistics 
about the most popular services. It is also possible to grant discounts to customers.
User can register using standard registration.



## Getting Started

1. Clone the repository.
2. Restore NuGet package.
3. Add initial migration and update database to seed.
```
Add-Migration 'init'
```
then
```
Update-Database
```
4. Run project. 

## Technologies and libraries used
- Web Api 2
- Entity Framework 6
- MSSQL database
- NLog
- Unity Container
- AutoMapper 6
- Moq, Effort


## Authorization

#### Register
Authorization has been implemented as OAuth2.
Endpoint: api/Users/Register/
```
{
        "FirstName": "testName",
        "LastName": "testLastName",
        "BirthDate": "1950-01-05 00:00:00",
        "Phone": "test phone",
        "Email": "test@wp.pl",
        "Password": "password",
        "ConfirmPassword": "password"
}
```

To authenticate user he must send Autorization header

```
Authorization: Bearer 'token'
```

## Unit testing
Tests were written in standard .net unit testing library and Moq library. Effort was used to mock MSSQL database.


## Test login data
- Administrator- login: admin@wp.pl, password: 1qaz@WSX

## Author

* **Kamil Urbański**
