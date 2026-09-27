#!/bin/bash
# Script idempotente para configurar a VM Ubuntu 22.04 com a TechStore API.
# Execute como root ou com sudo na VM.
set -e

echo "=== Configurando VM para TechStore API ==="

# 1. Instalar ASP.NET Core 8 Runtime (pacote do repositório padrão do Ubuntu)
if ! dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.AspNetCore.App 8."; then
    echo "Instalando ASP.NET Core 8 Runtime..."
    apt-get update
    apt-get install -y aspnetcore-runtime-8.0
    echo "ASP.NET Core 8 instalado com sucesso."
else
    echo "ASP.NET Core 8 Runtime já instalado."
fi

# 2. Instalar Nginx (idempotente)
if ! command -v nginx &> /dev/null; then
    echo "Instalando Nginx..."
    apt-get install -y nginx
else
    echo "Nginx já instalado."
fi

# 3. Instalar Certbot para HTTPS (idempotente)
if ! command -v certbot &> /dev/null; then
    echo "Instalando Certbot..."
    apt-get install -y certbot python3-certbot-nginx
else
    echo "Certbot já instalado."
fi

# 4. Criar diretório da aplicação
echo "Criando diretório /opt/techstore-api..."
mkdir -p /opt/techstore-api
chown www-data:www-data /opt/techstore-api

# 5. Copiar configuração do Nginx
echo "Configurando Nginx..."
cp /tmp/nginx-techstore.conf /etc/nginx/sites-available/techstore
ln -sf /etc/nginx/sites-available/techstore /etc/nginx/sites-enabled/techstore
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl reload nginx

# 6. Copiar e habilitar serviço systemd
echo "Configurando serviço systemd..."
cp /tmp/techstore-api.service /etc/systemd/system/techstore-api.service
systemctl daemon-reload
systemctl enable techstore-api

echo ""
echo "=== Configuração concluída! ==="
echo ""
echo "Próximos passos:"
echo "  1. Copie os artefatos para /opt/techstore-api/"
echo "     scp techstore-api.tar.gz azureuser@techstore-matheus.spaincentral.cloudapp.azure.com:/tmp/"
echo "     ssh azureuser@<vm> 'sudo tar -xzf /tmp/techstore-api.tar.gz -C /opt/techstore-api/'"
echo ""
echo "  2. Inicie o serviço:"
echo "     sudo systemctl start techstore-api"
echo "     sudo systemctl status techstore-api"
echo ""
echo "  3. Configure HTTPS com Certbot:"
echo "     sudo certbot --nginx -d techstore-matheus.spaincentral.cloudapp.azure.com"
echo ""
echo "  4. Teste:"
echo "     curl http://localhost/health"
