// config/environment.ts
export const config = {
  apiBaseUrl: process.env.REACT_APP_API_BASE_URL || '/api',
  signalRUrl: process.env.REACT_APP_SIGNALR_URL || '/hubs',
  enableRealTime: process.env.REACT_APP_ENABLE_REALTIME === 'true',
  pollingInterval: parseInt(process.env.REACT_APP_POLLING_INTERVAL || '30000'),
  environment: process.env.NODE_ENV || 'development'
}

// Build Configuration
// package.json scripts
{
  "scripts": {
    "build": "react-scripts build",
    "build:staging": "REACT_APP_API_BASE_URL=https://staging-api.example.com npm run build",
    "build:production": "REACT_APP_API_BASE_URL=https://api.example.com npm run build",
    "analyze": "npm run build && npx bundle-analyzer build/static/js/*.js"
  }
}

// Docker Configuration
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
