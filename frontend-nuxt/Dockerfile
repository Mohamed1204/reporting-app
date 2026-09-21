# ===== Stage 1: build =====
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build          # outputs a self-contained server to /app/.output

# ===== Stage 2: run =====
# node, not nginx. The SPA next door builds to static files any web server can
# hand out; this one builds to a *server* that renders pages per request, so
# Node has to be here at runtime.
FROM node:22-alpine AS run
WORKDIR /app
ENV NODE_ENV=production

# Only .output — Nitro bundles its dependencies into it, so there is no
# node_modules to install or copy in this stage.
COPY --from=build /app/.output ./.output

# Nitro reads these at startup, not at build. That is why this image needs no
# envsubst step: the same image runs against any API by changing the variable,
# where the SPA has to rewrite its nginx config on boot because its config was
# baked in at build time.
ENV NITRO_PORT=3000 NITRO_HOST=0.0.0.0
EXPOSE 3000

CMD ["node", ".output/server/index.mjs"]
