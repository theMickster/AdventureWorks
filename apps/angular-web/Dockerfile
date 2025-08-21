FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx nx build adventureworks-web --configuration=docker

FROM nginxinc/nginx-unprivileged:alpine AS final
COPY --from=build /app/dist/apps/adventureworks-web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 8080
