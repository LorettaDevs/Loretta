# Nightly Tool

This is a tool to help with getting the version number for the next nightly.

It reads the version from `LibraryProject.props` and then compares it with the latest stable release tag from the git repo information and then checks the modified files for each commit since then to actually generate the version number.

For files that will be ignored for versioning purposes by this tool and therefore will not contribute to a new nightly release, check `IsFileSignificant` in `Program.cs`.

Commits that have `[nightly skip]`, `[nightly ignore]`, `[skip nightly]` or `[ignore nightly]` anywhere in the message will also be ignored.

### Testing

To test this tool, I recommend running this command from the project root:

```console
$ dotnet run --project src/Tools/NightlyTool -- $TTY
```

This will make it output the output parameters that are sent to `$GITHUB_OUTPUT` to the current TTY which is easier than opening a random file to check.