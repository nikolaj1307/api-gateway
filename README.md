# API Gateway

A standalone Ocelot gateway for the System Integration API gateway exercises.

**Information:** This Project should be used with ```ChannelService``` from the ```bizcord``` project. The project can be found here: https://github.com/nikolaj1307/bizcord/

## Hashicorp Vault 

In order to use the gateway, follow these steps:

1. Setup Hashicorp vault in the ```bizcord``` project.
2. Copy the application token.
3. Create .env file, with attributes from ```.env.example```
4. Insert the application token and vault address.

**Note:** For local development use http://vault:8200 as the vault address.

## Start with Docker Compose

Start the Docker containers

```bash
docker compose up --build -d
```

The gateway is available at `http://localhost:8080`

Start the microservices from the ```bizcord``` project. Follow the README located in the ```bizcord```project, in order to ensure correct setup. 
The gateway does not start your microservice, database or message broker.

## Authentication and authorization
Run in the following order:
1. Open Swagger at `http://localhost:8080/swagger`
2. Run the POST method ```/faucet```, with the following claims:

**Note:** Insert the channel ID inside the ```sub```claim. 
```json
{
  "claims": {
    "role": "channel",
    "scope": "channel",
    "sub": "insert-channel-id-here"
  }
}
```
3. Copy the returned value, this is your bearer token. 

4. We suggest that you use ```Postman```, when calling through the gateway.

**Example:** 

Call this GET method at http://localhost:8080/channel/ById

Set the following properties:
- Go to the ```Authorization```tab, and set the bearer token.
- Go to the ```Headers``` tab, then insert ```ChannelId``` under ```Key``` and paste a valid ```ChannelId``` under the ```Value```.

This should let you call the GET endpoint from the ```bizcord``` project and returns the channel corresponding to the given ID.
