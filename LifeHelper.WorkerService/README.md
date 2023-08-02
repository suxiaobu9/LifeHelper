# ReadMe

## docker compose

```yml
version: "1.0"
name: lifehelper-worker-service
services:
  seq:
    image: datalust/seq:2023.2
    container_name: seq
    restart: unless-stopped
    environment:
      - ACCEPT_EULA=Y
    ports:
      - 5341:80
    volumes:
      - ./data/seq:/data
    networks:
      seq_log_network:
        ipv4_address: 172.30.0.2

  lifehelper:
    image: arisuokay/lifehelper-worker-service:latest
    container_name: lifehelper-worker
    restart: unless-stopped
    environment:
      - SeqLogServerAddress=http://172.30.0.2:5341/
      - Duc__UserName=
      - Duc__Password=
      - Duc__ZoneId=
      - Duc__HostNames=
      - Duc__DelayMinutes=5
      - WakeUp__HostAddress=
      - WakeUp__DelayMinutes=10
    networks:
      seq_log_network:
        ipv4_address: 172.30.0.3

networks:
  seq_log_network:
    ipam:
      config:
        - subnet: 172.30.0.0/16
```

## build 指令

```ps1
docker build -t arisuokay/lifehelper-worker-service .
# 需要變更版本
docker tag arisuokay/lifehelper-worker-service:latest arisuokay/lifehelper-worker-service:v1.1

docker push arisuokay/lifehelper-worker-service:latest
# 需要變更版本
docker push arisuokay/lifehelper-worker-service:v1.1

```
