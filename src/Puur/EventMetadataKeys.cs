namespace Puur
{
    public static class EventMetadataKeys
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public const string Id = "event-id";

        /// <summary>
        /// The CLR type of the event, used to deserialize it from the store.
        /// </summary>
        public const string ClrType = "event-clr-type";

        /// <summary>
        /// The date and time when the event was persisted.
        /// </summary>
        public const string Timestamp = "event-timestamp";

        /// <summary>
        /// The owner of the event.
        /// </summary>
        public const string OwnerId = "event-owner-id";

        /// <summary>
        /// The version of the owner of the event after the event was generated.
        /// </summary>
        public const string OwnerVersion = "event-owner-version";

        /// <summary>
        /// The type name of the owner of the event. The aggregate root that raised the event.
        /// </summary>
        public const string Owner = "event-owner-name";
    }
}