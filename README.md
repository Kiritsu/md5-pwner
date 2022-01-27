# Md5Pwner

[![Build and Run tests](https://github.com/Kiritsu/md5-pwner/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Kiritsu/md5-pwner/actions/workflows/dotnet.yml)
[![Docker Image CI](https://github.com/Kiritsu/md5-pwner/actions/workflows/docker-image.yml/badge.svg)](https://github.com/Kiritsu/md5-pwner/actions/workflows/docker-image.yml)

## How to use

### Clone

> git clone https://github.com/Kiritsu/md5-pwner

### Run with docker-compose

> docker-compose up -d --build

### Pre-add a few slaves

> docker-compose up -d --build --scale pwner=x

Where `x` is the amount of slaves to spawn. You can also scale it from the interface.

### Webinterface

The web interface is available in the port 80.
