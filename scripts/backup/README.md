# Backup cơ sở dữ liệu UniHub

## Định dạng xuất ra (mặc định)

| DB | File | Đọc được? | Restore |
|----|------|-----------|---------|
| **PostgreSQL** | `postgres_unihub.sql` | Có — file SQL text | `psql -f postgres_unihub.sql` |
| **MongoDB** | `mongodb_*_json/collections/*.json` | Có — JSON array | `mongoimport` từng collection |

Trước đây script dùng `.dump` (Postgres) và `.bson.gz` (Mongo) — **tốt để restore**, khó mở xem. Giờ **mặc định là SQL + JSON**.

### So sánh nhanh

- **SQL / JSON** — backup “kiểu file để xem, chỉnh, chia sẻ”: mở Notepad, VS Code, import Excel/script.
- **Dump / BSON** — backup “kiểu production restore”: nhỏ, nhanh, dùng `pg_restore` / `mongorestore`.

## Chạy backup

```powershell
cd E:\HCMUE-Forum
.\scripts\backup\backup-databases.ps1
```

Kết quả: `backups\export_YYYYMMDD_HHMMSS\`

```
export_20260516_225006/
  postgres_unihub.sql          ← CREATE TABLE, INSERT...
  mongodb_unihub_db_dev_json/
    _index.json
    collections/
      user_action_logs.json    ← [ {...}, {...} ]
  backup-manifest.json
  README.txt
```

### Chỉ định thư mục

```powershell
.\scripts\backup\backup-databases.ps1 -OutputDir "D:\UniHub-backups\hom-nay"
```

### Định dạng restore (BSON + pg_dump binary)

```powershell
.\scripts\backup\backup-databases.ps1 -PostgresFormat Dump -MongoFormat Bson
```

## Điều kiện

Docker containers `unihub-postgres` và `unihub-mongodb` đang chạy (khuyến nghị):

```powershell
docker compose --env-file .env.docker ps
```

Cấu hình đọc từ `.env.docker` + `appsettings.Development.json` (tên DB Mongo: thường **`unihub_db_dev`**).

Thư mục `backups/` đã **gitignore** — không commit file backup.

## Khôi phục (tham khảo)

**PostgreSQL (SQL):**

```powershell
$env:PGPASSWORD = "unihub_dev_2026"
psql -h 127.0.0.1 -p 5433 -U unihub -d unihub -f backups\export_xxx\postgres_unihub.sql
```

**MongoDB (JSON — từng collection):**

```powershell
mongoimport --uri="mongodb://unihub:PASSWORD@127.0.0.1:27018/?authSource=admin" `
  --db=unihub_db_dev --collection=user_action_logs `
  --file=backups\export_xxx\mongodb_unihub_db_dev_json\collections\user_action_logs.json `
  --jsonArray --drop
```
