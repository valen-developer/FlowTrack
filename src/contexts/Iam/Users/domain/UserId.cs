namespace FlowTrack.Iam.Users.Domain
{
    internal record UserId : Uuid
    {
        public UserId(string value)
            : base(value, new InvalidUserId()) { }
    }
}
