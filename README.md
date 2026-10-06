# E-Commerce Microservices Platform

> A production-style e-commerce backend built with **.NET 10**: six independently deployable services, an **event-driven Saga with compensating transactions**, and a **real payment gateway integration** (SSLCommerz).

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Dapper-CC2927?logo=microsoftsqlserver&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-MassTransit-FF6600?logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-RabbitMQ-2496ED?logo=docker&logoColor=white)
![Status](https://img.shields.io/badge/status-backend%20core%20complete-brightgreen)

---

## Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [The checkout Saga](#the-checkout-saga)
- [Payment security model](#payment-security-model)
- [Engineering highlights](#engineering-highlights)
- [Service reference](#service-reference)
- [Tech stack](#tech-stack)
- [Getting started](#getting-started)
- [Try it: end-to-end walkthrough](#try-it-end-to-end-walkthrough)
- [Design decisions and trade-offs](#design-decisions-and-trade-offs)
- [Known limitations and roadmap](#known-limitations-and-roadmap)
- [Repository structure](#repository-structure)

---

## Overview

A customer browses a catalog, fills a cart, checks out, pays through a real payment gateway, and gets an email. Behind that flow, six services each own one business capability and one database, and coordinate through events instead of shared tables or chained synchronous calls.

| Service | Owns | Highlights |
|---|---|---|
| **Identity** | Users, roles, tokens | JWT access + refresh tokens with rotation, immediate revocation on logout, role-based authorization |
| **Catalog** | Products, categories, images | Soft delete, pagination and filtering, multi-image support, batched lookups for other services |
| **Order** | Cart, orders | Live-priced cart, transactional checkout with price snapshots, Saga participant, Hangfire cleanup job |
| **Inventory** | Stock, reservations | Atomic oversell-safe reservation, compensating stock release, admin stock tools |
| **Payment** | Payments | SSLCommerz sandbox (redirect + IPN), server-side validation, duplicate-callback protection |
| **Notification** | Notification log | Event-driven transactional email (SMTP), send-once guard, audit trail |

Everything above is implemented and was verified end to end against the real SSLCommerz sandbox: the success path (paid, order confirmed, email sent) and the failure path (payment cancelled, order cancelled, stock released).

---

## Architecture

### System view

```mermaid
flowchart TB
    Client(["Client<br/>Swagger today, React UI planned"])

    subgraph SVC["Microservices (.NET 10)"]
        ID["Identity Service<br/>:7278"]
        CAT["Catalog Service<br/>:7179"]
        ORD["Order Service<br/>:7009"]
        INV["Inventory Service<br/>:7301"]
        PAY["Payment Service<br/>:7165"]
        NOT["Notification Service"]
    end

    MQ{{"RabbitMQ<br/>via MassTransit"}}

    IDDB[("IdentityServiceDb")]
    CATDB[("CatalogServiceDb")]
    ORDDB[("OrderServiceDb")]
    INVDB[("InventoryServiceDb")]
    PAYDB[("PaymentServiceDb")]
    NOTDB[("NotificationServiceDb")]

    SSL["SSLCommerz<br/>payment gateway sandbox"]
    MAIL["Mailtrap<br/>SMTP sandbox"]

    Client --> ID
    Client --> CAT
    Client --> ORD
    Client --> INV
    Client --> PAY

    ID --> IDDB
    CAT --> CATDB
    ORD --> ORDDB
    INV --> INVDB
    PAY --> PAYDB
    NOT --> NOTDB

    ORD -.->|"product data"| CAT
    ORD -.->|"stock levels"| INV
    PAY -.->|"order amount"| ORD
    PAY -.->|"customer info"| ID
    NOT -.->|"order lookup"| ORD
    NOT -.->|"customer email"| ID

    ORD <-->|"events"| MQ
    INV <-->|"events"| MQ
    PAY <-->|"events"| MQ
    MQ --> NOT

    PAY <-->|"initiate, validate, IPN"| SSL
    NOT -->|"SMTP"| MAIL
```

**How to read it:** solid arrows are client requests, data access, and asynchronous events. Dotted arrows are synchronous HTTP calls between services. Each service has its own database and never reads another service's tables.

### Inside each service

Every service uses the same three-project layout, so once you've read one, you've read them all.

```mermaid
flowchart LR
    API["<b>Api</b><br/>Controllers, auth,<br/>Swagger, Program.cs"]
    APP["<b>Application</b><br/>Commands, Queries, Handlers,<br/>Models, Interfaces, Consumers"]
    INF["<b>Infrastructure</b><br/>Dapper repositories, HTTP clients,<br/>MassTransit, SMTP, gateways"]

    API --> APP
    INF --> APP
    API -.->|"DI registration only"| INF
```

- **Application** depends on nothing. It defines interfaces such as `IPaymentGateway`, `IEmailSender`, `IStockRepository`, and `IFileStorageService`.
- **Infrastructure** implements those interfaces. Swapping SSLCommerz for Stripe, Mailtrap for SendGrid, or local disk for S3 means writing one new class and changing one line of DI, with no changes to business logic.
- **Api** is thin. Controllers only dispatch to MediatR (CQRS).
- There is deliberately no separate Domain project (see [trade-offs](#design-decisions-and-trade-offs)).

### Shared code

`ECommerce.Contracts` is the only code shared between services. It contains event shapes and nothing else, no logic. Services never reference each other's projects.

---

## The checkout Saga

Checkout touches four services but uses no distributed transaction. Each service does its own local work and announces the result as an event (a **choreographed Saga**). When something fails, services run compensating actions instead of rolling back.

### Success path

```mermaid
sequenceDiagram
    autonumber
    actor C as Customer
    participant O as Order
    participant MQ as RabbitMQ
    participant I as Inventory
    participant P as Payment
    participant G as SSLCommerz
    participant N as Notification

    C->>O: POST /api/orders/checkout
    Note over O: Order saved as Pending in one transaction, cart cleared
    O-->>C: 200 with orderId
    O->>MQ: OrderPlaced
    MQ->>I: OrderPlaced
    Note over I: Atomic conditional UPDATE reserves stock
    I->>MQ: StockReserved
    MQ->>O: StockReserved
    Note over O: Status becomes StockReserved
    MQ->>P: StockReserved
    P->>G: Initiate session
    G-->>P: GatewayPageURL
    C->>G: Pays on the hosted page
    G->>P: IPN callback to /api/payments/ipn
    P->>G: Validation API with val_id
    G-->>P: VALID and amount
    P->>MQ: PaymentCompleted
    MQ->>O: PaymentCompleted
    Note over O: Status becomes Confirmed
    MQ->>N: PaymentCompleted
    N-->>C: Confirmation email
```

### Failure paths and compensation

```mermaid
sequenceDiagram
    autonumber
    participant O as Order
    participant MQ as RabbitMQ
    participant I as Inventory
    participant P as Payment
    participant N as Notification

    Note over P: IPN fails validation, or the amount does not match
    P->>MQ: PaymentFailed
    MQ->>O: PaymentFailed
    Note over O: Status becomes Cancelled
    O->>MQ: StockReleaseRequested
    MQ->>I: StockReleaseRequested
    Note over I: Reservation released, stock available again
    MQ->>N: PaymentFailed
    Note over N: Cancellation email sent
```

| Failure | Detected by | Result |
|---|---|---|
| Not enough stock at reservation time | Inventory (conditional UPDATE affects 0 rows) | `StockUnavailable` → order cancelled, customer emailed. Any items already reserved for that order are rolled back inside Inventory. |
| Payment declined, cancelled, or invalid | Payment (gateway validation) | `PaymentFailed` → order cancelled, stock released, customer emailed |
| Amount in the callback differs from the order total | Payment (amount check) | Treated as a failed payment |
| Duplicate IPN from the gateway | Payment (`ProcessedIpnCallbacks`) | Ignored |
| Duplicate event delivery to Notification | Notification (send-once guard) | No second email |

### Order lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending: checkout
    Pending --> StockReserved: StockReserved event
    Pending --> Cancelled: StockUnavailable event
    StockReserved --> Confirmed: PaymentCompleted event
    StockReserved --> Cancelled: PaymentFailed event
    Confirmed --> [*]
    Cancelled --> [*]
```

`Shipped` and `Delivered` exist in the status enum for a future fulfillment flow but are not driven by any event yet.

### Events

| Event | Published by | Consumed by |
|---|---|---|
| `OrderPlaced` | Order | Inventory |
| `StockReserved` | Inventory | Order, Payment |
| `StockUnavailable` | Inventory | Order, Notification |
| `PaymentCompleted` | Payment | Order, Notification |
| `PaymentFailed` | Payment | Order, Notification |
| `StockReleaseRequested` | Order | Inventory |

Each consumer has its own queue (for example `inventory-order-placed-queue`, `notification-payment-failed-queue`), so one event can fan out to several services independently.

---

## Payment security model

The payment flow rests on one rule: **nothing a customer's browser can reach is allowed to mark an order as paid.**

```mermaid
flowchart TD
    A["Customer pays on the SSLCommerz page"] --> B["Browser is redirected to<br/>/success, /fail or /cancel"]
    A --> C["SSLCommerz server POSTs an IPN<br/>to /api/payments/ipn"]

    B --> D["Returns a friendly message.<br/>Changes nothing."]

    C --> E{"Already processed<br/>this transaction?"}
    E -- Yes --> F["Ignore the duplicate"]
    E -- No --> G["Call the SSLCommerz Validation API<br/>with val_id, independently"]
    G --> H{"Status is VALID and<br/>amount matches the order?"}
    H -- Yes --> I["Payment = Completed<br/>publish PaymentCompleted"]
    H -- No --> J["Payment = Failed<br/>publish PaymentFailed"]
```

- The `/success`, `/fail`, and `/cancel` URLs are ordinary public endpoints. Anyone can visit them, so they only display a message.
- The IPN body is not trusted either. Payment Service takes the `val_id` from it and asks SSLCommerz directly whether the transaction is real and what amount was paid. Only that answer counts.
- A customer can forge a request to our server. They cannot forge SSLCommerz's answer to a server-to-server call.

---

## Engineering highlights

| Topic | What was done |
|---|---|
| **Overselling prevention** | Stock reservation is a single conditional `UPDATE ... WHERE QuantityAvailable >= @qty` (`TryReserveStockAsync`), so the check and the decrement can't be separated by a concurrent request. A multi-item order that partially succeeds rolls back its earlier reservations. |
| **Compensating transactions** | Payment failure triggers order cancellation and a `StockReleaseRequested` event that Inventory handles with the same release logic used for partial rollback. |
| **Gateway trust boundary** | Browser redirects are display-only. The IPN is cross-checked against SSLCommerz's Validation API, including amount, and deduplicated by transaction id. |
| **Immediate token revocation** | JWTs are stateless, so logout alone can't invalidate an access token. Logout stores the token's `jti` in a blocklist checked by middleware that runs between authentication and authorization, and rotates and revokes refresh tokens. The blocklist sits behind `ITokenBlocklistService` so a Redis implementation can replace SQL. |
| **IDOR protection** | `GetOrderById` and the payment queries compare the resource's `UserId` to the caller's JWT and return 404 on mismatch. The user id always comes from the token, never from the request body. |
| **Live vs snapshot data** | Cart lines store only `ProductId` and quantity and are priced live from Catalog. At checkout, name and price are copied into `OrderItems` so later catalog changes can't rewrite history. |
| **Fail-fast validation** | `AddToCart` checks live stock, including the quantity already in the cart, and rejects with a clear message before anything is written. The authoritative check still happens atomically in Inventory. |
| **Avoiding N+1** | Images for a page of products are loaded in one `IN (...)` query. Order Service uses Catalog's bulk endpoint instead of one call per cart line. |
| **Service-to-service auth** | Internal endpoints (`/internal/...`) are protected by an `X-Internal-Api-Key` filter, separate from customer JWTs. |
| **Transactions** | Order plus items are written in one DB transaction. The cart is cleared only after the order is safely stored, and events are published after the commit. |
| **Replaceable infrastructure** | Gateway, email, file storage, token blocklist, and messaging all sit behind interfaces defined in the Application layer. |
| **Background jobs** | Hangfire runs a daily job that deletes carts untouched for 30 days. |

---

## Service reference

<details>
<summary><b>Identity Service</b> (<code>:7278</code>, <code>IdentityServiceDb</code>)</summary>

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register (always a Customer) |
| POST | `/api/auth/login` | Public | Returns access and refresh tokens |
| POST | `/api/auth/refresh` | Public | Exchange a refresh token (single-use, rotated) |
| POST | `/api/auth/logout` | Authenticated | Revokes the refresh token and blocklists the access token |
| GET | `/api/auth/me` | Authenticated | Current user |
| GET | `/api/auth/internal/{userId}` | Internal API key | User lookup for other services |

Roles: `Customer`, `Admin`. The first Admin is promoted manually in the database, deliberately, since there is no self-service path to elevated roles.

Tables: `Users`, `RefreshTokens`, `RevokedAccessTokens`.
</details>

<details>
<summary><b>Catalog Service</b> (<code>:7179</code>, <code>CatalogServiceDb</code>)</summary>

| Method | Route | Access | Purpose |
|---|---|---|---|
| GET | `/api/products` | Public | Paged list, optional `categoryId` filter |
| GET | `/api/products/{id}` | Public | Product with images |
| GET | `/api/products/bulk?ids=` | Public | Batched lookup (used by Order) |
| GET | `/api/products/ids` | Public | All active product ids (used by Inventory) |
| POST | `/api/products` | Admin | Create with multiple images (`multipart/form-data`) |
| PUT | `/api/products/{id}` | Admin | Update (SKU is immutable) |
| DELETE | `/api/products/{id}` | Admin | Soft delete |
| POST | `/api/products/{id}/image` | Admin | Add an image |
| PATCH | `/api/products/{id}/images/{imageId}/set-primary` | Admin | Choose the primary image |
| DELETE | `/api/products/{id}/images/{imageId}` | Admin | Remove an image |
| GET / POST | `/api/categories` | Public / Admin | List and create categories |

Images live in a generic `Files` table (`EntityType` + `EntityId`) so other entities can reuse it. Storage is local disk behind `IFileStorageService`, ready for an S3 implementation.

Tables: `Categories`, `Products`, `Files`.
</details>

<details>
<summary><b>Order Service</b> (<code>:7009</code>, <code>OrderServiceDb</code>)</summary>

| Method | Route | Access | Purpose |
|---|---|---|---|
| GET | `/api/cart` | Customer | Cart with live price, image, and stock availability message |
| POST | `/api/cart/items` | Customer | Add (idempotent: same product increments quantity) |
| DELETE | `/api/cart/items/{productId}` | Customer | Remove a line |
| DELETE | `/api/cart` | Customer | Clear the cart |
| POST | `/api/orders/checkout` | Customer | Convert the cart to an order and start the Saga |
| GET | `/api/orders/{orderId}` | Customer | Own order only |
| GET | `/api/orders/my-orders` | Customer | Order history |
| GET | `/api/orders/internal/{orderId}` | Internal API key | Order lookup for other services |
| | `/hangfire` | Dev only | Job dashboard |

Consumes: `StockReserved`, `StockUnavailable`, `PaymentCompleted`, `PaymentFailed`. Publishes: `OrderPlaced`, `StockReleaseRequested`.

Tables: `Carts`, `CartItems`, `Orders`, `OrderItems` (plus `HangfireDb` for job storage).
</details>

<details>
<summary><b>Inventory Service</b> (<code>:7301</code>, <code>InventoryServiceDb</code>)</summary>

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/stock` | Admin | Set initial stock for a product (rejected if it already exists) |
| POST | `/api/stock/adjust` | Admin | Add or remove stock (cannot go negative) |
| GET | `/api/stock/missing` | Admin | Products that exist in Catalog but have no stock record |
| GET | `/api/stock-levels?productIds=` | Public | Available quantities |

Consumes: `OrderPlaced`, `StockReleaseRequested`. Publishes: `StockReserved`, `StockUnavailable`.

Cataloging a product and stocking it are deliberately separate business actions, so there is no hidden coupling between the two services. `GET /api/stock/missing` is the reconciliation tool for products someone forgot to stock.

Tables: `Stock`, `StockReservations`.
</details>

<details>
<summary><b>Payment Service</b> (<code>:7165</code>, <code>PaymentServiceDb</code>)</summary>

| Method | Route | Access | Purpose |
|---|---|---|---|
| GET | `/api/payments/{orderId}/redirect-url` | Customer | Hosted checkout URL for the order |
| GET | `/api/payments/order/{orderId}` | Customer | Own payment status |
| GET | `/api/payments/my-payments` | Customer | Payment history |
| POST | `/api/payments/success`, `/fail`, `/cancel` | Gateway redirect | Display only, change nothing |
| POST | `/api/payments/ipn` | Gateway callback | Validated, authoritative payment result |

Consumes: `StockReserved`. Publishes: `PaymentCompleted`, `PaymentFailed`.

Tables: `Payments`, `ProcessedIpnCallbacks`.
</details>

<details>
<summary><b>Notification Service</b> (<code>appsettings</code> port, <code>NotificationServiceDb</code>)</summary>

No public API. It only reacts to events.

| Event | Email |
|---|---|
| `PaymentCompleted` | Order confirmed |
| `PaymentFailed` | Order cancelled, payment failed |
| `StockUnavailable` | Order cancelled, item unavailable |

A shared dispatcher resolves order, then user, then email address through the other services, skips the send if one was already recorded as sent for that order and type, and writes every attempt (`Sent` or `Failed` with the error) to `NotificationLogs`. A failed send is logged and not retried forever.

Tables: `NotificationLogs`.
</details>

---

## Tech stack

| Area | Choice |
|---|---|
| Runtime | .NET 10, ASP.NET Core Web API |
| Data access | Dapper (raw SQL), SQL Server, one database per service |
| Patterns | CQRS with MediatR, repository per aggregate, FluentValidation |
| Messaging | RabbitMQ through MassTransit |
| Auth | JWT bearer, BCrypt password hashing, refresh token rotation |
| Payments | SSLCommerz sandbox (hosted checkout and IPN) |
| Email | MailKit over SMTP, Mailtrap sandbox inbox |
| Background jobs | Hangfire (SQL Server storage) |
| API docs | Swagger / OpenAPI per service |
| Local infrastructure | Docker (RabbitMQ), ngrok (public URL for the payment IPN) |

---

## Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server (Express is fine) and SSMS or another SQL client
- Docker Desktop
- Free accounts for: [SSLCommerz sandbox](https://developer.sslcommerz.com/registration/), [Mailtrap](https://mailtrap.io), [ngrok](https://ngrok.com)

### 1. Clone

```powershell
git clone https://github.com/tusar-molla/ecommerce-microservices.git
cd ecommerce-microservices
```

### 2. Create the databases

Run [`database/setup.sql`](database/setup.sql) in SSMS. It creates all seven databases and their tables, and is safe to re-run.

### 3. Configure secrets

Real configuration is not in git. Each service ships an `appsettings.Development.example.json`. Copy it to `appsettings.Development.json` and fill it in:

```powershell
Get-ChildItem -Recurse -Filter appsettings.Development.example.json | ForEach-Object {
    $target = Join-Path $_.DirectoryName "appsettings.Development.json"
    if (-not (Test-Path $target)) { Copy-Item $_.FullName $target }
}
```

Values that **must be identical** across services, or you'll see unexplained `401`s:

| Key | Must match in |
|---|---|
| `Jwt:SecretKey` (32+ characters), `Jwt:Issuer`, `Jwt:Audience` | All services |
| `InternalApiKey` | Identity, Order, Payment, Notification |

Other values to fill in: SQL connection strings, `SslCommerz:StoreId` and `StorePassword` (Payment), `Smtp:Username` and `Password` (Notification, from your Mailtrap inbox), and each `Services:*BaseUrl` to match the target service's port.

The launch profiles set `ASPNETCORE_ENVIRONMENT=Development`, which is what loads these files.

### 4. Start RabbitMQ

```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Management UI: http://localhost:15672 (guest / guest).

### 5. Run the services

Start each in its own terminal, or run them from Visual Studio:

```powershell
dotnet run --project IdentityService/IdentityService.Api --launch-profile https
dotnet run --project CatalogService/CatalogService.Api --launch-profile https
dotnet run --project OrderService/OrderService.Api --launch-profile https
dotnet run --project InventoryService/InventoryService.Api --launch-profile https
dotnet run --project PaymentService/PaymentService.Api --launch-profile https
dotnet run --project NotificationService/NotificationService.Api --launch-profile https
```

| Service | Swagger |
|---|---|
| Identity | https://localhost:7278/swagger |
| Catalog | https://localhost:7179/swagger |
| Order | https://localhost:7009/swagger |
| Inventory | https://localhost:7301/swagger |
| Payment | https://localhost:7165/swagger |
| Notification | Port in its `launchSettings.json` |

Order, Payment, and Notification call other services over HTTP, so start Identity, Catalog, and Inventory first. In RabbitMQ's **Queues** tab you should see one queue per consumer once everything is up.

### 6. Expose Payment Service for the gateway callback

SSLCommerz's servers must reach your machine to deliver the IPN:

```powershell
ngrok http https://localhost:7165
```

Put the forwarding URL in Payment Service's `App:BaseUrl` and **restart Payment Service**. The free ngrok URL changes every session, so repeat this each time.

---

## Try it: end-to-end walkthrough

1. **Create an Admin.** Register through `POST /api/auth/register`, then promote the account and log in again so the token carries the role:
   ```sql
   USE IdentityServiceDb;
   UPDATE Users SET Role = 'Admin' WHERE Email = 'you@example.com';
   ```
2. **Seed the catalog.** As Admin: create a category, then a product (`POST /api/products`, with an image if you like).
3. **Stock it.** `GET /api/stock/missing` shows the product has no stock. Fix it with `POST /api/stock` and a quantity.
4. **Shop.** Register and log in as a Customer, `POST /api/cart/items`, then `GET /api/cart` to see live price and availability.
5. **Check out.** `POST /api/orders/checkout`. Within a few seconds `GET /api/orders/{id}` shows `StockReserved`.
6. **Pay.** `GET /api/payments/{orderId}/redirect-url`, open the returned URL in a browser, and complete a sandbox payment (test credentials are on the SSLCommerz sandbox dashboard).
7. **Watch it resolve.** The order becomes `Confirmed`, and the email appears in your Mailtrap inbox. The ngrok inspector at http://127.0.0.1:4040 shows the IPN arriving.
8. **Break it on purpose.** Cancel the payment on the gateway page instead. The order becomes `Cancelled`, `Stock.QuantityAvailable` returns to its earlier value, and a cancellation email is sent.

---

## Design decisions and trade-offs

- **Choreography over orchestration.** With three or four participants, independent consumers reacting to events are simple and loosely coupled. The cost is that the whole flow isn't visible in one place. If more steps are added, a MassTransit state-machine saga would centralize it.
- **Dapper instead of EF Core.** SQL stays explicit and fast, and the business logic doesn't need a rich domain model. That also drove the choice of three projects per service instead of four (no separate Domain project).
- **Catalog and Inventory are separate on purpose.** Catalog is read-heavy and changes rarely. Inventory is write-heavy and needs strong consistency. Cataloging a product and stocking it are two business events, so creating a product does not silently create stock.
- **Database per service.** No cross-service joins. Data that crosses a boundary travels by HTTP call (when needed now) or event (when it can be eventual).
- **Reservation is authoritative in Inventory, advisory elsewhere.** The cart shows stock hints and rejects obviously impossible quantities early, but only the atomic update in Inventory decides, because stock can change between adding to cart and checking out.
- **Test-friendly infrastructure.** Sandbox gateway, sandbox inbox, local disk storage, and SQL-backed token blocklist are all behind interfaces, so the production versions are additive changes.

---

## Known limitations and roadmap

Documented honestly so a reader knows what is and isn't covered.

**Known gaps**

- **Dual-write problem.** An order is saved and then its event is published as two separate steps. A crash between them would leave an order stuck in `Pending`. The fix is the Outbox Pattern.
- **Idempotency is partial.** Payment (IPN) and Notification are protected against duplicates. The Inventory and Order consumers don't yet deduplicate redelivered messages.
- **No automated tests yet.** The flows were verified manually and through the real sandbox. Unit and integration tests are planned, plus a concurrent-checkout test to demonstrate the overselling guarantee under load.
- **Secrets.** Config lives in gitignored files and the repo ships example templates, which suits local development. A secrets manager would be used in a real deployment.
- **Cart is cleared at checkout,** before stock and payment are confirmed. A cancelled order doesn't restore it.

**Roadmap**

- [ ] Outbox Pattern for reliable event publishing
- [ ] Idempotent consumers in Inventory and Order
- [ ] Docker Compose for one-command startup of all services and infrastructure
- [ ] API Gateway (YARP) as a single entry point
- [ ] Redis for caching and the token blocklist, plus rate limiting
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Unit and integration tests, concurrency test for stock reservation
- [ ] S3-backed file storage
- [ ] React frontend with SignalR for live order status

---

## Repository structure

```
ecommerce-microservices/
├── ECommerce.Contracts/          # shared event contracts (only shared code)
├── IdentityService/
│   ├── IdentityService.Api/
│   ├── IdentityService.Application/
│   └── IdentityService.Infrastructure/
├── CatalogService/               # same three-project layout
├── OrderService/
├── InventoryService/
├── PaymentService/
├── NotificationService/
├── database/
│   └── setup.sql                 # creates all databases and tables
└── README.md
```

---

## Author

**Mohammad Tusar Molla**, .NET backend developer.
GitHub: [@tusar-molla](https://github.com/tusar-molla)

<!-- Optional: add screenshots under docs/images and link them here (Swagger, RabbitMQ queues, the Mailtrap confirmation email, the ngrok IPN inspector). -->
