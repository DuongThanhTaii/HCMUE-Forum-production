# Build stage
FROM node:20-alpine AS builder

# Set working directory
WORKDIR /usr/src/app

# Copy package files
COPY package*.json ./

# Copy prisma schema
COPY prisma ./prisma/

# Install all dependencies (including devDependencies)
RUN npm install

# Copy source code
COPY . .

# Generate Prisma client
RUN npx prisma generate

# Build the application
RUN npm run build

# Production stage
FROM node:20-alpine AS production

# Set node environment to production
ENV NODE_ENV production

# Set working directory
WORKDIR /usr/src/app

# Copy package files
COPY package*.json ./

# Copy prisma schema
COPY prisma ./prisma/

# Install only production dependencies
RUN npm ci --only=production && npm cache clean --force

# Generate Prisma client for production
RUN npx prisma generate

# Copy the built artifacts from the builder stage
COPY --from=builder /usr/src/app/dist ./dist

# Expose port (Render automatically assigns PORT)
EXPOSE 3000

# Start the application
CMD ["npm", "run", "start:prod"]
