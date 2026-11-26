# Interface - Full-Stack Code Assessment (2026)

Thank you for taking the time to complete this assessment. It is designed to take roughly **6-8 hours**. Please do not spend more than that. We would rather see a smaller scope done well than a large scope done poorly.

---

## 1. Objective

Build a small **Employee Management and Payroll Calculation** application: a backend API with a front-end client.

We evaluate **how** your solution is organized, not only whether it works. You are free to choose your own tech stack (see section 5), as long as the rules in this document are followed.

Using any external library is allowed, and so is old code from your own repositories. Every external library must be used for a good reason, and you must be able to justify it.

---

## 2. Your Tasks

### Task 1 - Backend API and front-end client

Create a backend Web API and a front-end client that talks to it.

### Task 2 - Basic CRUD with validation and error handling

Implement CRUD operations for an **Employee** with at least these fields:

| Field               | Rules                              |
| ------------------- | ---------------------------------- |
| Full name           | Required, max 100 characters       |
| Email               | Required, valid format, unique     |
| Job title           | Required                           |
| Department          | Required                           |
| Hire date           | Required, cannot be in the future  |
| Base monthly salary | Required, greater than 0           |
| Status              | Active / Inactive                  |

Requirements:

- Input validation on the API (never trust the client). The UI should validate too.
- Proper HTTP status codes and a consistent error response format (e.g. RFC 7807 Problem Details).
- A global error-handling mechanism. Do not scatter try/catch blocks through every handler.
- List endpoint with **pagination** and basic **search or filter** (e.g. by name or department).
- The UI must support list, create, edit and delete, and show validation and server errors clearly to the user.

### Task 3 - Payroll calculation (core business logic)

Implement a payroll calculation that returns a monthly payslip for a given employee:

- **Gross** = base salary + allowances (fixed amounts, e.g. transport or housing)
- **Income tax** is calculated with **progressive tax brackets** (e.g. 0% up to X, 10% from X to Y, 20% above Y)
- **Social insurance** = a configurable percentage of base salary, with an optional cap
- **Net** = gross - tax - social insurance - other deductions

Requirements:

- Tax brackets, insurance rate and cap must be **configuration, not hard-coded**.
- Expose it through the API (e.g. `GET /employees/{id}/payslip?month=2026-03`) and show it in the UI.
- Handle edge cases explicitly (inactive employee, zero allowances, salary exactly on a bracket boundary, rounding rules, etc.). Document the rounding rule you chose.
- State any assumptions you make in the README. The numbers do not need to match any real country's rules.

### Task 4 - Unit tests

Cover the **core business logic** (at minimum the payroll calculation and the employee validation rules) with automated unit tests.

- Tests must run in isolation (no real database or network).
- Include meaningful edge-case tests, not just the happy path.
- Optional: a few integration or API tests, or front-end component tests.

### Task 5 (Optional) - Authentication and SSO

Choose **one**:

- **SSO** via OpenID Connect with any identity provider (Microsoft Entra ID, Keycloak, Auth0, Google...), **or**
- Simple JWT-based authentication with at least two roles (e.g. `HR` can do everything, `Employee` can only view their own payslip).

Authorization must be enforced on the **server side**, not only hidden in the UI.

---

## 3. Focus Points

- **Scalability**: the solution must be maintainable and able to grow (new deduction types, new tax rules, new modules such as leave or attendance).
- **Readability**: meaningful names for functions, variables and files.
- **Testability**: code must be easy to test in isolation.
- **Security**: never commit secrets. Use environment variables or configuration files that are git-ignored, and provide an `.env.example` or equivalent.
- **Comments**: only where the code is not self-explanatory. Prefer clear code over comments.
- **Your own code**: importing libraries must be for a good reason. Do not import a library for something trivial.
- **Money**: choose a safe way to represent money and decimals, and be ready to explain why.

---

## 4. Application Architecture

A well-structured application is the foundation of maintainable software.

- Use a **clear layered or modular architecture** (e.g. Clean, Onion, Hexagonal, N-Tier, or feature modules). The business rules should be separate from the framework, the database and the HTTP layer.
- Keep **business logic out of controllers** and **out of UI components**.
- Use **Dependency Injection** and program to interfaces or abstractions.
- Keep data access behind an abstraction (repository or equivalent) so that it can be replaced in tests.
- Briefly **explain your architectural choices in the README**.

---

## 5. Technologies

You may choose any of the following. Pick what you are strongest in and be ready to justify the choice.

| Area         | Allowed choices |
| ------------ | --------------- |
| **Backend**  | Node.js with **TypeScript** (NestJS preferred, Express or Fastify acceptable), **or** ASP.NET Core Web API (.NET 8 or later, LTS recommended) |
| **Database** | Any SQL or NoSQL database. An ORM or query builder is fine (e.g. TypeORM, Prisma, MikroORM, EF Core, Dapper). Provide a migration or seed approach. |
| **Frontend** | **React**, Angular or Vue.js, **with TypeScript** |
| **Styling**  | **CSS or SCSS** (no UI kit is required, but you may use one if you can justify it) |
| **API docs** | OpenAPI / Swagger |
| **Testing**  | Any mainstream framework (Jest, Vitest, xUnit, NUnit...) |

**Language requirement:** the submission must include real, hand-written code in **TypeScript** (frontend, and backend if you pick Node.js), **CSS/SCSS**, and your chosen backend language.

**Bonus:** Docker / docker-compose, a CI pipeline (GitHub Actions, GitLab CI...), structured logging, health-check endpoint.

---

## 6. AI Usage

Using AI tools (GitHub Copilot, ChatGPT, Claude, Cursor, etc.) is **allowed**. It is part of modern software development, and we value **transparency**.

- Attach the **full prompt or chat session** (exported transcript or share link) in an **`/ai-usage`** folder in your repository.
- Mention **which parts were AI-generated**.
- You must **understand and be able to explain every line** you submit.

### Skills and AI tooling

If you used any AI skills, agents or custom configurations, list and include them:

- Agent skills, custom commands or prompt files
- Custom instructions (e.g. `copilot-instructions.md`, `.cursorrules`, `CLAUDE.md`, `AGENTS.md`)
- MCP servers, plugins or extensions
- Your reason for using each one

*Not using AI is completely fine too. Just state it in the README.*

---

## 7. Submission

Please share the following with us:

- A **Git repository link** with a meaningful commit history (not a single commit).
- A **README** with:
  - setup and run instructions (ideally one or two commands),
  - an architecture overview and the reasoning behind your choices,
  - the assumptions you made,
  - known limitations and what you would do next with more time.
- The **`/ai-usage`** folder (chat sessions and skills used), if applicable.
- **Seed data or scripts** needed to run the application.

---

## 8. Evaluation Criteria

| #   | Criterion                                        | Weight |
| --- | ------------------------------------------------ | ------ |
| 1   | Application architecture and SOLID principles    | 25%    |
| 2   | Code quality and readability                     | 20%    |
| 3   | Functionality, validation and error handling     | 20%    |
| 4   | Testing                                          | 15%    |
| 5   | Transparency and judgment in AI usage            | 10%    |
| 6   | Bonus: SSO / auth, Docker, CI pipeline, logging  | 10%    |

We do **not** favour any particular stack. We evaluate the same engineering principles regardless of the technology you choose.
