# TestResults policy

Unity Test Runner XML files are generated artifacts and are not versioned. Run commands may write
them into this folder locally or upload them as CI artifacts.

Versioned evidence must be compact and reviewable:

- architecture/dependency snapshots as `.txt`;
- execution summaries as `.md`;
- exact command, exit code, total/passed/failed and relevant log path;
- no full NUnit XML in commits.

Historical documentation may name an XML artifact that is reproduced locally by its documented
command. Removing XML from the Git index does not change the validation result it recorded.
