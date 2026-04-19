# CLAUDE.md

Behavioral guidelines for Claude when working on this project.
**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

---

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them — don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

---

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

**Exception:** Flag (don't ignore) security issues, race conditions, or data integrity risks even if not in scope.

---

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it — don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

**The test:** Every changed line should trace directly to the user's request.

---

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria require constant clarification.

---

## 5. Response Efficiency

**Match response length to task complexity.**

- Trivial edit (1-2 lines): Show only the change
- Single function: Show function + 1 line context
- Full file rewrite: Only if user asked for it
- Explanations: Only when requested, or when decision needs justification

**Default to code. Explain only critical reasoning.**

When showing existing file changes:
- Prefer diff-style or "before → after" snippets
- Don't reprint the entire file unless asked

When responding conversationally:
- No filler ("Great question!", "Certainly!")
- No restating the user's request
- Get to the point in the first sentence

### Cost awareness
- Assume every message costs real money/limit for the user
- Don't re-upload context already in this conversation
- If unsure whether to show long code or short summary, default to short

---

## 6. Fail Fast on Assumptions

**When you don't know, say so. Don't fabricate.**

- Unknown API behavior → say "Not sure, would need to verify"
- Unfamiliar library version → ask or suggest checking docs
- Project conventions → ask, don't guess
- Database schema → request the file, don't imagine

False confidence wastes more time than asking.

### Preview reliability
- Visualizer previews render in sandboxed iframes — may differ from browser
- Never claim "preview matches final output" — verify code against spec instead
- For SVG: verify source order (rects before lines) before trusting preview

---

## 7. Project Context: KlangIT_V3

### Tech stack (don't change without asking)
- **Framework:** ASP.NET Core MVC (not Razor Pages, not Minimal API)
- **ORM:** Entity Framework Core
- **Database:** SQL Server (main data) + SQLite (Identity)
- **Frontend:** Bootstrap 5 + custom `ds-*` design system CSS
- **Locale:** Thai (th-TH) + Thai Buddhist calendar

### Domain
IT asset/equipment management system (คลัง IT) — tracks items, borrow/return history, departments, sections.

### Folder conventions
```
/Controllers        — 1 controller per entity, CRUD actions
/Models             — EF entities (partial classes)
/Models/Enums       — enums with [Display(Name=...)] in Thai
/ViewModels         — DTOs for views (see patterns below)
/Views/{Entity}     — Index, Details, Create, Edit, Delete per entity
/Views/Shared       — _Layout, _DesignSystem, _ItemStateDiagram
/Helpers            — Utility.GetCurrentUserName() etc.
/Validation         — custom validation attributes
```

---

## 8. Project Patterns (follow these)

### ViewModels
- **Edit actions** use dedicated ViewModels (`ItemEditViewModel`), NOT raw Model
- **Delete actions** use dedicated ViewModels (`ItemDeleteViewModel`) with dependency-check fields
- **Index actions** may use ViewBag for simple cases, ViewModel for complex (cascade dropdowns)
- **Create actions** use ViewModels with dropdown population

### Soft delete policy
- **Master data** (ItemType, ItemBrand, ItemModel, Department, Section) → soft delete (`IsDeleted = true`)
- **Items** → currently hard delete (see TODO BUG-001 — may change)
- **BorrowHistory** → hard delete
- When adding new entity, decide explicitly and document

### Audit fields
Every entity has: `CreatedDate`, `ModifiedDate`, `CreatedBy`, `ModifiedBy`, `IsDeleted`
- Populate via `Utility.GetCurrentUserName()` — not inline code
- Set both `CreatedDate/By` and `ModifiedDate/By` on insert
- Update only `ModifiedDate/By` on update

### Dependency check before delete
Controllers must check related data before hard delete:
```csharp
if (entity.ChildEntities.Any(c => !c.IsDeleted))
    return RedirectToAction(nameof(Delete), new { id });
```

### Cascade dropdowns
- ItemType ↔ ItemBrand (many-to-many via ItemTypeToBrand)
- ItemBrand → ItemModel (one-to-many)
- Built client-side from JSON maps in ViewModel — no AJAX

---

## 9. Design System Rules

### CSS class naming
- Prefix all custom classes with `ds-` (design system)
- Bootstrap classes allowed alongside
- Put styles in `/Views/Shared/_DesignSystem.cshtml`

### Required classes
- `ds-card`, `ds-form-row`, `ds-btn-primary`, `ds-btn-ghost`, `ds-btn-danger`
- `ds-input`, `ds-select`, `ds-input-ro` (readonly)
- `ds-table-wrap`, `ds-filter-bar`

### Status badges — limited use
- **Removed from**: table/list views (Index pages)
- **Still used in**: form status displays (Create/Edit/Return) as read-only indicators
- When user asks to remove badges, check scope (all pages? or just lists?)
- Plain text alternative for tables:
```razor
@(Model.IsReturn ? "คืนแล้ว" : "กำลังยืม")
```

---

## 10. Language Rules

### Thai (use for)
- UI labels, button text, form field labels
- Validation messages
- Page titles, section headers
- Enum `[Display(Name=...)]`
- Business logic comments where helpful

### English (use for)
- Class names, method names, variable names
- File names
- XML doc comments (`/// <summary>`)
- Code comments describing technical logic
- Error messages in logs (not user-facing)

### Don't translate
- Existing Thai UI strings (keep as-is unless asked)
- Technical terms (controller, ViewModel, migration) when writing code

---

## 11. What NOT to do

- ❌ Don't rename existing classes/methods to "improve naming"
- ❌ Don't add `[Authorize]` without asking (project doesn't use it yet)
- ❌ Don't introduce new libraries without asking (project uses: EF Core, Bootstrap, jQuery validation, Newtonsoft.Json)
- ❌ Don't convert to async if method is already sync and works
- ❌ Don't add logging/telemetry unless requested
- ❌ Don't change connection strings or `appsettings.json` structure
- ❌ Don't modify `_Layout.cshtml` unless asked
- ❌ Don't add new routes unless asked

---

## 12. When to ask vs when to proceed

### Proceed without asking
- Fixing obvious typos in code
- Following exact pattern from existing file (explicitly referenced)
- Adding missing `using` statements
- Applying user-requested changes verbatim

### Ask first
- Adding new NuGet package
- Creating new entity / table / migration
- Changing existing method signature
- Adding new controller / route
- Touching security (auth, validation)
- Touching data (migrations, seed)
- Interpretation of user request is ambiguous

### Always announce before doing
- Changing more than 3 files
- Any database migration
- Refactoring shared code (_Layout, _DesignSystem, Utility)
- Creating or editing SVG diagrams → draw all `<rect>` first, then all `<path>`/`<line>`
  (SVG paints in source order — lines drawn before boxes get painted over)

---

## 13. Output format preferences

### For code changes
```
File: Controllers/ItemsController.cs
Change: [1-line summary]

[code diff or snippet]
```

### For multi-file changes
List files first, then show changes per file:
```
Files changed:
1. Controllers/ItemsController.cs — added dependency check
2. Views/Items/Delete.cshtml — updated error UI

[details per file]
```

### For explanations
- Bullet points, not paragraphs
- Lead with conclusion, then reasoning
- Maximum 5 bullets unless asked for more

---

## 14. Current project status

- **Last updated**: 2026-04-19
- **Current phase**: Post-redesign, integrating ViewModels
- **Active focus**: BUG-001 (return amount), badge cleanup, pagination planning

See `TODO.md` at project root for:
- Known bugs (BUG-XXX)
- Missing features (FEAT-XXX)
- Tech debt (TECH-XXX)
- UX improvements (UX-XXX)

When user references a task ID (e.g. "do BUG-001"), look up TODO.md for full context.

---

**These guidelines are working if:**
- Fewer unnecessary changes in diffs
- Fewer rewrites due to overcomplication
- Clarifying questions come before implementation rather than after mistakes
- Response length matches task complexity