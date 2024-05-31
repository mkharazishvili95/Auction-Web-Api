# Auction-Web-API

Auction-Web-Api is an ASP.NET CORE WEB API PROJECT,(on .NET8)which allows users to place their own product on the auction, as well as participate in the auction to buy a specific product at the desired price.


## Getting Started:

To use the API and access its services, you need to register as a user. Use the following registration form:

### Registration Form:

{
  "firstName": "string",
  "lastName": "string",
  "age": 0,
  "identityNumber": "string",
  "contactNumber": "string",
  "email": "string",
  "password": "string",
  "userAddress": {
    "country": "string",
    "city": "string"
  }
}
I added validations to the registration form that none of the fields should be empty, the user should not use the email for registration that already exists in the database, 
and it is also not possible to register with a personal number and mobile number that is already registered in the database. After registration, the User's password is stored in a hashed database.
Once registered, you can log in using the following user login form:

### User Login Form:
{
  "email": "string",
  "password": "string"
}

### Users can get their balance and details about their profile:
/api/User/GetMyBalance
/api/User/GetMyProfile
/api/User/UpdateEmailAddress (User can change his Email):
{
  "eMail": "string",
  "repeatEmail": "string",
  "newEmail": "string",
  "password": "string",
  "repeatPassword": "string"
}
/api/User/ChangePassword  (User can change his password):
{
  "eMail": "string",
  "repeatEmail": "string",
  "password": "string",
  "repeatPassword": "string",
  "newPassword": "string"
}

## Product Management:
### Adding a Product:
Authorized users can add a product using the following request body:
{
  "name": "string",
  "description": "string",
  "startingPrice": 0,
  "startTime": "2024-05-31T07:14:06.640Z",
  "endTime": "2024-05-31T07:14:06.640Z",
  "categoryId": 
}
All the fields must be filled by the user to add the product, also the existing category Id must be entered.

User can get information about all products: 
/api/Product/GetAllProducts
Also information about available products that are not sold:
/api/Product/GetAvailableProducts
Also information about a product that is in a specific category (by Category Id):
/api/Product/GetProductsByCategoryId
Also get information about the product according to its Id:
/api/Product/GetProductById
Also get information about your winning products:
/api/Product/GetWinningProducts
Also get information about your won product through Id:
/api/Product/GetWinningProductById
Also to purchase the winning product after the end of the auction through the product Id:
/api/Product/BuyWinningProduct

## Bidding Management:
During the auction, the user has the right to make a bid:
/api/Bid/MakeABid:
{
  "biddingCurrentAmount": 0,
  "productId": 0
}
Also has the right to increase bid by 10GEL via productId:
/api/Bid/RiseABidBy10

 ## Admin Management:
 The administrator has the right to receive information about all Users:
/api/Admin/GetAllUsers
Also information about a specific User through his Id:
/api/Admin/GetUserById
Also information about users living in a specific country:
/api/Admin/GetUsersByCountry
Also information about users living in a specific city:
/api/Admin/GetUsersByCity
Also information according to their Balance:
/api/Admin/GetUsersByBalance:
{
 "minBalance": 0,
 "maxBalance": 0
}
Also information according to their age:
GET
/api/Admin/GetUsersByAge:
{
 "minAge": 0,
 "maxAge": 0
}
Also the admin has the right to block the user through userId:
/api/Admin/BlockUser (A blocked user is prohibited from making a bid, as well as adding a product, as well as buying a winning product.)
Also the admin has the right to unlock the blocked User through userId:
/api/Admin/UnBlockUser
Also get information about blocked users:
/api/Admin/GetBlockedUsers
Also has the right to delete a User through his userId:
/api/Admin/DeleteUser
Also has the right to add a new category by its name:
/api/Admin/AddNewCategory
Also get information on all product categories:
/api/Admin/GetAllCategories
Also get product category information according to its Id:
/api/Admin/GetCategoryById
Also update the category
/api/Admin/UpdateCategory:
{
 "categoryId": 0,
 "newName": "string"
}
Also delete the category by clicking on the Category Id:
/api/Admin/DeleteCategory
Also to receive information about the completed auction:
/api/Admin/GetFinishedBids
Also update the finished bid according to the product Id, after which the buyer Id of this product changes and the customer who made the maximum bid can buy this product:
/api/Admin/RefreshFinishedBidById
Also refresh the completed bids, after which the buyer Ids of these products are changed and users who made the highest bid can purchase the winning products:
/api/Admin/RefreshFinishedBids

## What have I made:
I created Role-Based authorizations, added registration and logging forms, token generation. I used nuget packages: Microsoft.AspNetCore.Authentication.JwtBearer, 
Microsoft.AspNetCore.Identity.EntityFrameworkCore, I defined the conditional time of the access token as 365 days. 
I added validations to the registration so that none of the user's data is empty in the database, and I also do not enter the user in the database with the same email, 
personal number and mobile number that already exists in the database. I used FluentValidation for validations. After registration, the hashed passwords go to the database. 
I used: TweetinviAPI. After logging in, the User's logs are also stored in the database.
I made services where the business logic of the project is written, and which I then implement in the controllers.
I check that the status codes are returned in the controllers that I expect, and I also check everything in the database after the process is executed.
I used Dapper ORM.
I created a NUnit tests project where I made FakeServices where I replaced the real objects in the project with Fake objects and tested it to make everything green, all 50 tests ran successfully.
I used Postman to test the status codes and controllers. Everything is working properly.
In the case of my API, localhost is:
https://localhost:7100/
