# ================================================================
# NetSentinel.Web — Dockerfile para produção
# Next.js 15 + React 19
# ================================================================

FROM node:20-alpine AS base

# ── Estágio 1: Dependências ────────────────────────────────────
FROM base AS deps
WORKDIR /app

COPY package*.json ./
RUN npm ci

# ── Estágio 2: Build ────────────────────────────────────────────
FROM base AS builder
WORKDIR /app

ARG NEXT_PUBLIC_API_URL=http://localhost:5149
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Cria pasta public vazia se não existir (evita erro no COPY)
RUN mkdir -p public

RUN npm run build

# ── Estágio 3: Produção ─────────────────────────────────────────
FROM base AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# Cria usuário não-root
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

# Cria pasta public vazia
RUN mkdir -p public

# Copia apenas o necessário do build
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static
COPY --from=builder --chown=nextjs:nodejs /app/public ./public

USER nextjs

EXPOSE 3000

ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

CMD ["node", "server.js"]