# Docker Build and Run

docker build --tag goblin-blogcrawler:1.0 .

docker run --network bridge --publish 8004:80 --env-file DockerEnv --detach --name goblin-blogcrawler goblin-blogcrawler:1.0

---

# Docker Remove

docker rm --force goblin-blogcrawler

---

# Network

docker network ls

docker network create -d bridge goblin

docker network inspect goblin

docker network rm goblin