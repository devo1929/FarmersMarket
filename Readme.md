# Farmers Market

# Schema

This database schema was designed with a “cart” in mind where a user could pay once for multiple products from more than one Vendor at the Market.

This schema is assuming MSSQL/AzureSQL.

## Locations

These lay out the “grid” that is the market.

* Id - INT NOT NULL
* X - INT NOT NULL, X coordinate of given market
* Y - INT NOT NULL, Y coordinate of given market

## Users

Users that can log into the system

* Id - BIGINT NOT NULL
* Email - NVARCHAR NOT NULL
* FirstName - NVARCHAR NOT NULL
* LastName - NVARCHAR NOT NULL
* Hash - varbinary NOT NULL (assuming in-house auth)
* Salt - varbinary NOT NULL (assuming in-house auth)

## Products

Products that a given Vendor provides at the market

* Id - BIGINT NOT NULL
* Name - NVARCHAR NOT NULL
* Description - TEXT null
* VendorId - BIGINT, FK to Vendors
* Units - INT NOT NULL
* PricePerUnit - DECIMAL NOT NULL
* Status - TINYINT NOT NULL
    * Inactive
    * Active
* LocationId - BIGINT, FK to Locations

## Order

An order for a customer

* Id - BIGINT NOT NULL
* ReferenceId - GUID
    * An unique ID that a customer or vendor can reference
* Status - TINYINT NOT NULL
    * Pending - order has been placed, but not yet started processing.
    * Processing - order has been reviewed and is now processing.
    * Complete - The overall Order has been completed.
    * Cancelled - The Order has been cancelled.
    * Partial - Parts of the order were able to be completed.

## Vendors

Vendors that are registered with the farmers’ market

* Id - BIGINT NOT NULL
* Name - NVARCHAR NOT NULL
* Description - TEXT null, a way for the Vendor to describe themselves on a website
* Status - TINYINT NOT NULL
    * Pending - New registration with the farmers’ market
    * Active - Registration approved and vendor account activated. Vendor is now listed at the farmers’ market.
    * Revoked - Registration was revoked. Vendor is no longer listed at the farmers’ market

## VendorLocations

This connects Vendors to Locations to create the “bounds” for a Vendor in the market. (see diagram)

* Id - BIGINT NOT NULL
* VendorId - BIGINT NOT NULL, FK to Vendors
* LocationId - BIGINT NOT NULL, FK to Locations

## VendorUsers

Users that can log in on behalf of a Vendor account. Users can then manage inventory and review orders.

* Id - BIGINT NOT NULL
* VendorId - BIGINT NOT NULL, FK to Vendors
* UserId - BIGINT NOT NULL, FK to Users

## UserRoles

Roles/permissions a particular system User might have for the entire farmers’ market

* Id - SMALLINT NOT NULL
    * Admin - full permissions over system
    * Registration - permission
* Name - NVARCHAR NOT NULL - Simply a display name to quickly identify the role

## VendorUserRoles

Roles/permissions for a particular VendorUser to manager their Vendor

* Id - SMALLINT NOT NULL
    * Admin - full permissions over Vendor account
    * Inventory - permission to manage inventory
    * Orders - permission to manage Orders

## UserUserRoles

Connecting a User to a UserRole

* Id - BIGINT NOT NULL
* UserId - BIGINT NOT NULL
* UserRoleId - SMALLINT NOT NULL

## VendorUserUserRoles

Connecting a VendorUser to a VendorUserRole

* Id - BIGINT NOT NULL
* VendorUserId - BIGINT NOT NULL
* VendorUserRoleId - SMALLINT NOT NULL

## VendorOrder

This is a partial order that is specific to a vendor. It could be a subset of products in a user’s overall “cart”

* Id - BIGINT NOT NULL
* OrderId - BIGINT NOT NULL, FK to Orders
* VendorId - BIGINT NOT NULL, FK to Vendors
* Status - TINYINT NOT NULL
    * Pending - the Vendor has received their part of the overall Order.
    * Processing - The Vendor has started processing their part of the overall Order.
    * Complete - The Vendor has completed their part of the overall Order.
    * Cancelled - The Vendor has cancelled their part of the overall Order.
    * Partial - The Vendor was able to complete part of their part of the overall Order.
* Notes - NVARCHAR null, free type property to provide “notes” to the customer on whey their part of the overall Order is of a particular status.

## VendorOrderProducts

These are the individual products from a specific Vendor in a VendorOrder

* Id - BIGINT NOT NULL
* VendorOrderId - BIGINT NOT NULL
* ProductId - BIGINT NOT NULL
* Units - INT NOT NULL
* Status - TINYINT NOT NULL
    * 0 - Pending
    * 1 - Processing
    * 2 - Complete
    * 3 - Cancelled
    * 4 - Partial
* Notes - NVARCHAR null, free type property to provide “notes” to the customer on why their Product is of a particular Status in a given Order.

---

# Authentication

The system would use an authentication system that uses JWTs. All routes in the API would attempt to validate an Authorization request header containing an access token, by
default. Only endpoints whitelisted for anonymous access would “skip” authentication (identified below).

Token Claims

* sub - User.Id
* vnd - Vendor.Id
* iat - issued at
* exp - expiration
* roles - array of UserRoles (system)
    * This is meant to supplement the front end in knowing what pages of a “portal” the user can see in market system management
    * These are NOT validated by the back end on any requests. That validation is done by recalculating the UserRoles on the token’s sub claim (User.Id) at request time on
      necessary requests.
* vndroles - array of VendorUserRoles (vendor)
    * This is meant to supplement the front end in knowing what pages of a “portal” the user can see for Vendor management.
    * These are NOT validated by the back end on any requests. That validation is done by recalculating the VendorUserRoles on the token’s sub claim (User.Id) at request time on
      necessary requests.
* profile - object of any necessary “display” properties of a given user for a better front end experience (first name, last name, email, etc…)

The “vnd” claim leaves the door open for a given User to have access to multiple Vendors with the same User account. If this is the case at the time of login, the user would need
to select a Vendor.. After login, the site/application would provide the ability to switch to another Vendor accessible to the current User.  
This “vnd” claim also creates “context” for various end points in the API.

---

# API

Below is a REST design that should meet the requirements for this assignment. However, I believe GraphQL would also be a good use case for this API. Below the specified routes are
model definitions that are sometimes more specific for a given end point to ensure that only relevant fields are being returned to the front end. GraphQL would allow the back end
to maintain fewer model definitions and end points and allow the front end to be smarter in requesting only the data it needs at any given point.

* Authentication -
    * `POST /authenticate` (LoginModel)
* Vendors
    * `GET /vendors` (anonymous access)
        * Retrieves all ACTIVE Vendors in the system (VendorModel)
        * Used by customers to view all Vendors at the market.
* Products
    * `GET /products` - (anonymous access)
        * Retrieves all ACTIVE products in the system (ProductModel)
        * Used by customers to view all Products at the market.
        * Filters
            * `vendorId` - taken from /vendors end point, customer can filter products to a given vendor.
            * `showOutOfStock` (true/false, default true) - customers can filter out Products that are not currently in stock
    * `POST /products` (vendor user access, with Inventory role)
        * Add a product to the system for a given Vendor (vnd claim) (ProductCreateModel)
    * `DELETE /products/{productId}` (vendor user access, with Inventory role)
        * Delete a given product by ID
    * `PUT /products` (vendor user access, with Inventory role)
        * Updates a given product
    * `GET /products/{productId}/route`
        * Returns an optimized RouteModel to the Product from a specified Location (defaults to market entry point, if not specified)
        * QueryString
            * `location` (x,y) - optional, a location to calculate the route from
* Orders
    * `POST /orders` (anonymous access, OrderCreateModel -\> OrderCreateResponseModel)
        * Where customers place their orders
    * `GET /orders` (vendor user access, with Orders role)
        * Where Vendors can review their orders
        * Filters
            * `status` - view only Pending or Completed orders
    * `GET /orders/{orderId}/route`
        * Returns an optimized RouteModel to each Product in a given Order from a specified Location (defaults to market entry point, if not specified)
        * QueryString
            * `location` (x,y) - optional, a location to calculate the route from

## API Models

LoginModel

* Email
* Password

VendorModel

* Id
* Name
* Description

ProductModel

* Id
* Name
* Description
* Vendor - VendorModel
* Units
* PricePerUnit
* Location - LocationModel - the X,Y location in the market

ProductCreateModel

* Name
* Description
* Units
* PricePerUnit
* Status

ProductUpdateModel

* Id
* Name
* Description
* Units
* PricePerUnit
* Status

OrderModel

* Id
* ReferenceId - a user readable and referenceable ID
* Status
* Products - ProductModel

OrderCreateModel

* Products - OrderCreateProductModel[]

OrderCreateProductModel

* Id
* Units

OrderCreateResponseModel

* Id
* ReferenceId - GUID

OrderUpdateModel

* Id
* Status
* Products - OrderUpdateProductModel[]

OrderUpdateProductModel

* Id
* Status
* Units

RouteModel

* Paths - PathModel[]
* Distance- INT (meters, ft, inches, etc…)
    * This field could be used to dynamically determine the amount of time to navigate a given route, based on the speed of the traveler.

PathModel

* Start - LocationModel
* End - LocationModel

LocationModel

* X - INT
* Y - INT

# Server Architecture

Using what I am most familiar with. Azure services allow a user to scale up and out based on demand.  
[https://azure.microsoft.com/en-us/pricing/calculator/](https://azure.microsoft.com/en-us/pricing/calculator/)

* Database
    * AzureSQL
    * Standard DTU tier/model
        * Lower environments for development/testing.
        * 250 GB storage
        * Estimated cost: $15/month
    * Performance General Purpose model
        * Production environment. However, Standard tiers may also meet demands.
        * 250 GB storage
        * Estimated cost: $405/month
* App Service
    * Provides the API
    * Linux
        * Basic
            * Cheapest option
            * Estimated cost: $13/month
        * Standard
            * Provides the ability for slots. Slots allow deployments to occur nearly seamlessly in a given environment by deploying to a staging slot then swapping it into a
              “live” slot when warmed up/ready.
            * Estimated cost: $70/month
        * Premium
            * Also provides slots, allowing for production deploys to occur with little to no downtime for customers
            * Significantly better performance over lower tiers
            * Estimated cost: $74/month
* Application Insights - Logging

Total estimated cost range: ~ \$30-\$480 / month

# Frameworks / Libraries

These are the frameworks/libraries that were used in the assignment.

* Hosting: [ASPNetCore](https://dotnet.microsoft.com/en-us/apps/aspnet)
* ORM: [EntityFrameworkCore 9](https://learn.microsoft.com/en-us/ef/core/)
* Route/Path Calculation: [Dijkstra.NET](https://www.nuget.org/packages/Dijkstra.NET)
* If using GraphQL: [HotChocolate v15](https://chillicream.com/docs/hotchocolate/v15)

## Market Diagram

This outlines an example of how the market could be laid out. Each letter in the grid is a Vendor. Each RED marking is a bin to access Products. Bins cannot be placed at the corner
of a given Vendor. This means that some bins are only available on one side of a given “path.” Paths are marked as white lines.

Examples:

VendorA Bounds:  
(0,0) (0,4) (4,0) (4,4)

VendorA Bins:  
(0,1) (0,2) (0,3)  
(1,4) (2,4) (3,4)  
(4,1) (4,2) (4,3)  
(1,0) (2,0) (3,0)  

VendorB Bounds:  
(4,0) (8,0) (4,2) (8,2)

VendorB Bins:  
(4,1)  
(5,2) (6,2) (7,2)  
(8,1)  
(5,0) (6,0) (7,0)

For example image of the market grid, see [market_grid.jpg](https://github.com/devo1929/FarmersMarket/blob/master/market_grid.jpg)