# Md5Pwner

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
