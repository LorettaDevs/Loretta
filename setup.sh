#! /usr/bin/env bash
set -euo pipefail

# Unset NixOS specific configs
git config -f /home/vscode/.config/git/config --unset core.editor || true
git config -f /home/vscode/.config/git/config --unset core.pager || true
git config -f /home/vscode/.config/git/config --unset credential.credentialstore || true
git config -f /home/vscode/.config/git/config --unset credential.helper || true
git config -f /home/vscode/.config/git/config --unset 'credential.https://github.com.helper' || true
git config -f /home/vscode/.config/git/config --unset interactive.difffilter || true

# Restore the project packages
dotnet restore
