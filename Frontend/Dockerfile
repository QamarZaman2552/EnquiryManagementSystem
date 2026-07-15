FROM node:22-alpine AS build
WORKDIR /app
COPY Enquiryapp/package*.json .
RUN npm ci
COPY Enquiryapp/ .
RUN npm run build -- --configuration production

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/Enquiryapp/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
