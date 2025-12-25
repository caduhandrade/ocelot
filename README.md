# Ocelot Microservices

Este projeto contém 3 microsserviços configurados com .NET 10 e Docker:

1. **WebApi1**: Serviço simples retornando "Hello World 1".
2. **WebApi2**: Serviço simples retornando "Hello World 2".
3. **ApiGateway**: Gateway Ocelot com Autenticação JWT (Endpoints de Token e Refresh Token).

## Estrutura

- `ApiGateway/`: Contém o gateway Ocelot e lógica de autenticação.
- `WebApi1/`: Minimal API 1.
- `WebApi2/`: Minimal API 2.

## Como Executar com Docker

Certifique-se de estar na pasta raiz `OCELOT`.

### 1. Criar Rede Docker

Crie uma rede para os containers se comunicarem:

```bash
docker network create ocelot-net
```

### 2. Construir e Rodar WebApi1

```bash
docker build -t webapi1 -f WebApi1/Dockerfile .
docker run -d --name webapi1 --network ocelot-net webapi1
```

### 3. Construir e Rodar WebApi2

```bash
docker build -t webapi2 -f WebApi2/Dockerfile .
docker run -d --name webapi2 --network ocelot-net webapi2
```

### 4. Construir e Rodar ApiGateway

```bash
docker build -t apigateway -f ApiGateway/Dockerfile .
docker run -d -p 5000:8080 --name apigateway --network ocelot-net apigateway
```

**Nota:** O ApiGateway expõe a porta 8080 internamente, mapeamos para 5000 no host.

## Testando

### 1. Obter Token (Autenticação)

Faça um POST para `http://localhost:5000/auth/token`:

```json
POST /auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "admin"
}
```

Resposta esperada: JSON com `token` e `refreshToken`.

### 2. Acessar WebApi1 via Gateway

Faça um GET para `http://localhost:5000/api1` com o header Authorization:

```
GET /api1
Authorization: Bearer <SEU_TOKEN>
```

Resposta esperada: "Hello World 1"

### 3. Acessar WebApi2 via Gateway

Faça um GET para `http://localhost:5000/api2` com o header Authorization:

```
GET /api2
Authorization: Bearer <SEU_TOKEN>
```

Resposta esperada: "Hello World 2"

### 4. Refresh Token

Faça um POST para `http://localhost:5000/auth/refresh-token`:

```json
POST /auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "<SEU_REFRESH_TOKEN>"
}
```
