# Rule: No Parallel Unity Batchmode

Do not run multiple Unity batchmode processes for the same project at the same time.

## Reason

Unity cannot safely open the same project in multiple instances. Parallel batchmode runs can fail with editor locks or corrupt validation evidence.

## Required Pattern

- Run Unity generators and validators sequentially.
- Use separate log files per command.
- Wait for the process to exit before starting the next Unity command.

## If Unity Is Already Open

Either:

- close the existing Unity instance if authorized; or
- record Unity validation/generation as blocked.

Do not launch additional Unity batchmode commands hoping one succeeds.
