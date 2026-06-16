# ===== Stage 1: build =====
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build          # outputs the static site to /app/dist

# ===== Stage 2: serve =====
FROM nginx:alpine AS serve
# Replace nginx's default site with our SPA config (handles client-side routing)
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Copy ONLY the built static files from the build stage
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
# nginx's base image already starts the server — no ENTRYPOINT/CMD needed
