# Build stage: compile the Angular app.
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Angular settings are compiled in, so the API URL and Google client ID are passed as build arguments.
ARG API_BASE_URL=http://localhost:5019
ARG GOOGLE_CLIENT_ID=
RUN sed -i \
      -e "s|apiBaseUrl: '[^']*'|apiBaseUrl: '${API_BASE_URL}'|" \
      -e "s|googleClientId: '[^']*'|googleClientId: '${GOOGLE_CLIENT_ID}'|" \
      src/environments/environment.ts \
 && npx ng build --configuration production

# Runtime stage: serve the static files with nginx.
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/interface-front/browser /usr/share/nginx/html
EXPOSE 80
