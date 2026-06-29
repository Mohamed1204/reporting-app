# ===== Stage 1: build =====
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build          # outputs the static site to /app/dist

# ===== Stage 2: serve =====
FROM nginx:alpine AS serve
# Our SPA config as a template; nginx's entrypoint runs envsubst on files in
# /etc/nginx/templates/ at startup, injecting ${BACKEND_URL} into the final config.
COPY default.conf.template /etc/nginx/templates/default.conf.template
# Copy ONLY the built static files from the build stage
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
# nginx's base image already starts the server — no ENTRYPOINT/CMD needed
