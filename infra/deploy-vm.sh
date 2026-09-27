#!/bin/bash
# Script idempotente para configurar a VM Ubuntu com a TechStore API.
# Execute como root ou com sudo na VM.
set -e

echo "=== Configurando VM para TechStore API ==="

# 1. Instalar .NET 8 Runtime (idempotente)
if ! dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.AspNetCore.App 8."; then
    echo "Instalando .NET 8 Runtime..."
    wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt-get update
    apt-get install -y aspnetcore-runtime-8.0
    echo ".NET 8 instalado com sucesso."
else
    echo ".NET 8 Runtime já instalado."
fi

# 2. Instalar Nginx (idempotente)
if ! command -v nginx &> /dev/null; then
    echo "Instalando Nginx..."
    apt-get install -y nginx
else
    echo "Nginx já instalado."
fi

# 3. Criar diretório da aplicação
echo "Criando diretório /opt/techstore-api..."
mkdir -p /opt/techstore-api
chown www-data:www-data /opt/techstore-api

# 4. Copiar configuração do Nginx
echo "Configurando Nginx..."
cp /tmp/nginx-techstore.conf /etc/nginx/sites-available/techstore
ln -sf /etc/nginx/sites-available/techstore /etc/nginx/sites-enabled/techstore
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl reload nginx

# 5. Copiar e habilitar serviço systemd
echo "Configurando serviço systemd..."
cp /tmp/techstore-api.service /etc/systemd/system/techstore-api.service
systemctl daemon-reload
systemctl enable techstore-api

echo ""
echo "=== Configuração concluída! ==="
echo ""
echo "Próximos passos:"
echo "  1. Copie os artefatos para /opt/techstore-api/"
echo "     scp -r artifacts/* <user>@<vm-ip>:/opt/techstore-api/"
echo ""
echo "  2. Edite as variáveis de ambiente no service:"
echo "     sudo nano /etc/systemd/system/techstore-api.service"
echo ""
echo "  3. Recarregue e inicie o serviço:"
echo "     sudo systemctl daemon-reload"
echo "     sudo systemctl start techstore-api"
echo "     sudo systemctl status techstore-api"
echo ""
echo "  4. Teste:"
echo "     curl http://localhost/health"
