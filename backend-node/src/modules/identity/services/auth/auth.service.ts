import { Injectable, ConflictException } from '@nestjs/common';
import { PrismaService } from '../../../../common/prisma/prisma.service';
import { JwtService } from '@nestjs/jwt';
import * as bcrypt from 'bcrypt';
import { RegisterDto } from '../../dtos/register.dto';
import * as crypto from 'crypto';

@Injectable()
export class AuthService {
  constructor(
    private prisma: PrismaService,
    private jwtService: JwtService,
  ) {}

  async validateUser(email: string, pass: string): Promise<any> {
    const user = await this.prisma.users.findUnique({ where: { email } });
    if (user && (await bcrypt.compare(pass, user.password_hash))) {
      const { password_hash, ...result } = user;
      return result;
    }
    return null;
  }

  async login(user: any) {
    const payload = { email: user.email, sub: user.id };
    return {
      accessToken: this.jwtService.sign(payload),
      refreshToken: crypto.randomBytes(40).toString('hex'),
    };
  }

  async register(dto: RegisterDto) {
    const existingUser = await this.prisma.users.findUnique({
      where: { email: dto.email },
    });

    if (existingUser) {
      throw new ConflictException('Email is already registered');
    }

    const salt = await bcrypt.genSalt(10);
    const passwordHash = await bcrypt.hash(dto.password, salt);

    const nameParts = dto.fullName.trim().split(' ');
    const lastName = nameParts.length > 1 ? nameParts.pop() || '' : '';
    const firstName = nameParts.join(' ') || dto.fullName;

    const user = await this.prisma.users.create({
      data: {
        id: crypto.randomUUID(),
        email: dto.email,
        password_hash: passwordHash,
        first_name: firstName,
        last_name: lastName,
        bio: dto.bio,
        status: 1, // 1 = Active
        created_at: new Date(),
      },
    });

    return {
      userId: user.id,
      email: user.email,
      fullName: `${user.first_name} ${user.last_name}`.trim(),
    };
  }
}
