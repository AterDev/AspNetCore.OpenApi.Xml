# Documentation Overview

This file provides a guide to all the documentation available in the AspNetCore.OpenApi.Xml project.

## For Users and Contributors

### README.md (Root)
**Purpose**: Main project documentation for users
**Contents**:
- Tech stack and system requirements
- Development environment setup
- Build and test instructions
- Project structure overview
- Quick start guide
- Contribution guidelines
- FAQ and roadmap

**When to read**: When you first encounter the project or need to understand how to use it

### PROJECT_SUMMARY.md (Root)
**Purpose**: Architectural overview and design decisions
**Contents**:
- Project structure and organization
- Core models and their relationships
- Generation logic and strategies
- Design tradeoffs and rationale
- Potential improvements
- Quick reference table

**When to read**: When you need to understand the architecture and design philosophy

## For AI Assistants

### .github/copilot-instructions.md
**Purpose**: GitHub Copilot guidance for code generation
**Audience**: GitHub Copilot (inline suggestions and chat)
**Contents**:
- Project overview and architecture
- Code conventions and patterns
- Common tasks and workflows
- Performance considerations
- Pitfalls to avoid
- File organization guide

**When used**: Automatically by GitHub Copilot when working in this repository

### .github/agents/coding-agent.md
**Purpose**: Comprehensive guide for AI coding agents
**Audience**: GitHub Copilot Coding Agent and similar AI agents
**Contents**:
- Quick start context
- Essential coding patterns
- Task-specific templates
- Type handling reference
- DataAnnotations extraction
- Testing guidelines
- Common mistakes to avoid
- Decision trees for feature additions

**When used**: By coding agents when making code changes

### .github/agents/development-workflows.md
**Purpose**: Step-by-step workflows for common development tasks
**Audience**: Both human developers and AI agents
**Contents**:
- Setup and environment configuration
- Workflow 1: Adding DataAnnotation support
- Workflow 2: Adding complex type support
- Workflow 3: Customizing HTML documentation
- Workflow 4: Adding extension methods
- Workflow 5: Debugging issues
- Workflow 6: Adding test controllers
- Build and release workflows
- Troubleshooting guide

**When used**: When performing specific development tasks

### .github/agents/quick-reference.md
**Purpose**: Fast lookup guide for common patterns and locations
**Audience**: AI agents and developers needing quick answers
**Contents**:
- Project at-a-glance
- File structure map
- Key classes and interfaces
- Common code patterns
- Type system reference
- DataAnnotations mapping
- API usage examples
- Build commands
- Important constants

**When used**: When you need to quickly look up a pattern, location, or example

## Documentation Structure

```
AspNetCore.OpenApi.Xml/
├── README.md                           # User documentation
├── PROJECT_SUMMARY.md                  # Architectural overview
├── LICENSE                             # MIT License
└── .github/
    ├── copilot-instructions.md        # GitHub Copilot guidance
    └── agents/
        ├── coding-agent.md            # AI coding agent instructions
        ├── development-workflows.md   # Step-by-step workflows
        └── quick-reference.md         # Quick lookup guide
```

## How to Use This Documentation

### As a New User
1. Start with **README.md** for setup and usage
2. Read **PROJECT_SUMMARY.md** to understand the architecture
3. Use **.github/agents/quick-reference.md** for quick lookups

### As a Contributor
1. Read **README.md** for contribution guidelines
2. Study **PROJECT_SUMMARY.md** for design philosophy
3. Follow workflows in **.github/agents/development-workflows.md**
4. Reference **.github/agents/coding-agent.md** for coding patterns
5. Use **.github/agents/quick-reference.md** for quick lookups

### As an AI Agent
1. Read **.github/copilot-instructions.md** for general guidance
2. Use **.github/agents/coding-agent.md** as your primary reference
3. Follow **.github/agents/development-workflows.md** for specific tasks
4. Consult **.github/agents/quick-reference.md** for fast lookups
5. Refer to **PROJECT_SUMMARY.md** for architectural context

## Documentation Principles

1. **Accuracy**: All documentation reflects the actual codebase
2. **Completeness**: Cover common scenarios and edge cases
3. **Clarity**: Use examples and clear explanations
4. **Maintainability**: Update documentation when code changes
5. **Accessibility**: Multiple formats for different audiences

## Keeping Documentation Updated

When making significant changes to the codebase:

- [ ] Update **README.md** if user-facing features change
- [ ] Update **PROJECT_SUMMARY.md** if architecture changes
- [ ] Update **.github/copilot-instructions.md** if coding patterns change
- [ ] Update **.github/agents/coding-agent.md** if new patterns are introduced
- [ ] Update **.github/agents/development-workflows.md** if workflows change
- [ ] Update **.github/agents/quick-reference.md** if constants or common patterns change

## Feedback and Improvements

If you find documentation that is:
- Outdated or incorrect
- Unclear or confusing
- Missing important information
- Could be improved

Please open an issue or submit a pull request to improve it.

---

**Last Updated**: Documentation created as part of AI description documentation initiative
**Maintainers**: Project contributors
**License**: Same as project (MIT)
