#! /bin/bash
set -xeuo

INSTALL_SCRIPT=/tmp/dotnet-install.sh
INSTALL_PATH=/usr/local/dotnet/current
INSTALL_SCRIPT_HASH="5840ce64f4186ccc4dac0c0fd8703acd0d387091ce48f310fef758e5f84d7a7f  $INSTALL_SCRIPT"

apt-get update -y
apt-get -y install --no-install-recommends wget ca-certificates icu-devtools

wget -O $INSTALL_SCRIPT "https://dot.net/v1/dotnet-install.sh"
sha256sum --check <<< "$INSTALL_SCRIPT_HASH"
chmod +x $INSTALL_SCRIPT

$INSTALL_SCRIPT --install-dir $INSTALL_PATH --channel 6.0 --version latest
$INSTALL_SCRIPT --install-dir $INSTALL_PATH --channel 7.0 --version latest
$INSTALL_SCRIPT --install-dir $INSTALL_PATH --channel 8.0 --version latest

rm $INSTALL_SCRIPT