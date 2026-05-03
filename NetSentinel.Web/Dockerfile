# ESTÁGIO 1: Build (Compilação do JS)
FROM node:20-alpine AS build-step
WORKDIR /app

# Copia os ficheiros de pacotes e instala as dependências
COPY package*.json ./
RUN npm install

# Copia o código fonte e gera a versão de produção
COPY . .
RUN npm run build

# ESTÁGIO 2: Runtime (Servidor Web Nginx)
FROM nginx:stable-alpine

# CORREÇÃO: O Next.js com exportação estática gera a pasta 'out'
COPY --from=build-step /app/out /usr/share/nginx/html

# Expõe a porta 80 (padrão HTTP)
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]