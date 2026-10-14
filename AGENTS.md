# Agent rules

- **Working files live in `.cache/`.** Plans, spikes, handoffs, reviews, research and AI context docs go under
  `.cache/` (for example `.cache/docs/`, `.cache/plans/`), never elsewhere in the repo. `.cache/` is git-ignored;
  keep it that way.
- **Never commit unless the user explicitly says to.** Don't offer or suggest commits either.
- **Human-only sections are off-limits.** In any Markdown file, never edit anything between
  `<!-- human-only-edit-section-start -->` and `<!-- human-only-edit-section-end -->`, under any circumstances:
  no rewording, reformatting, moving or deleting, and leave the markers themselves untouched. When editing a file that
  contains such a section, change only the text outside it.

Start a session by reading `.cache/docs/handoff.md`.
