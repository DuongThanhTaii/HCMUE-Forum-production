const { PrismaClient } = require('@prisma/client');
require('dotenv').config();

const prisma = new PrismaClient();

async function main() {
  const users = await prisma.users.findMany({
    select: {
      id: true,
      email: true,
      role: true,
    }
  });
  console.log("USERS:", JSON.stringify(users, null, 2));

  const posts = await prisma.posts.findMany({
    select: {
      id: true,
      title: true,
      status: true,
      created_at: true,
    },
    orderBy: { created_at: 'desc' },
    take: 5
  });
  console.log("POSTS:", JSON.stringify(posts, null, 2));
}

main()
  .catch(e => console.error(e))
  .finally(() => prisma.$disconnect());
