const { Client } = require('pg');

const client = new Client({
  connectionString: 'postgresql://neondb_owner:npg_MmvVXYou6a2n@ep-green-dust-aok0et2e.c-2.ap-southeast-1.aws.neon.tech/unihub?sslmode=require&channel_binding=require'
});

async function main() {
  await client.connect();

  console.log('Recounting comments for all posts...');

  const query = `
    UPDATE forum.posts
    SET comment_count = (
      SELECT COUNT(*)
      FROM forum.comments
      WHERE forum.comments.post_id = forum.posts.id
        AND forum.comments.is_deleted = false
    );
  `;

  await client.query(query);

  console.log('Done.');
  await client.end();
}

main().catch(console.error);
