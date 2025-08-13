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
    * Pending  - New registration with the farmers’ market
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

The system would use an authentication system that uses JWTs. All routes in the API would attempt to validate an Authorization request header containing an access token, by default. Only endpoints whitelisted for anonymous access would “skip” authentication (identified below).

Token Claims

* sub - User.Id
* vnd - Vendor.Id
* iat - issued at
* exp - expiration
* roles - array of UserRoles (system)
    * This is meant to supplement the front end in knowing what pages of a “portal” the user can see in market system management
    * These are NOT validated by the back end on any requests. That validation is done by recalculating the UserRoles on the token’s sub claim (User.Id) at request time on necessary requests.
* vndroles - array of VendorUserRoles (vendor)
    * This is meant to supplement the front end in knowing what pages of a “portal” the user can see for Vendor management.
    * These are NOT validated by the back end on any requests. That validation is done by recalculating the VendorUserRoles on the token’s sub claim (User.Id) at request time on necessary requests.
* profile - object of any necessary “display” properties of a given user for a better front end experience (first name, last name, email, etc…)

The “vnd” claim leaves the door open for a given User to have access to multiple Vendors with the same User account. If this is the case at the time of login, the user would need to select a Vendor.. After login, the site/application would provide the ability to switch to another Vendor accessible to the current User.  
This “vnd” claim also creates “context” for various end points in the API.

---

# API

Below is a REST design that should meet the requirements for this assignment. However, I believe GraphQL would also be a good use case for this API. Below the specified routes are model definitions that are sometimes more specific for a given end point to ensure that only relevant fields are being returned to the front end. GraphQL would allow the back end to maintain fewer model definitions and end points and allow the front end to be smarter in requesting only the data it needs at any given point.

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

* Products  - OrderCreateProductModel[]

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
            * Provides the ability for slots. Slots allow deployments to occur nearly seamlessly in a given environment by deploying to a staging slot then swapping it into a “live” slot when warmed up/ready.
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

## 

## Market Diagram

This outlines an example of how the market could be laid out. Each letter in the grid is a Vendor. Each RED marking is a bin to access Products. Bins cannot be placed at the corner of a given Vendor. This means that some bins are only available on one side of a given “path.” Paths are marked as white lines.![][image1]  
Examples:  
VendorA Bounds:  
(0,0) (0,4) (4,0) (4,4)  
VendorA Bins:  
(1,0) (2,0) (3,0)  
(0,1) (4,1)  
(0,2) (4,2)  
(0,3) (4,3)  
(1,4) (2,4) (3,4)

VendorB Bounds:  
(4,0) (8,0) (4,2) (8,2)  
VendorB Bins:  
(5,0) (6,0) (7,0)  
(4,1) (8,1)  
(5,2) (6,2) (7,2)


[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAtAAAAGxCAYAAABLF4H+AABQzElEQVR4Xu3dCa8c1bX28f2F+A6RJYQsZIQsEMKyAIEFYhAkshIGQQgIZBwIZrBIIBDGEC5DuFxCIME4DGI0M5jBGAwxEGOwARtswDb1vruS6tR5qqtO9+nda+12/Zf0071ZVbup/Xi5XW736Q4/+clPCgAAAACjCdoAAAAA0C5oAwAAAEC7oA0AAAAA7YI2AAAAALQL2gAAAADQLmgDAAAAQLugDQAAAADtgjYAAAAAtAvaAAAAANAuaAMAAABAu6ANAAAAIDextFf1247Nd7zrWJegDQAAAGAWVDe/890EDzs+6tphgjYAAACAWTDqTfCw46OuHSZoAwAAAMhd/cZ3vptgPT7O2mGCNgAAAICctZWeVz9/oWuHCdoAAAAAZsl8N8Bdx7uOtQnaAAAAAGaJ3gTP979HPdYmaAMAAADITVVt/epY/f8fdrxr7aiCNgAAAIBZNu4N8biCNgAAAIBZxg00AAAAMKJp3zxHQRsAAAAA2gVtAAAAAGgXtJHaTNUhh4yOSl+acZtcS6+zTa6l19mGSl+acZccS6+xS46l19glx9JrbJNr6XW2odKXZtxlxkrvR1ML2khtpkqHpQuVvjTjNrmWXmebXEuvsw2VvjTjLjmWXmOXHEuvsUuOpdfYJtfS62xDpS/NuMuMld6Ppha0kZrVRlJoDEsHXZurgzF/XZcLvc42ui4Xep1tdF2uDsbZzzV/vcYuujYHeo1ddG0O9Brb6Lpc6HW20XW54rnHl1X+QRupWW0kBR2WLro2Vwdj/rouF3qdbXRdLvQ62+i6XB2Ms59r/nqNXXRtDvQau+jaHOg1ttF1udDrbKPrcsVzjy+r/IM2UrPaSAo6LF10ba4Oxvx1XS70OtvoulzodbbRdbk6GGc/1/z1Grvo2hzoNXbRtTnQa2yj63Kh19lG1+WK5x5fVvkHbaRmtZEUdFi66NpcHYz567pc6HW20XW50Otso+tydTDOfq756zV20bU50GvsomtzoNfYRtflQq+zja7LFc89vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNoLhyB99xewD8MBzjy+r/IM2UrPaCIYjf/QVsw/AA889vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNoLhyB99xewD8MBzjy+r/IM2UrPaCIYjf/QVsw/AA889vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNoLhyB99xewD8MBzjy+r/IM2UrPaCIYjf/QVsw/AA889vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNoLhyB99xewD8MBzjy+r/IM2UrPaCIYjf/QVsw/AA889vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNoLhyB99xewD8MBzjy+r/IM2UrPaCIYjf/QVsw/AA889vqzyD9pIzWojGI780VfMPgAPPPf4sso/aCM1q41gOPJHXzH7ADzw3OPLKv+gjdSsNpLCY489NrjeUWrNmjWNx8hNVdoHDnbMPrqceuqpgxkZp/RxAOU9K4sXL65N7PB64IEHGuvarFy5Upe31p49exrrrVWl/dSCNlKz2kgKS5YsKS677LJysDZu3Fh8//33g+uvaseOHcXTTz9d3HHHHcWiRYsaj5GbqrQPHOyYfcznnHPOKe66667ihRdeKL788svBzFS1f//+YvPmzcX69euLG2+8sTjrrLMajwGoqrRv6cwzzyxuuummcnbfe++9xv1MnO0rr7yysa7NueeeW/5eeemll4pvvvlmzmNt3769vC+6+eabi9NOO62x1lpV2k8taCM1q41Mw+rVqwfXHyv+zWrp0qWN83JWlfaBgx2zj3EceuihxbZt2wZzE+u2225rnAfMpyrtezrmmGOKr776qjbdRbF79+7GeaP42c9+NniM+Hsm/t7RczxVpf3UgjZSs9rINBx77LGD64+1devWxjm5q0r7wMGO2ce44r881uukk05qnAPMpyrte4uvRGvFf3nX80bx3XffletffvnlxjFvVWk/taCN1Kw2Mg1HHnnk4Ppjvf/++41zcleV9oGDHbOPcb3xxhuDuYm10JsL9FtV2vf21ltv1ab73xXfkqHnjSK+eh3r+eefbxzzVpX2UwvaSM1qI9Ogb8TftGlT45zcVaV9jOawww4rLr744uKee+4pXnnllfJtPFq7du0q/xZ+7733FhdeeOGcf86K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yB99ZTX7QRupWW0EyE0Os3/22WcPrmOUevLJJxuPcdttt+lp89bNN9/ceBxrVWk/F+eff37x2muvFfv27aslN3p9/fXXxbvvvlvceeedjcfOQVXahw3yR19ZzX7QRmpWGwFyk8vsr1y5srj11luLp556qvj0008H11XVd999V7zxxhvFr3/96+KII45orD/ssMOKSy+9tHjooYeKt99+u9izZ48+RNmLN4P33Xdf8atf/ao49NBDG49jrSrte4sZP/vss8X+/fsH1xhfdX766aeL3/72t8WZZ55ZLF26tFi8eHFx0kknFZdffnnx8MMPl69OD6tNmzY1/hs5qEr7sEH+6Cur2Q/aSM1qI0Bucp397du3D64t1nXXXdc4p0u8qYtvL6jqxx9/LE455ZTGed6q0r63zz77bHBt8a0aDzzwQHmzrOcNs2rVqsaNNDfQGIb80VdWsx+0kZrVRoDc5Dr7H3zwweDaYv3iF79onDOf+N7cquKrz3o8B1Vp39NZZ501uK6dO3cW55xzTuOc+cR/EYj/ElDV5s2bG+fkoCrtwwb5o6+sZj9oIzWrjQC5yXX233///cG1xYpv8dBz5lN/G0d8+4Eez0FV2vdyzDHHFJ9//nl5Tbt37y5++tOfNs4Z1VFHHVV8++235WNt2bKlcTwHueXfN+SPvrKa/aCN1Kw2AuRmUIcc0qDnWoqvWNZrITdy1c1brPjDbHo8B1Vp38urr75aXk98y8vatWsbx8cV39MeK76lQ4/lILf8+4b80VdWsx+0kZrVRsahNzNddO2s0H200XW50+vvomutDSqza3vvvff+e23/v84444zGOfOJr6BWFT9mTY/noCrte1izZs3geuLH0enxhYjvh44VP+JOj1nT+e6iazE5zbj0n9Jzc9C41ha6Lnd6/V10bS70Orvo2lxYzX7QRmpWGxmHDkEXXTsrdB9tdF3u9Pq76Fprg8rs2uIPndXrtNNOa5wzn/i2jaq4gZ7fRx99NLieX/7yl43jC7Fo0aLim2++Kd9Lrces6Xx30bWYnGZcymj+VeNaW+i63On1d9G1udDr7KJrc2E1+0EbqVltZBw6BF107azQfbTRdbnT6++ia60NKrNri58dXK+TTz65cc586jfQX375ZeN4DqrSvrULL7xwcC2pr+ett94q306jfWs63110LSanGZemMG+pNK61ha7LnV5/F12bC73OLro2F1azH7SRmtVGxqFD0EXXzgrdRxtdlzu9/i661tqgMru2+BaCesWPpdNz5sMN9Ogef/zxwbXkcD3ToPPdRddicppxKeN5a1xrC12XO73+Lro2F3qdXXRtLqxmP2gjNauNjEOHoIuunRW6jza6Lnd6/V10rbVBZXZt9Y9Ai3X88cc3zplP/MHBqnK4gdZ8u+jaafvkk09qaef1XJiKZtxF12JymnEp43lrXGsLXZc7vf4uujYXep1ddG0urGY/aCO1LGvIILSa1dJ9tJm10uvvkkvpdR3i+8QT/9m/XsuXL2+cMx9uoEcTP7NZv6pbzzkYaMZddC0mpxmXci691jazVnr9XXItvc4umZf+PkktaCM1q42MozEEHXTtrNB9tNF1udPr76JrrQ0qs2t78803/3ttCYob6HarV6+ek1X82nQ952CgGXfRtZicZlz6T+m5OWhcawtdlzu9/i66Nhd6nV10bS6sZj9oIzWrjYxDh6CLrp0Vuo82ui53ev1ddK21QWV2bdxAN9dPyzXXXDMnq/gNjnrOKG677bY5j9NVd911V2P9tGnGXXQtJqcZl/5Tem4OGtfaQtflTq+/i67NhV5nF12bC6vZD9pIzWoj49Ah6KJrZ4Xuo42uy51efxdda21QmV2b3kDHb8jTc+YTP7quKm6g211xxRW1pIviwIEDjXNGEd8KEj/3+cEHHyw/xzs+Tr3il7P87W9/K1/xXrx4cWP9tGnGXXQtJqcZl/5Tem4OGtfaQtflTq+/i67NhV5nF12bC6vZD9pIzWoj49Ah6KJrZ4Xuo42uy51efxdda21QmV2b3kAvW7ascc586u+BzuFzoDXfLrp2ms4999xa0v8uPWchnnnmmTmPGb+VUM+xpBl30bWYnGZc+k/puTloXGsLXZc7vf4uujYXep1ddG0urGY/aCM1q40AuRlUZk88Gzdu/O+1/f867rjjGufMp/4xdjl8lbfm20XXTtOJJ55YS/rfpecsRHybRr3iWzz0HG9VaR/p6YyXyB89ZTX7QRupWW0EyE2us6830Av5GLv6V3nHm2k9bq1x89BB105b/LbAeunxhbj99tvnPOb111/fOMdbVdqHDfJHX1nNftBGalYbAXKT6+zrx9jFV0n1nPnUbwrjzbQet6Y3yV107bRp3np8IfSHCteuXds4x1tV2ocN8kdfWc1+0EZqVhsBcpPr7Os3Ea5YsaJxznzi10dXFW+m9bg1vUnuomun7f7776+lXRQ///nPG+eMS1+Bjp/2oed4q0r7sEH+6Cur2Q/aSM1qI0Bucp19vYE++eSTG+fMZ8+ePYP18WZaj+egKu1bO/bYY8uPr6vq5ZdfbpwzLm6gMR/yR19ZzX7QRmpWGwFyk+vs6w30qaee2jhnPt9///1gfbyZ1uM5qEr7Hp544onB9cSPoPvd737XOGccf/zjHwePF4u3cECRP/rKavaDNlKz2giQm1xnf9OmTYNri3X66ac3zplP/QZ6oV8OMm1Vad9D/Lr0+HnZ9czOP//8xnmj0k/h4IcIocgffWU1+0EbqVltBMhNrrP//vvvD64t1llnndU4Zz779u0brI8303o8B1Vp38tFF11U/PDDD4PrijfRN9xwQ+O8Udxzzz2Dx4n1hz/8oXGOt6q0Dxvkj76ymv2gjdSsNgLkJsfZX7RoUbFjx47BtcVas2ZN47wuF1xwwZz18VvwFvJJHtNWlfY9xbdu1Cu+nePVV18tVq5c2Ti3Tfzimw0bNsx5nPiWDj3PW1Xahw3yR19ZzX7QRmpWGwFyk8vsn3322eU/+b/wwgvF559/PriuquIryPGzoePXQC9ZsqSxPn6N9CWXXFI88MAD5bcY1n+AsKr4SRwvvfRSceeddxbnnXdeceihhzYex1pV2vf2xRdf1JL7d+3fv7/8iu74Vd2XXnpp+baa+GuxdOnS4owzziiuvfba4uGHHy42b94859X/qu69997Gf8dbVdqHDfJHX1nNftBGalYbAXKTw+zHm+dx6sknn2w8hn7m8Ch18803Nx7HWlXa97Z48eLi7rvvbvxLwDgV30/9xhtvlH+piX+5Ofzwwxv/HW9VaR82yB99ZTX7QRupWW0EyA2z7yv3/OOr9KtWrSoeeeSR8tXn7du3l9/qGF9hjuLHA+7cubPYtm1b+YOfjz/+eHHjjTcu6HO7PeSe/8GO/NFXVrMftJGa1UaA3DD7vsjfF/n7In/0ldXsB22kZrURIDfMvi/y90X+vsgffWU1+0EbqVltBMgNs++L/H2Rvy/yR19ZzX7QRmpWGwFyw+z7In9f5O+L/NFXVrMftJGa1UaA3DD7vsjfF/n7In/0ldXsB22kZrURIDfMvi/y90X+vsgffWU1+0EbqVltBMgNs++L/H2Rvy/yR19ZzX7QRmpWGwFyw+z7In9f5O+L/NFXVrMftJGa1UaA3DD7vsjfF/n7In/0ldXsB22kZrURIDfMvi/y90X+vsgffWU1+0EbqVltBMgNs++L/H3lnv+FF144uMaUtW7dusZ/y0NV2sd4Lr/88tqvbprasWNH47+DdKrSfmpBG6lZbQTIDbPvi/x9zUL+Z599dnHnnXcWL774YrF79+7BNU9STz/9dOO/46Eq7WM8ixYtKi644ILiz3/+c/HGG28U33zzTe1Xe2G1a9euxn8H6VSl/dSCNlKz2giQG2bfF/n7mrX8jzzyyOLrr78eXHes1atXN86rW7x4cbFixYryJnzfvn3lmpdffrlxnoeqtI/JHHPMMcWePXsG+cY69dRTG+dVDj300OLYY48tVq1aVfzrX/8qz4/r9TykU5X2UwvaSM1qI0BumH1f5O9rFvPftGnT4LpjXXTRRY1z2jz33HPlmrfeeqtxzENV2sfktm3bNsg31oknntg4Z5iLL764PD/+ZUuPIZ2qtJ9a0EZqVhsBcsPs+yJ/X7OY/9tvvz247ljnn39+45w28RXGWB988EHjmIeqtI/Jffzxx4N8Y8VXmPWcNp999lm5Zvny5Y1jSKMq7acWtJGa1UaA3DD7vsjf1yzmv3HjxsF1x/rFL37ROKdNfK9sfB/1p59+2jjmoSrtY3Jbt26tTUlRHHHEEY1z2qxfv75cc9555zWOIY2qtJ9a0EZqVhsBcsPs+yJ/X7OY/5tvvjm47lg//elPG+d0ee2117L5hIWqtI/Jffjhh7UpKcq/POk5bap/qbj66qsbx5BGVdpPLWgjNauNALlh9n2Rv69ZzD9+ykK9Tj/99MY5s6Iq7WNyW7ZsGeS7f//+xnH4spr9oI3UrDYC5IbZ90X+vmYxf72BPvnkkxvnVOIPjv3444/F7bff3jiWg6q0j8m9//77g3x/+OGHxnH17LPPFp9//nmjj+mwmv2gjdSKQw4ZSs8DDjZWv4kxHPn7msX89Qb6hBNOaJxTuemmm8pzfv/73zeO5aAq7WNy9Rvo77//vnFcxffFxx881D6mw2r2gzZS0xtnbqDRF1a/iTEc+fuaxfz1BrrrkxI2bNhQnnPttdc2juWgKu1jcvUb6L179zaO18W/hB04cCCbT2fpA6vZD9pITW+cuYFGX1j9JsZw5O9rFvN//fXXB9cda9jHky1durS47rrrylceY11xxRWNc3JQlfYxuc2bNw/y/fbbbxvH4w8VLlmypPyClVdffbU87913322ch+moSvupBW2kpjfO3ECjL6x+E2M48vc1i/nrDfQodckllzQeJwdVaR+Tq99Aj1q5fMFOH1Sl/dSCNlLTG2duoNEXVr+JMRz5+5rF/LmBxii4gc5bVdpPLWgjNb1x5gYafWH1mxjDkb+vWcxfb6CPOuqoxjnxbR1XXnll+aUpsX7zm980zslBVdrH5Oo30Hv27Gkcr97CsWLFimLdunXlebyFw05V2k8taCM1vXHmBhp9YfWbGMORv69ZzF9voI888sjGOZV//OMf5Tlr165tHMtBVdrH5Oo30PP9EOFhhx1WfPfdd/wQoSGr2Q/aSE1vnLmBxsFKZ7yNrsN0WD2JYvTZz33+x7mBXrNmTXnOjTfe2DhmTTNuo+uwMPUb6HhzrMdV/OZCPsZuOnTGLec9aCM13ZDVxgBrOuNtdB2moyrtIz2d8S66Nif6MXZdN9DxrRy5fJGKZtxG12Fh6jfQo3wO9FNPPcUXqUyJzrjlvAdtpKYbstoYYE1nvI2uw3RUpX2kpzPeRdfm5M033xzMTayuG+icaMZtdB0WZtxvIsT06IxbznvQRmpVaR842Ohv3ja6DpPTjLvoWkxOM+6ia3OyadOmwZ9ZsbiBxjBbt24dzMj+/fsbx2FHZ9xy3oM2UqtK+8DBRn/zttF1mJxm3EXXYnKacRddmxP9eLJhn8KRI824ja7DwnzyySeDGYnfMqjHR/HnP/+5vPmOn9ShxzA6nXHLeQ/aSK0q7QMHG/3N20bXYXKacRddi8lpxl10bU62bNky+DMr1rBvIsyRZtxG12Fhtm3bNmdO9Ph84lfEx28wjG8F0WMYj8645bwHbaS20AEDZo3+5m2j6zA5zbiLrsXkNOMuujYn//znP2u3RUVx3HHHNc7JkWbcRtdhYbZv3z5nTuLnPus5XTZs2FCu+5//+Z/GMYxHZ9xy3oM2UqtK+8DBRn/zttF1mJxm3EXXYnKacRddmxN9ZXFW/nldM26j67AwO3funDMnixcvbpzT5pprrinf9rFv375i2bJljeMYj8645bwHbaRWlfaBg43+5m2j6zA5zbiLrsXkNOMuujYXRx99dPmRZPVatWpV47wcacZtdB3GF9/WEz95o17xWwf1vEp8dfrwww8vTjzxxOKee+4ZzBjfTJiGzrjlvAdtpFaV9oGDjf7mbaPrMDnNuIuuxeQ04y661tPKlSuLm266qXjiiSeKHTt2DP68qip+y9xbb71V/PWvfy2uvfbabF+R1ozb6DrML94An3/++eXN78svvzz4CvdJK4fPDz8Y6IxbznvQRmpVaR842DH7dvSJs4uuxXTkPv8XXnjh4BrHKX2cXM3a9ebq8ssvr/3qp6n4CvasfMLLLKpK+6kFbaRmtREgN8y+Hb1J7qJrMR3Mvy/yR19ZzX7QRmpWGwFyw+zb0ZvkLroW08H8+yJ/9JXV7AdtpGa1ESA3zL4v8vdF/r7IH31lNftBG6lZbQTIDbPvi/x9kb8v8kdfWc1+0EZqVhsBcsPs+yJ/X+Tvi/zRV1azH7SRmtVGgNww+77I3xf5+yJ/9JXV7AdtpGa1ESA3zL4v8vdF/r7IH31lNftBG6lZbQTIDbPvi/x9kb8v8kdfWc1+0EZqVhsBcsPs+yJ/X+Tvi/zRV1azH7SRmtVGgNww+77I3xf5+yJ/9JXV7AdtpGa1ESA3zL4v8vdF/r7IH31lNftBG6lZbQTze+6554qTTz650cd0MPu+yN8X+fsif/SV1ewHbaRmtRF0O+qoo4q9e/cWjzzySOMYpoPZ90X+vsjfF/mjr6xmP2gjNauNoNt9991X/jrs2LGjcQzTwez7In9f5O+L/NFXVrMftJGa1UbQ7bPPPhv8Wlx55ZWN40iP2fdF/r7I3xf5o6+sZj9oIzWrjaDdxo0bB78Osfbt21ccf/zxjfOQVlXahw3y90X+vsgffWU1+0EbqVltBMOdcsop5Q1zrP379w9+PR566KHGuUirKu3DBvn7In9f5I++spr9oI3UrDaC4davX1/mf+DAgeL+++8f/HrwXujpq0r7sEH+vsjfF/mjr6xmP2gjNauNoGnx4sXFrl27yvw3bdpU9up17bXXNtYgnaq0Dxvk74v8fZE/+spq9oM2UrPaCJpuvfXWQf5XX3112avXO++801iDdKrSPmyQvy/y90X+6Cur2Q/aSM1qI2j66KOPyuy3bds26H3zzTeDX5P4nugzzjijsQ5pVKV92CB/X+Tvi/zRV1azH7SRmtVGMNcFF1wwyP6uu+4a9Kv3RFf1xBNPNNYijaq0Dxvk74v8fZE/+spq9oM2UrPaCObasGFDmfvu3buLJUuWDPorVqwYfCpHrPiKdP040qlK+7BB/r7I3xf5o6+sZj9oIzWrjeC/li1bVnz//fdl7uvWrWscf+WVVwa/LrHqr1Ajnaq0Dxvk74v8fZE/+spq9oM2UrPaCP7rL3/5S5n5Dz/8UJx00kmN4xdeeOHg1yXWv/71r8Y5mFxV2ocN8vdF/r7IH31lNftBG6lZbQT/tmjRomLnzp1l5i+//HLjeGXLli2DX5tYV1xxReMcTKYq7cMG+fsif1/kj76ymv2gjdSsNoJ/u/7668u8f/zxx/IHCfW4nlcVH2mXXlXahw3y90X+vsgffWU1+0EbqVltBP+2efPmMu8PPvigcawuvlIdv42wqvhNhT/72c8a52HhqtI+bJC/L/L3Rf7oK6vZD9pIzWoj+ElxzjnnlK88x4r/N37aRvxhwj179pT27t1bvi869uMNs9Zzzz3XeEwsXFXahw3y90X+vsgffWU1+0EbqVltBD8pXnjhhUHeseJNdLxRjl+YEm+ao/j/x96wG+jvvvuu/AQPfVwsTFXahw3y90X+vsgffWU1+0EbqVltpO+WL18++Oi6J598snG8zZtvvjn4NYr18MMPN87BwlSlfdggf1/k74v80VdWsx+0kZrVRvou3vjGiq8wn3nmmY3jbdasWTP4NYr19ddfF4cddljjPIyvKu3DBvn7In9f5I++spr9oI3UrDbSZ4sXLy5vfGMt5NM0vvjii8GvU6w77rijcQ7GV5X2YYP8fZG/L/JHX1nNftBGalYb6bM//elPg5zjK8p6fD4PPfTQYH2sTz/9tHEOxleV9mGD/H2Rvy/yR19ZzX7QRmrFIYcMpedhYeLH0W3fvr0clvh/9fgojj/++PLTOep15ZVXNs7DeKrSPmyQvy/y90X+6Cur2Q/aSE1vnLmBTuv2228fDMsjjzzSOD6q+NaPei3krSCYqyrtwwb5+yJ/X+SPvrKa/aCN1PTGmRvodFasWFHs2rVrMCxr165tnDOq+BnQ9Yofc3fNNdc0zsPoqtI+bJC/L/L3Rf7oK6vZD9pITW+cuYGe3Kmnnlo88MADgx8crCreTD/zzDPFddddV/5goa5TxxxzTPG73/2u2LBhQ/kZ0VrxEz3iK9G33HJLsXTp0sZ6zKUzXpWel6vG9bfQdbmatfwrmncXXZuTQc3Ydc8izbeU+fw3rreFrsuNXu8o9DFypdfdRdd6spr9oI3UNOQcwwZS0Bm3+k2cSuP6W+i6XM1a/hXNu4uuzcmgZuy6Z5HmW8p8/hvX20LX5UavdxT6GLnS6+6iaz1ZzX7QRmoaco5hAynojFv9Jk6lcf0tdF2uZi3/iubdRdfmZFAzdt2zSPMtZT7/jettoetyo9c7Cn2MXOl1d9G1nqxmP2gjNQ05x7CBFHTGrX4Tp9K4/ha6Llezln9F8+6ia3MyqBm77lmk+ZYyn//G9bbQdbnR6x2FPkau9Lq76FpPVrMftJGahpxj2EAKOuMzV3r9bajplubdZRZKr/kQnv9T03xnYjb0ettQfqW/Fl0yLP19klrQRmqNkP9DzwNmnc74zJVefxtquqV5d5mF0ms+hOf/1DTfmZgNvd42lF/pr0WXDEt/n6QWtJFaI+T/0POAWaczbvWbOJXG9bfQdd70+kalj5MLvc4uujYng5qx655Fmm8p8+efxvW20HWzIPfsR6W/Fl10rSer/IM2UrPaCOBNn1BmbfYb199C13nT6xuVPk4u9Dq76NqcDGrGrnsWab6lzJ9/GtfbQtfNgtyzH5X+WnTRtZ6s8g/aSM1qI4A3fUKZtdlvXH8LXedNr29U+ji50OvsomtzMqgZu+5ZpPmWMn/+aVxvC103C3LPflT6a9FF13qyyj9oIzWrjQDe9All1ma/cf0tdJ03vb5R6ePkQq+zi67NyaBm7LpnkeZbyvz5p3G9LXTdLMg9+1Hpr0UXXevJKv+gjdSsNgJ40ycUZt9GI/cR6eMgrUGR/dRpviWef9yQvS+r/IM2UrPaCOCNP8CA/2L+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t8X+fshe19W+QdtpGa1ESA3zD76jPn3Rf5+yN6XVf5BG6lZbQTIDbOPPmP+fZG/H7L3ZZV/0EZqVhsBcsPso8+Yf1/k74fsfVnlH7SRmtVGgNww++gz5t/XrOW/YsWK4t577y1effXV4tNPPy1++OGH4sCBA8XevXuLL7/8svjggw+K5557rrjjjjuKU045pbE+J7OW/cHGKv+gjdSsNgLkhtlPa/HixYNMx6l9+/YV3377bfHxxx+XfwBfeumljcdGelVpHzZmJf/Vq1cX7777bnmzPE599tlnxbp164qzzz678ZjeqtK+leOOO66W1MJq//79xXfffVd89dVXxUcffVQ8//zzxc0331wcccQRjf9ebqrSfmpBG6lZbQTIDbOf3sqVK4u77rqreOmll8pXpRZab7zxRnHUUUc1Hh/pVKV92Mg9/2XLlhWvv/568eOPPw6uddu2bcUjjzxSXH755eVfmJcvX16cc845xfXXX19s2LCh2L179+Dcqr755pvGY3urSvuW4nPln/70p+Lll18uvv7661pik9WePXuKxx57LOsb6aq0n1rQRmpWGwFyw+xP12GHHTbIONZDDz1UnHfeeeWrL0uWLCkOP/zw4qSTTir/MP7HP/5RPvHXa/PmzY3HRDpVaR82cs4/3hTv3LlzcI3xVc54k6znqXjTtn79+jmvVu/atatxnreqtO8l/mUkvmJfr+uuu65xXnzOPPbYY8ub76uvvrr461//WmzZsmXovw7ExzvrrLMaj5GDqrSfWtBGalYbAXLD7E9fveZ7a0Z8stdXrW+//fbGeUijKu3DRq75n3vuueVbqqr65JNPihNOOKFxXpf//d//HayPr67qcW9Vad9T/Fe3esUbZD2nzcUXX1y+jUMr/iVo3F87C1VpP7WgjdSsNgLkhtmfvnqdeeaZjeNq0aJFc/7w3rFjR+McpFGV9mEjx/zfeeedwXXFVzVXrVrVOGdUd999d/k48V+W9Ji3qrTv6bXXXhtcV6xf//rXjXNGEd9Ko7V27drGeZ6q0n5qQRupFYccMpSeB8w6nfE2ug4LV6/4U/x6fJj6P2Vu3769cRzj0xnvomsxOc24ja6zFN8yUK/4iqieM474yme8CY8/JKzHrGnObXSdpVdeeWVO/gv9y8sxxxzTeC+659vhNGPLvIM2UtMNWW0MsKYz3kbXYeHqFX/gSI8PU38FeuPGjY3jGJ/OeBddi8lpxm10naWtW7fWfrcWxfnnn984Z1zxhw5jxZs6PWZJc26j6yzFHyas1yWXXNI4Z1SPP/74nMeKf5HxeiuHZmyZd9BGarohq40B1nTG2+g6LFy94g+/6HEV/9Cu1yg/uIT56Yx30bWYnGbcRtdZ+eUvfznn9138VyA9ZyGqtxPEH3rTY5Y05za6ztKLL74459fgoosuapwzKn0ejeX18ySasWXeQRup6YasNgZY0xlvo+uwcPUa5VWot99+e3B+/OlyPY6F0RnvomsxOc24ja6zEj85o15PPfVU45xZpjm30XWWXnjhhTm/BhdeeGHjnHHopxo98cQTjXMsaMaWeQdtpKYbstoYYE1nvI2uw8LVq+sGOn6MU/wSlaq++OKL4rTTTmuch4XRGe+iazE5zbiNrrPy4Ycf1n6n+r1aOS2acxtdZ6n+/BfrggsuaJwzDv1Ujknf075QmrFl3kEbqemGrDYGWNMZb6PrsHD1Ov7448veoYceWixdurQ4/fTTiyuuuKL8fOjqM2fjlza89dZbxYknnth4LCyczngXXYvJacZtdJ0VfbUyvqVDz5llmnMbXWfpmWeemfNrMOl70OPzaL02bdrUOMeCZmyZd9BGarohq40B1nTG2+g6LNw4FT9x45Zbbim/LEAfB5PRGe+iazE5zbiNrrNw9NFH62/F4owzzmicN8s05za6ztLTTz8959dg0hto/aHE9957r3GOBc3YMu+gjdR0Q1YbA6zpjLfRdVi4hdT3339ffiZqfIVaHw8LozPeRddicppxG11nIX4+u9YoP/A7SzTnNrrOUuob6FdffXXO43l9opFmbJl30EZquiGrjQHWdMbb6DosXL1OPfXU8uu941s4oiOPPLLsXXbZZcWDDz5YfmlKveKN9Jo1axqPifHpjHfRtZicZtxG11mIn5Ch5fWRZ9OiObfRdZZS30DrWzji4+s5FjRjy7yDNlLTDVltDLCmM95G12Hh6nXcccc1jtfFm2v9Fq14E/2LX/yicS7GozPeRddicppxG11nIX7BkdbZZ5/dOG+Wac5tdJ2lZ599ds6vwaQ30B9//PGcx7v33nsb51jQjC3zDtpITTdktTHAms54G12HhavXfDfQlfrXCcd6/fXXG+dgPDrjXXQtJqcZt9F1FpYsWVJ+0Ua9Fvo10rnSnNvoOkv6KRyT3kDrD4aed955jXMsaMaWeQdtpFaV9oGDHbM/ffUa9QY6foFAvfbu3ds4B5OrSvuwkVP+X3755eB6Yt1///2Ncw4mVWnfk34O9CQ30PpFKvFTjvQcT1VpP7WgjdSsNgLkhtmfvnqNegMdxZvmep100kmNczCZqrQPGznl37d/9alK+570mwgn+SjBhx9+eM5jPfbYY41zPFWl/dSCNlKz2giQG2Z/+uo1zg20viLGl6qkV5X2YSOn/OMrzvXatWtX+TMJet7Boirte9KPnfvVr37VOGcUixYtmvMD2fHnSHL7odCqtJ9a0EZqVhsBcsPsT1+9xrmBrr9/b//+/eX7NPUcTKYq7cNGTvnHT8OJv8/qdeuttzbOO1hUpX1P+rFzF198ceOcUcRvkazXI4880jjHW1XaTy1oIzWrjQC5Yfanr16j3kDHV5vr9a9//atxDiZXlfZhI7f89WPPPv300/LVTD3vYFCV9j3Fr9qu16WXXto4Zz7xOXb37t2Dx4ifxLF48eLGed6q0n5qQRupWW0EyA2zP331GvUGOn61d73iZ0TrOZhcVdqHjdzyv+CCCxqvQuf46mUKVWnfU/yik3qtXr26cU6X+K90H3300WD9119/ne03Slal/dSCNlKz2giQG2Z/+uo1yg10fML/9ttvB2vie/niF67oeZhcVdqHjRzzf+aZZwbXFWvfvn3FVVdd1Thv1lWlfU9vv/12Lfmi+M1vftM4p83xxx9ffPjhh4O18T3s5557buO8XFSl/dSCNlKz2giQG2Z/+uo13w30OeecU3z++eeD8+MncUzyk+joVpX2YSPH/OM/9//zn/8cXFusH374objpppsa584nvv3jj3/8Y/HJJ58UxxxzTOO4p6q072nz5s211Ivi6quvbpwzzC233FLeMFcV8z799NMb5+WkKu2nFrSRmtVGgNww+9NXr/iHcPyGs2XLlpU/4X/UUUcVZ511VtmPP0ATX+2qKn5uKTfP01WV9mEj1/zj70+9iY5ftBI/JeKUU05pnK/iWwnuuuuuwV+G41+E16xZ0zjPU1Xa9/TZZ58NrivWdddd1zhn6dKlxZlnnlmsXbu2WL9+ffHFF18Mzo+ftvHoo4/OxKenVKX91II2UrPaCJAbZj+9eEMcf3o//lNw/dXkUeu7774rHn/88exesToYVaV92Mg5//i2qfjNeD/++OPgOmPFm7T4w4Z/+tOfys9mP/zww8v/G/+ye88995R/Ea4+QSeeG38vL1++vPH43qrSvqV4IxxfPHjiiSfKV40XWvGHBuNjnHzyyY3/Rq6q0n5qQRupWW0EyA2zn1b8599RK77aHF+Z+uqrr4otW7YUzz//fPmHSXxVWh8X01GV9mFjFvK/7LLLGm8t6Kp4w71169bivvvuK1/J1sfLRVXatxLfzjZuxefM+JeT7du3l198E19tvvzyy2fyk1Kq0n5qQRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRoDcMPvoM+bfF/n7IXtfVvkHbaRmtREgN8w++oz590X+fsjel1X+QRupWW0EyA2zjz5j/n2Rvx+y92WVf9BGalYbAXLD7KPPmH9f5O+H7H1Z5R+0kZrVRsZVHHLIyHRtrvS62+i6WaJ76aJrreU6+200vza6Lld63V10ba70utvoOg+zNv91mmcbXZeTWcpfc22j63I1S9nXad5ddG1OrPIP2kjNaiPj0mHoomtzpdfdRtfNEt1LF11rLdfZb6P5tdF1udLr7qJrc6XX3UbXeZi1+a/TPNvoupzMUv6aaxtdl6tZyr5O8+6ia3NilX/QRmpWGxmXDkMXXZsrve42um6W6F666Fpruc5+G82vja7LlV53F12bK73uNrrOw6zNf53m2UbX5WSW8tdc2+i6XM1S9nWadxddmxOr/IM2UrPayLh0GLro2lzpdbfRdbNE99JF11rLdfbbaH5tdF2u9Lq76Npc6XW30XUeZm3+6zTPNrouJ7OUv+baRtflapayr9O8u+janFjlH7SRmtVGxqXD0EXX5kqvu42umyW6ly661lqus99G82uj63Kl191F1+ZKr7uNrvMwa/Nfp3m20XU5maX8Ndc2ui5Xs5R9nebdRdfmxCr/oI3Usq0hA9FqVkqvu80sl+6lCzVeaX5tZqX0urvMSul1t6EmK82zDZWmNNc21HRL8+4yA6X3o6kFbaSWbekwdJmV0utuM8ule+lCjVeaX5tZKb3uLrNSet1tqMlK82xDpSnNtQ013dK8u8xA6f1oakEbAAAAANoFbQAAAABoF7QBAAAAoF3QBgAAAIB2QRsLNcqbtkc5BwAAAMhZ0MZC1G+K226QRzkHAAAAyF3QxkKMcnM8yjkAAABA7oI2FmKUm+NRzgEAAAByF7SxEKPcHI9yDgAAAJC7oI2FGOXmeJRzAAAAgNwFbSzEKDfHo5wDAAAA5C5oYyHabo6rGtbXxwAAAABmQdDGQtVvltt6+r8BAACAWRO0kRI3zAAAADjYBG2kxM0zAAAADjZBGwAAAADaBW0AAAAAaBe0AQAAAKBd0AYAAACAdkEbAAAAANoFbQAAAABoF7QBAAAAoF3QBgAAAIB2QRsAAAAA2v0/EkaqzbYZqi0AAAAASUVORK5CYII=>