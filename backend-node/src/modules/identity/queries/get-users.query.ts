export class GetUsersQuery {
  constructor(
    public readonly skip?: number,
    public readonly take?: number,
  ) {}
}
