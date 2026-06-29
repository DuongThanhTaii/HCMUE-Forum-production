import { IQueryHandler, QueryHandler } from '@nestjs/cqrs';
import { PrismaService } from '../../../common/prisma/prisma.service';
import { NotFoundException } from '@nestjs/common';

export class GetPostByIdQuery {
  constructor(public readonly id: string) {}
}

@QueryHandler(GetPostByIdQuery)
export class GetPostByIdHandler implements IQueryHandler<GetPostByIdQuery> {
  constructor(private readonly prisma: PrismaService) {}

  async execute(query: GetPostByIdQuery) {
    const post = await this.prisma.posts.findUnique({
      where: { id: query.id },
    });
    
    if (!post) {
      throw new NotFoundException('Post not found');
    }

    let authorName;
    let categoryName;

    if (post.author_id) {
      const user = await this.prisma.users.findUnique({ where: { id: post.author_id } });
      authorName = user ? `${user.last_name} ${user.first_name}`.trim() : undefined;
    }

    if (post.category_id) {
      const cat = await this.prisma.categories.findUnique({ where: { id: post.category_id } });
      categoryName = cat ? cat.name : undefined;
    }

    return {
      ...post,
      authorName,
      categoryName,
    };
  }
}
