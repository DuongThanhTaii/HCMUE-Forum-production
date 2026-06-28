const { Client } = require('pg');

async function check() {
  const client = new Client({
    connectionString: 'postgresql://neondb_owner:npg_MmvVXYou6a2n@ep-green-dust-aok0et2e.c-2.ap-southeast-1.aws.neon.tech/neondb?sslmode=require'
  });

  try {
    await client.connect();
    const res = await client.query(`
      SELECT table_schema, table_name 
      FROM information_schema.tables 
      WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
    `);
    console.log("Found tables:", res.rows);
  } catch (err) {
    console.error("Error:", err);
  } finally {
    await client.end();
  }
}

check();
