import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  const posts = await prisma.posts.findMany({
    select: {
      id: true,
      title: true,
      status: true,
      author_id: true,
      created_at: true,
    },
    orderBy: { created_at: 'desc' },
    take: 5
  });
  console.log(JSON.stringify(posts, null, 2));
}

main()
  .catch(e => console.error(e))
  .finally(() => prisma.$disconnect());
