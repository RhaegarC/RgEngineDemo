---
name: doc-assert
description: Check the documentation's invariants — links, single-home facts, indexes, status markers
disable-model-invocation: true
---

Run the repository's documentation checker from the repository root. It is commonly
`scripts/doc-assert.py`; if the repository has no such script, say so rather than inventing one.

The checker should be stdlib-only and need no network, no build and no database. Every check it runs
should exist because the defect it catches has already happened — a broken link that reached
`develop`, a test count written into five documents and stale in four, a checklist requiring a CI job
that does not exist, a command missing from the README's list.

**A failure is a finding, not a formatting nit.** Fix the document the check names; do not weaken the
check to make it pass. If a check is genuinely wrong — it flags something that is not drift — say so
and change the check deliberately, in its own commit, rather than deleting the rule.

If it reports nothing, say exactly that. Do not re-run it to be sure, and do not report a check that
was skipped as one that passed.
