import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  console.log('Recounting comments for all posts...');
  const posts = await prisma.posts.findMany();
  
  for (const post of posts) {
    const commentCount = await prisma.comments.count({
      where: {
        post_id: post.id,
        is_deleted: false,
      },
    });

    if (post.comment_count !== commentCount) {
      console.log(`Updating post ${post.id}: ${post.comment_count} -> ${commentCount}`);
      await prisma.posts.update({
        where: { id: post.id },
        data: { comment_count: commentCount },
      });
    }
  }

  console.log('Done.');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
