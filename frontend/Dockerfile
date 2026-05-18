FROM node:lts-alpine3.23

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

EXPOSE 5173

CMD ["sh", "-c", "npm run build && npm run preview -- --port ${PORT:-5173}"]