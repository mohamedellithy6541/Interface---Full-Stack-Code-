# AI usage

## Tool

| Tool | Model | Used for |
| --- | --- | --- |
| Claude Code (Claude desktop app, Code tab) | Claude Opus 5.5 | Converting the assessment PDF to Markdown, scaffolding the solution, writing the backend layers, unit tests, README and CI workflow |

## Prompts and sessions

- `sessions/` holds the exported chat transcripts, one file per session (exported when the work is finished).
- The prompt that started the backend work is in [`prompts/01-backend.md`](prompts/01-backend.md).

## What was AI-generated

| Area | AI-generated | Reviewed and adjusted by me |
| --- | --- | --- |
| `docs/Interface_Code_Assessment_2026.md` (PDF to Markdown) | Yes | Yes |
| Backend: Domain, Application, Infrastructure, Api | Yes, from my architecture and library choices (Clean Architecture, CQRS with MediatR, AutoMapper, FluentValidation, JWT + Google, global middleware, static registration classes) | Yes |
| Backend unit tests | Yes | Yes |
| README, CI workflow | Yes | Yes |

My decisions, given to the AI as instructions: the stack (ASP.NET Core Web API on .NET 9, Angular 21), Clean Architecture,
CQRS, AutoMapper, FluentValidation, JWT plus Google OAuth, RFC 7807 Problem Details from one global middleware,
pagination and search, and one static registration class per layer.

## Skills, agents, custom instructions, MCP servers

| Item | Used? | Why |
| --- | --- | --- |
| Agent skills / custom commands | No custom ones. The built-in PDF reading in Claude Code was used once to read the brief. | Convert the brief to Markdown so it could be used as context |
| Custom instructions (`CLAUDE.md`, `AGENTS.md`, `.cursorrules`, ...) | None | - |
| MCP servers / plugins | None for code generation | - |

The author is responsible for reviewing every generated line before submission and must be able to explain it.
