# Prick It and wake line chatbot up

## docker

```bash
docker run --name wake-it-up -e WAKEITUP_ADDRESS='https://domain.com/api/WakeUp' -e DELAY_MIN='10' -v /host/dir:/app/data arisuokay/wake-it-up:latest
```

## docker compose

```yml
version: "1"
name: wake-it-up
services:
  wake-it-up:
    image: arisuokay/wake-it-up:latest
    container_name: wake-it-up
    environment:
      - WAKEITUP_ADDRESS=https://domain.com/api/WakeUp
      - DELAY_MIN=10
    volumes:
      - ./data/WakeItUp:/app/data
```

## build 指令

```ps1
docker build -t arisuokay/wake-it-up .
# 需要變更版本
docker tag arisuokay/wake-it-up:latest arisuokay/wake-it-up:v1.0

docker push arisuokay/wake-it-up:latest
# 需要變更版本
docker push arisuokay/wake-it-up:v1.0

```
