FROM mcr.microsoft.com/devcontainers/base:ubuntu

COPY ./install-dotnet.sh /tmp/install-dotnet.sh

RUN chmod +x /tmp/install-dotnet.sh \
    && /tmp/install-dotnet.sh \
    && rm /tmp/install-dotnet.sh

ENV PATH $PATH:/usr/local/dotnet/current