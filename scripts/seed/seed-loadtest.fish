#!/usr/bin/env fish

set -l project_root /home/thinhdev/Desktop/ForumNET/HCMUE-Forum
set -l env_file "$project_root/.env.docker"
set -g SEED_ENV_FILE "$env_file"

if not test -f "$env_file"
    echo "Missing .env.docker. Create it from .env.docker.example first."
    exit 1
end

set -l rows 300000
if test (count $argv) -ge 1
    set rows $argv[1]
end

function read_env --argument-names key default_value
    set -l line (grep -E "^$key=" "$SEED_ENV_FILE" | tail -n 1)
    if test -z "$line"
        echo $default_value
        return
    end

    set -l value (string replace -r "^$key=" "" -- "$line")
    if test -z "$value"
        echo $default_value
    else
        echo $value
    end
end

set -l POSTGRES_USER (read_env POSTGRES_USER unihub)
set -l POSTGRES_PASSWORD (read_env POSTGRES_PASSWORD unihub_dev_2026)
set -l POSTGRES_DB (read_env POSTGRES_DB unihub)
set -l MONGO_ROOT_USER (read_env MONGO_ROOT_USER unihub)
set -l MONGO_ROOT_PASSWORD (read_env MONGO_ROOT_PASSWORD unihub_dev_2026)
set -l MONGO_DB (read_env MONGO_DB unihub)
set -l REDIS_PASSWORD (read_env REDIS_PASSWORD unihub_dev_2026)

echo "Seeding loadtest dataset with $rows records per datastore..."

# PostgreSQL
set -l pg_sql "
CREATE TABLE IF NOT EXISTS loadtest_events (
    id BIGINT PRIMARY KEY,
    user_id BIGINT NOT NULL,
    category VARCHAR(50) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
TRUNCATE TABLE loadtest_events;
INSERT INTO loadtest_events (id, user_id, category, payload, created_at)
SELECT i,
       (i % 10000) + 1,
       CASE WHEN i % 3 = 0 THEN 'forum' WHEN i % 3 = 1 THEN 'learning' ELSE 'career' END,
       jsonb_build_object('score', i % 100, 'title', concat('event-', i), 'active', (i % 2 = 0)),
       NOW() - make_interval(secs => (i % 86400))
FROM generate_series(1, $rows) AS g(i);
CREATE INDEX IF NOT EXISTS idx_loadtest_events_user_id ON loadtest_events(user_id);
CREATE INDEX IF NOT EXISTS idx_loadtest_events_category ON loadtest_events(category);
"

docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" unihub-postgres \
    psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -v ON_ERROR_STOP=1 -c "$pg_sql" >/tmp/loadtest_pg_seed.log

# MongoDB
set -l mongo_js "
const total = $rows;
const batch = 5000;
const dbName = '$MONGO_DB';
const target = db.getSiblingDB(dbName);
target.loadtest_events.drop();
target.createCollection('loadtest_events');
let inserted = 0;
for (let i = 0; i < total; i += batch) {
  const docs = [];
  const end = Math.min(i + batch, total);
  for (let j = i; j < end; j++) {
    docs.push({
      _id: j + 1,
      userId: (j % 10000) + 1,
      category: (j % 3 === 0) ? 'forum' : (j % 3 === 1 ? 'learning' : 'career'),
    payload: { score: j % 100, title: 'event-' + (j + 1), active: j % 2 === 0 },
      createdAt: new Date(Date.now() - ((j % 86400) * 1000))
    });
  }
  target.loadtest_events.insertMany(docs, { ordered: false });
  inserted += docs.length;
}
print('mongo_inserted=' + inserted);
"

docker exec unihub-mongodb \
    mongosh --quiet -u "$MONGO_ROOT_USER" -p "$MONGO_ROOT_PASSWORD" --authenticationDatabase admin --eval "$mongo_js" >/tmp/loadtest_mongo_seed.log

# Redis
set -l batch_size 100000
set -l total_batches (math -s0 "($rows + $batch_size - 1) / $batch_size")

for batch_index in (seq 0 (math "$total_batches - 1"))
        set -l start (math "$batch_index * $batch_size + 1")
        set -l current_batch_size $batch_size
        if test $batch_index -eq (math "$total_batches - 1")
                set current_batch_size (math "$rows - $batch_index * $batch_size")
        end

        docker exec unihub-redis redis-cli -a "$REDIS_PASSWORD" EVAL "
local s=tonumber(ARGV[1]);
local c=tonumber(ARGV[2]);
for i=s,s+c-1 do
    redis.call('SET','loadtest:user:'..i,'{\\\"score\\\":'..(i%100)..',\\\"active\\\":'..((i%2==0) and 'true' or 'false')..'}')
end
return c
" 0 "$start" "$current_batch_size" >>/tmp/loadtest_redis_seed.log
end

# Verification
set -l pg_count (docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" unihub-postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -t -A -c "SELECT count(*) FROM loadtest_events;")
set -l mongo_count (docker exec unihub-mongodb mongosh --quiet -u "$MONGO_ROOT_USER" -p "$MONGO_ROOT_PASSWORD" --authenticationDatabase admin --eval "db.getSiblingDB('$MONGO_DB').loadtest_events.countDocuments({})")
set -l redis_count (docker exec unihub-redis redis-cli -a "$REDIS_PASSWORD" --scan --pattern "loadtest:user:*" | wc -l)

echo "Done."
echo "PostgreSQL rows: $pg_count"
echo "MongoDB docs:   $mongo_count"
echo "Redis keys:     $redis_count"
