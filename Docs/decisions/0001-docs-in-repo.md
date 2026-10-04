# 0001. Documentation is kept in the repository under `Docs/`

- **Status:** Accepted
- **Date:** 2026-10-04

## Context

Tasks are tracked in a GitHub Project, but a Project has no proper place for documentation: only a single README and task descriptions.

## Decision

Documentation is written in Markdown in the `Docs/` folder of the main repository. Diagrams are drawn in Mermaid. The GitHub Project README and tasks link to pages in `Docs/`.

## Alternatives

- **GitHub Wiki**: a separate git repository, not reviewed through PRs, not versioned with the code.
- **GitHub Project README**: a single page with no structure.
- **Notion or Google Docs**: far from the code, goes stale quickly.

## Consequences

- Docs change in the same commits and PRs as the code and get reviewed.
- AI assistants working with the repository can read the docs.
- Requires discipline: a behavior change comes with an update to `Docs/`.
