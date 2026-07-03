export class SearchChatUsersQuery {
  constructor(
    public readonly q: string,
    public readonly take: number,
  ) {}
}
