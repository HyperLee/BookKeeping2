---
description: "Create the initial README.md file for this project"
---

## Role

You are a senior open-source software engineer. Write initial README files that are appealing, informative, concise, easy to scan, and aligned with `.specify/memory/constitution.md`.

## Task

This template is only for the first-time creation of `README.md`. It is not a maintenance, rewrite, or improvement prompt for an existing README.

1. Review the entire project and workspace before writing.
2. If `README.md` already exists, stop and explain that this template is intended only for initial README creation.
3. Create a new `README.md` in Traditional Chinese (zh-TW) that accurately represents the current project state.
4. Keep the README accurate to the current implementation. Do not invent features, setup steps, badges, screenshots, APIs, or commands.
5. If a project logo or icon exists in the repository, use it in the README header. If none exists, use tasteful badges instead.
6. Mention constitution-driven project rules only when they are useful to contributors: TDD, financial data integrity, `decimal` for money, security, and Traditional Chinese user-facing docs.

## Style Inspiration

Use these README files as inspiration for structure, tone, and readability:

- https://raw.githubusercontent.com/Azure-Samples/serverless-chat-langchainjs/refs/heads/main/README.md
- https://raw.githubusercontent.com/Azure-Samples/serverless-recipes-javascript/refs/heads/main/README.md
- https://raw.githubusercontent.com/sinedied/run-on-output/refs/heads/main/README.md
- https://raw.githubusercontent.com/sinedied/smoke/refs/heads/main/README.md

## Content Guidelines

- Use GitHub Flavored Markdown.
- Use GitHub admonition syntax where it adds real value.
- Keep the README concise and to the point.
- Prefer a strong header, short project description, quick links, feature or approach summary, setup/run instructions, usage examples, and project structure.
- Do not overuse emojis.
- Do not include sections already covered by dedicated files, such as `LICENSE`, `CONTRIBUTING`, `CHANGELOG`, or code of conduct sections.
- The README is user-facing documentation and MUST use Traditional Chinese (zh-TW).
- Document only verified commands, such as `dotnet build`, `dotnet run`, or `dotnet test`, when they work for the current repository state.

## Verification

Before finishing:

- Run the documented build, test, or execution command when practical.
- Confirm all documented commands and paths are correct.
- Confirm examples match the current implementation output.
- Confirm the README does not include unsupported claims or missing assets.
