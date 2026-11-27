# Build stage: compile the Angular app.
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Angular settings are compiled in, so the API URL is a build argument.
# The Google client ID comes from src/environments/environment.ts.
ARG API_BASE_URL=http://localhost:5019
RUN sed -i "s|apiBaseUrl: '[^']*'|apiBaseUrl: '${API_BASE_URL}'|" src/environments/environment.ts \
 && npx ng build --configuration production

# Runtime stage: serve the static files with nginx.
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/interface-front/browser /usr/share/nginx/html
EXPOSE 80
