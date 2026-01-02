# 🚀 NGINX Setup on macOS (Homebrew)

## 📌 Install

```bash
brew install nginx
brew services start nginx
```

---

## 📍 Important Paths

| Purpose | Path |
|---------|------|
| Main config file | `/opt/homebrew/etc/nginx/nginx.conf` |
| Sites / servers config | `/opt/homebrew/etc/nginx/servers/` |
| Logs | `/opt/homebrew/var/log/nginx/` |
| PID | `/opt/homebrew/var/run/nginx.pid` |

---

## 📂 Create Servers Directory (if not exists)

```bash
sudo mkdir -p /opt/homebrew/etc/nginx/servers
```

---

## 🛠 Edit main nginx.conf

```bash
sudo nano /opt/homebrew/etc/nginx/nginx.conf
```

Ensure this line exists inside the `http { ... }` block:

```nginx
include /opt/homebrew/etc/nginx/servers/*.conf;
```

---

## ⚙️ Create Blog API Gateway config

```bash
sudo nano /opt/homebrew/etc/nginx/servers/blog-api.conf
```

Paste:

```nginx
# 🚀 Blog Platform - API Gateway
server {
    listen 8095;
    server_name api.blog.local;

    access_log /opt/homebrew/var/log/nginx/blog-api-access.log;
    error_log  /opt/homebrew/var/log/nginx/blog-api-error.log;

    # 🔁 Forward Headers
    proxy_set_header Host              $host;
    proxy_set_header X-Real-IP         $remote_addr;
    proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;

    # ❤️ Health Check
    location /health {
        return 200 "OK\n";
    }

    # 🧩 API Routes
    location /api/auth/ {
        proxy_pass http://localhost:5001;   # IdentityService
    }

    location /api/articles/ {
        proxy_pass http://localhost:5002;   # ArticleService
    }

    # 🖼️ Media (MinIO)
    location /media/ {
        proxy_pass http://localhost:9000;
    }
}
```

---

## 🖥 Add Local Domain to Hosts

```bash
sudo nano /etc/hosts
```

Add:

```
127.0.0.1   api.blog.local
```

---

## 🔄 Restart & Test

```bash
nginx -t
brew services restart nginx
```

Test URLs:

```bash
curl http://api.blog.local:8095/health
curl http://api.blog.local:8095/api/articles?page=1&pageSize=10
curl http://api.blog.local:8095/api/auth
```

---

## 📦 Cheatsheet

```bash
nginx -t                     # Test config
brew services restart nginx  # Restart
brew services stop nginx
brew services start nginx
nginx -s reload              # Reload without stop
```

---

## 🎉 Summary

✔ Routing microservices through 1 domain  
✔ Ready for frontend integration  
✔ Works for local development on macOS  

---

# 🌐 Final Structure

- `api.blog.local:8095/api/auth/...` ➝ IdentityService (5001)
- `api.blog.local:8095/api/articles/...` ➝ ArticleService (5002)
- `api.blog.local:8095/media/...` ➝ MinIO (9000)

---

**Next Steps You Can Do Later**
- Add HTTPS with mkcert
- Docker Compose deployment
- Move same config to Linux server
- Add caching / rate limit

---

💡 _This file is safe to store in your repo at:_  
`docs/nginx-macos-setup.md`

