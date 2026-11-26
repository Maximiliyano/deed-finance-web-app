# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

The primary user is the author, using Deed Finance as a personal tool for their own daily money tracking. Their money is UAH-based and spread across several accounts and currencies (UAH, USD, EUR, PLN, CZK). They check it from both phone and desktop, often in short sessions: log an expense, move money between accounts, see where the month stands.

Anonymous visitors can try most features through the demo mode, but they are not the design target.

## Product Purpose

Deed Finance tracks money across multiple accounts ("capitals") and currencies, and ties the everyday records (expenses, incomes, transfers) to planning (budget estimations, goals, debts, recurring utility bills). Success is that one glance at the home page answers "how much do I have, what is coming due, and am I on plan", without opening a spreadsheet.

## Positioning

Multi-currency is the base case, not an add-on: accounts, transfers and totals work natively across UAH, USD, EUR, PLN and CZK, converted with daily PrivatBank rates. Everything lives on a single dashboard (capitals, expenses, incomes, budget planner, goals, debts, utility bills) instead of separate apps. It integrates with Ukrainian banks (PrivatBank rates, Monobank account and jar details) and sends scheduled e-mail reminders on user-defined cron times.

## Operating Context

- Money is entered by hand; there is no automatic bank sync of transactions.
- Exchange rates are fetched daily from the PrivatBank public API and cached.
- Balances change when expenses, incomes, transfers and bill payments are saved; a capital with references cannot be deleted.
- Reminders (balance summary, expense logging, unpaid debts) run as background jobs for users who configured them.

## Capabilities and Constraints

- Capitals with currency, "include in total" and "only for savings" flags, and manual ordering.
- Expenses and incomes with categories (global, shared) and free-form tags; cross-currency transfers carry both a source and a destination amount.
- Budget planner, goals, debts and utility bills with drag-and-drop ordering where it applies. Utility bills repeat monthly on a due day; paying one creates a linked expense.
- Anonymous demo mode is open: most endpoints work without sign-in, limited to 3 created items per entity; signing in (Auth0) removes the limit.
- Mobile is first-class: every flow must work under 576px (bottom-sheet dialogs, touch targets, stacked tables).
- Both dark and light themes are required; dark is the default and the choice is remembered.
- Currency totals are computed on the server, not in the frontend.

## Brand Commitments

The product name is "Deed Finance" ("Deed" in the UI). No other brand assets or voice rules have been declared as binding.

## Evidence on Hand

No testimonials, customer logos, benchmarks or pricing exist. The README feature table is the source of truth for what the product does. Do not invent social proof or metrics.

## Product Principles

1. Multi-currency is never a special case: every amount shows its currency, and totals state which currency they are in.
2. The home page answers the money question at a glance; detail and edit flows stay one step away.
3. Recording a transaction must be fast on a phone, in a few taps.
4. Dates, due days and statuses are explained in plain words, so that nobody has to guess what a number means.
5. Demo visitors can try the product without signing in, and the limits are visible rather than surprising.
