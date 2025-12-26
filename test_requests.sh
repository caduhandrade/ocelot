#!/bin/bash

# URLs
GATEWAY_URL="https://estudos-ocelot-ocelot-uk8ltf-a1acb9-103-199-184-68.traefik.me"

echo "=== INICIANDO TESTES ==="
echo "URL do Gateway: $GATEWAY_URL"

# 0. Teste de Conectividade (Health Check)
echo -e "\n0. Testando Health Check (/)..."
curl -vk "$GATEWAY_URL/"
echo "----------------------------------------"

# 1. Obter Token
echo -e "\n1. Obtendo Token..."
# Usando -v para debug detalhado se falhar
RESPONSE=$(curl -vk -X POST "$GATEWAY_URL/auth/token" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin"}' 2>&1)

# Separar corpo da resposta dos logs do verbose (hack simples para bash)
BODY=$(echo "$RESPONSE" | grep -v "^*" | grep -v "^>" | grep -v "^<" | grep "{")

echo -e "\nResposta Bruta do CURL:"
echo "$RESPONSE"
echo "----------------------------------------"

if [[ $RESPONSE == *"Connection refused"* ]] || [[ $RESPONSE == *"Could not resolve host"* ]]; then
    echo "❌ FALHA CRÍTICA DE CONEXÃO. Verifique se a URL está correta e o serviço está rodando."
    exit 1
fi

# Tentar extrair token do corpo (se houver corpo JSON)
TOKEN=""
REFRESH_TOKEN=""

if [ ! -z "$BODY" ]; then
    if command -v jq &> /dev/null; then
        TOKEN=$(echo $BODY | jq -r '.token')
        REFRESH_TOKEN=$(echo $BODY | jq -r '.refreshToken')
    else
        TOKEN=$(echo $BODY | grep -o '"token":"[^"]*' | cut -d'"' -f4)
        REFRESH_TOKEN=$(echo $BODY | grep -o '"refreshToken":"[^"]*' | cut -d'"' -f4)
    fi
fi

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    echo "❌ Erro ao obter token."
    exit 1
fi

echo "✅ Token obtido: ${TOKEN:0:20}..."
echo "----------------------------------------"

# 2. Testar WebApi1 via Gateway
echo -e "\n2. Chamando WebApi1 via Gateway (/api1)..."
curl -ik -X GET "$GATEWAY_URL/api1" \
  -H "Authorization: Bearer $TOKEN"
echo -e "\n----------------------------------------"

# 3. Testar WebApi2 via Gateway
echo -e "\n3. Chamando WebApi2 via Gateway (/api2)..."
curl -i -X GET "$GATEWAY_URL/api2" \
  -H "Authorization: Bearer $TOKEN"
echo -e "\n----------------------------------------"

# 4. Testar Refresh Token
echo -e "\n4. Testando Refresh Token..."
curl -i -X POST "$GATEWAY_URL/auth/refresh-token" \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}"
echo -e "\n----------------------------------------"
