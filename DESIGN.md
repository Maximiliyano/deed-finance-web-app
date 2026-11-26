---
name: Deed Finance
description: A dark-first, glanceable personal-finance dashboard in tonal navy, with money states carried by green, red and amber and sections tagged by quiet accent hues.
colors:
  primary: "#4f8cf7"
  primary-hover: "#2d6ee0"
  primary-tint: "#1a3660"
  on-primary: "#ffffff"
  night-canvas: "#07090f"
  night-surface: "#0d1526"
  night-surface-raised: "#111e38"
  hairline: "#1c2e50"
  hairline-strong: "#274068"
  ink: "#e2e8f5"
  ink-muted: "#6a8ab0"
  ink-faint: "#344e6e"
  income-green: "#22c55e"
  income-green-text: "#4ade80"
  income-green-tint: "#061710"
  expense-red: "#ef4444"
  expense-red-text: "#f87171"
  expense-red-tint: "#1a0808"
  due-amber: "#f59e0b"
  due-amber-text: "#fbbf24"
  due-amber-tint: "#1a1200"
  info-sky: "#38bdf8"
  info-sky-text: "#7dd3fc"
  info-sky-tint: "#061a26"
  accent-violet: "#a78bfa"
  accent-violet-text: "#c4b5fd"
  accent-violet-tint: "#140f2b"
  accent-teal: "#2dd4bf"
  accent-teal-text: "#5eead4"
  accent-teal-tint: "#051c1d"
  accent-pink: "#f472b6"
  accent-pink-text: "#f9a8d4"
  accent-pink-tint: "#240c1a"
  accent-orange: "#fb923c"
  accent-orange-text: "#fdba74"
  accent-orange-tint: "#241208"
typography:
  headline:
    fontFamily: "Inter, Roboto, Helvetica Neue, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 700
    letterSpacing: "-0.01em"
  title:
    fontFamily: "Inter, Roboto, Helvetica Neue, sans-serif"
    fontSize: "0.9375rem"
    fontWeight: 600
  stat-value:
    fontFamily: "Inter, Roboto, Helvetica Neue, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 700
    lineHeight: 1.2
    fontFeature: "tnum"
  body:
    fontFamily: "Inter, Roboto, Helvetica Neue, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
  label:
    fontFamily: "Inter, Roboto, Helvetica Neue, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 600
    letterSpacing: "0.05em"
rounded:
  sm: "6px"
  md: "10px"
  lg: "14px"
  full: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "20px"
  xxl: "24px"
  xxxl: "32px"
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.on-primary}"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-primary-hover:
    backgroundColor: "{colors.primary-hover}"
  button-secondary:
    backgroundColor: "{colors.night-surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.ink-muted}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-danger:
    backgroundColor: "{colors.expense-red}"
    textColor: "{colors.on-primary}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  card:
    backgroundColor: "{colors.night-surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
  card-header:
    backgroundColor: "{colors.night-surface-raised}"
    typography: "{typography.title}"
    padding: "12px 20px"
  input:
    backgroundColor: "{colors.night-surface-raised}"
    textColor: "{colors.ink}"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "8px 12px"
  stat-tile:
    backgroundColor: "{colors.night-surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.lg}"
    padding: "16px"
  badge-neutral:
    backgroundColor: "{colors.night-surface-raised}"
    textColor: "{colors.ink-muted}"
    typography: "{typography.label}"
    rounded: "{rounded.full}"
    padding: "3px 9px"
  badge-success:
    backgroundColor: "{colors.income-green-tint}"
    textColor: "{colors.income-green-text}"
    rounded: "{rounded.full}"
    padding: "3px 9px"
  badge-danger:
    backgroundColor: "{colors.expense-red-tint}"
    textColor: "{colors.expense-red-text}"
    rounded: "{rounded.full}"
    padding: "3px 9px"
  badge-warning:
    backgroundColor: "{colors.due-amber-tint}"
    textColor: "{colors.due-amber-text}"
    rounded: "{rounded.full}"
    padding: "3px 9px"
  icon-chip:
    backgroundColor: "{colors.primary-tint}"
    textColor: "{colors.primary}"
    rounded: "{rounded.md}"
    size: "28px"
  segmented-button-active:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.on-primary}"
    rounded: "{rounded.md}"
    padding: "6px 12px"
---

# Design System: Deed Finance

## Overview

**Creative North Star: "The Pocket Dashboard"**

Deed is a personal money tool checked in short sessions, usually on a phone. The system is built so that one look answers "how much do I have, what is coming due, am I on plan". The page is a stack of flat panels in tonal navy, each led by a small colored icon chip and a clear title, with the numbers doing the talking. Chrome stays out of the way; hue is reserved for meaning.

The default is the dark theme (deep navy, not black), with a light theme that mirrors every token. Density is compact but never cramped: 14px body text, a 4px spacing base, and tabular numerals so amounts line up in every column. On touch devices the same layout grows to 44px targets and 16px inputs rather than switching to a different design.

Color works in two layers. Green, red and amber keep their money meaning (income, expense, due and warning) and are never used for decoration. Sky, violet, teal, pink and orange are category tags: they identify a section or a chip and never signal a state.

**Key Characteristics:**
- Flat and tonal: depth comes from stepping between two navy surfaces and 1px hairlines, not from heavy shadow.
- Mobile-first behavior: dialogs become bottom sheets, wide tables become stacked cards, grids collapse to one column.
- Numbers first: amounts are bold, tabular and aligned; labels are small, uppercase and muted.
- One quiet accent for action (blue), many quiet tints for categories.
- Both themes are first-class; every colored surface has a tint, a fill and a text variant.

## Colors

A navy-tonal palette with one action blue, three money-state colors, and five category accents. Values below are the dark theme; the light theme overrides each token (see Light theme).

### Primary
- **Ledger Blue** (#4f8cf7): the single action color. Primary buttons, active segmented options, active nav item, focus ring, scrollbar hover, links to action. Hover deepens to **Pressed Blue** (#2d6ee0). Its soft background is **Blue Wash** (#1a3660), used for active nav and the primary icon chip.

### Neutral
- **Night Canvas** (#07090f): the page background behind every panel.
- **Night Surface** (#0d1526): cards, header, modals, secondary buttons.
- **Raised Navy** (#111e38): card headers, inputs, toolbars, stat tiles in dialogs, hover fill. One step above the surface.
- **Hairline** (#1c2e50): 1px borders and dividers. **Strong Hairline** (#274068) for inputs and control outlines.
- **Ink** (#e2e8f5): primary text. **Muted Ink** (#6a8ab0): labels, meta, placeholders. **Faint Ink** (#344e6e): hints and disabled-looking helper text.

### Money states
- **Income Green** (#22c55e, text #4ade80, tint #061710): income amounts, paid state, success badges, "profit".
- **Expense Red** (#ef4444, text #f87171, tint #1a0808): expense amounts, overdue, destructive actions, deficit.
- **Due Amber** (#f59e0b, text #fbbf24, tint #1a1200): due soon, warnings, bills to pay.

### Category accents
- **Sky** (#38bdf8, text #7dd3fc, tint #061a26): informational, upcoming, scheduling hints.
- **Violet** (#a78bfa, text #c4b5fd, tint #140f2b): budget planner, profit tile.
- **Teal** (#2dd4bf, text #5eead4, tint #051c1d): goals.
- **Pink** (#f472b6, text #f9a8d4, tint #240c1a): reserved tag hue, not yet bound to a section.
- **Orange** (#fb923c, text #fdba74, tint #241208): debts.

### Light theme
The light theme (set by `data-theme="light"`) keeps every role and swaps values: canvas #f1f5f9, surface #ffffff, raised #f8fafc, hairline #e2e8f0 / #cbd5e1, ink #0f172a, muted #64748b, faint #94a3b8; action blue #2563eb (hover #1d4ed8, wash #dbeafe). Money states: green #16a34a / text #15803d / tint #f0fdf4; red #dc2626 / #b91c1c / #fef2f2; amber #d97706 / #92400e / #fffbeb. Accents: sky #0284c7 / #0369a1 / #e0f2fe; violet #7c3aed / #6d28d9 / #ede9fe; teal #0d9488 / #0f766e / #ccfbf1; pink #db2777 / #be185d / #fce7f3; orange #ea580c / #c2410c / #ffedd5.

### Named Rules
**The Meaning-First Hue Rule.** Green, red and amber only ever say something about money or time (income, expense, due). Sky, violet, teal, pink and orange tag sections and chips; they never appear on a status.

**The Text-Variant Rule.** Colored text on a tinted background uses the `-text` variant, never the fill. The fill color is for bars, dots, icons and borders. This keeps contrast safe in both themes.

**The Both-Themes Rule.** A new color is not done until it has a tint, a fill and a text value in dark and light.

## Typography

**Display Font:** Inter (with Roboto, Helvetica Neue, sans-serif)
**Body Font:** Inter (same stack)
**Label/Mono Font:** Inter; numerals use tabular figures

**Character:** A single neutral sans, differentiated only by weight, size and case. The personality comes from numbers set large and bold against small uppercase labels.

### Hierarchy
- **Headline** (700, 1.5rem, tight tracking -0.01em): page titles.
- **Stat Value** (700, 1.25rem, line-height 1.2, tabular): the big amount on a stat tile.
- **Title** (600, 0.9375rem): card and dialog headers, next to their icon chip.
- **Body** (400-500, 0.875rem): table cells, form text, buttons (500).
- **Small** (0.8125rem and 0.75rem): secondary lines, hints, meta under a name.
- **Label** (600, 0.75rem, uppercase, +0.04-0.05em tracking, muted): stat labels and table column headers.

### Named Rules
**The Tabular Rule.** Every amount, date and count uses tabular numerals so columns align and values do not jitter when they change.

**The One-Family Rule.** Inter is the only family. Hierarchy is made with weight, size and case, never a second typeface.

## Layout

A single centered column up to 1200px (page max), padded 16px on phones and 24px from 576px up. The fixed header is 60px tall plus the safe-area inset, with its inner row capped at 1400px; the main nav is hidden below 1200px. Spacing runs on a 4px base (4, 8, 12, 16, 20, 24, 32) and panels are separated by 12 to 16px.

The home page is a dashboard: a KPI row (1 column on phones, 2 from 470px, 4 from 992px) followed by stacked panels that lazy-render as they scroll into view. Inside panels, tables and lists wrap in a scroll container; below 576px tables turn into stacked cards where each cell shows its own label.

Breakpoints are 320, 470, 576, 768, 992 and 1200px. Below 576px dialogs become bottom sheets with a grabber and respect the bottom safe area. On touch devices (`pointer: coarse`) buttons, icon buttons and segmented options grow to 44px and inputs use 16px text so the browser does not zoom.

### Named Rules
**The Thumb Rule.** On touch, every tappable control is at least 44px; on a mouse pointer it can drop to 28-32px.

**The Stack-Don't-Scroll Rule.** Data that cannot fit a phone width is restacked into labeled rows before it is allowed to scroll sideways.

## Elevation & Depth

Flat and tonal. Depth is conveyed by stepping between Night Surface and Raised Navy and by 1px hairlines. Shadows are small and structural, applied only to the fixed header, cards, dropdowns and modals; they are never a hover effect. In the dark theme shadows are heavy and black-based so they read against the navy; in the light theme they soften to 4-12% black.

### Shadow Vocabulary
- **Header** (`box-shadow: 0 1px 3px rgba(0,0,0,0.7)` dark, `0 1px 3px rgba(0,0,0,0.08)` light): the fixed header edge.
- **Card** (`0 2px 8px rgba(0,0,0,0.6), 0 1px 3px rgba(0,0,0,0.5)` dark): panels and stat tiles.
- **Raised** (`0 4px 16px rgba(0,0,0,0.7), 0 2px 6px rgba(0,0,0,0.5)` dark): dropdown menus.
- **Modal** (`0 12px 32px rgba(0,0,0,0.8), 0 0 0 1px rgba(28,46,80,0.5)` dark): dialogs, over a 60% black backdrop.
- **Focus ring** (`0 0 0 3px` in action blue at 22% alpha): the only interactive shadow, on inputs and search fields.

### Named Rules
**The Flat-By-Default Rule.** At rest, surfaces are separated by tone and hairline. A shadow means "this floats above the page" (header, dropdown, modal), not "this is hovered".

## Shapes

Gently rounded and consistent. Controls use 10px, panels 14px, small chips and drag handles 6px, and pills and progress bars are fully round. Borders are always 1px hairlines; the one exception is the 3px colored top edge on dashboard stat tiles, which tags the tile's meaning. Icon chips are 28px rounded squares (10px), never circles, so they read as a tag, not an avatar.

## Components

### Buttons
Compact and restrained.
- **Shape:** 10px radius, 8px by 16px padding (5px by 12px for the small variant), 14px medium text, optional leading icon at 0.875em.
- **Primary:** Ledger Blue fill with white text; hover deepens to Pressed Blue.
- **Secondary:** Night Surface fill, Ink text, Strong Hairline border; hover raises to Raised Navy and the border turns blue.
- **Ghost:** transparent, Muted Ink text; hover fills Raised Navy.
- **Danger / Success:** solid Expense Red or Income Green with white text; hover swaps to the lighter text variant.
- **Disabled:** 45% opacity, no pointer.

### Icon buttons
28px square, 10px radius, transparent with a Muted Ink icon; hover fills Raised Navy. The destructive variant hovers to Expense Red on its tint. 44px on touch.

### Cards / Containers
- **Corner Style:** 14px, clipped.
- **Background:** Night Surface with a Hairline border.
- **Header:** Raised Navy strip, 12px by 20px padding, a bottom hairline, title 15px semibold led by an icon chip; actions sit at the right.
- **Shadow Strategy:** the Card shadow only.

### Stat tiles
Surface fill, 14px radius, 16px padding, and a 3px top edge in the tile's meaning color (blue capital, green income, red expense, violet profit or amber deficit, sky break-even). A 28px icon chip sits beside a small uppercase label; the amount is large, bold and tabular, with an optional muted secondary-currency line.

### Icon chips
28px rounded squares with a tint background and the same hue's text color for the glyph. Variants: primary, success, danger, warning, info, accent, teal, pink, orange. They lead panel titles and KPI labels.

### Badges
Pill, 12px semibold, 3px by 9px padding, an optional leading icon. Tint background with the matching `-text` color for success, danger, warning, info, accent, teal and orange; neutral is Raised Navy with a hairline and Muted Ink.

### Inputs / Fields
- **Style:** Raised Navy fill, 1px Strong Hairline, 10px radius, 8px by 12px padding, 14px text (16px on touch).
- **Focus:** border turns Ledger Blue with a 3px soft blue ring.
- **Disabled:** fills with the surface color and mutes the text. Field errors appear as small Expense Red text below the field.

### Segmented options
A wrapping row of outlined 10px controls with tabular text; the selected one fills Ledger Blue with white text. Used for period and filter choices.

### Tables
Full width, 14px, with a Raised Navy header row of small uppercase muted labels and hairline row separators. Numeric columns use tabular figures. Under 576px each row becomes a stacked card with per-cell labels.

### Progress and tone
A 6px fully rounded track in Hairline with a bar colored by tone (success, warning, danger, primary, info, accent, teal, orange).

### Navigation
A fixed 60px header on Night Surface with a hairline and header shadow. The logo is 28px with the "Deed" wordmark at 18px bold and -0.02em tracking (hidden under 470px). Nav items are 14px medium Muted Ink with a small icon; hover fills Raised Navy, and the active item sits on Blue Wash in Ledger Blue. Dropdown menus use Night Surface, a hairline and the Raised shadow. The inline nav hides under 1200px.

### Dialogs
480px max, 14px radius, Modal shadow, a Raised Navy header and footer around a 20px-padded body with 16px field gaps. Under 576px it becomes a bottom sheet with top-only rounding, a grabber and safe-area padding.

## Do's and Don'ts

### Do:
- **Do** use green, red and amber only for money and time states, and the five category accents only to tag sections.
- **Do** pair every tinted surface with its `-text` color, and define dark and light values for any new color.
- **Do** set amounts, dates and counts in tabular numerals.
- **Do** build depth from the surface and raised-navy steps plus 1px hairlines; reserve shadow for header, dropdowns and modals.
- **Do** explain statuses in plain words with a date or count ("Dec 1 · in 5 days", "25 days late") rather than a bare number.
- **Do** keep touch targets at 44px and form text at 16px on `pointer: coarse`.
- **Do** open bottom sheets and stacked tables on phones instead of shrinking desktop layouts.

### Don't:
- **Don't** use a category accent (sky, violet, teal, pink, orange) to communicate a status or an amount's sign.
- **Don't** introduce a second typeface; build hierarchy with weight, size and case.
- **Don't** add hover shadows or heavy elevation to cards and buttons.
- **Don't** use a pure-black background; the canvas is deep navy (#07090f).
- **Don't** use the fill color for text on a tint; use the `-text` variant.
- **Don't** let a table scroll sideways on a phone before restacking its rows.
