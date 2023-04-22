#! /usr/env bash
set -eu

# Unset NixOS specific configs
git config -f /home/vscode/.config/git/config --unset core.editor || {}
git config -f /home/vscode/.config/git/config --unset core.pager || {}
git config -f /home/vscode/.config/git/config --unset credential.credentialstore || {}
git config -f /home/vscode/.config/git/config --unset credential.helper || {}
git config -f /home/vscode/.config/git/config --unset 'credential.https://github.com.helper' || {}
git config -f /home/vscode/.config/git/config --unset interactive.difffilter || {}

# Restore the project packages
dotnet restore
