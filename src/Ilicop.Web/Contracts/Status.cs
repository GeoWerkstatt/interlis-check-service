namespace Geowerkstatt.Ilicop.Web.Contracts
{
    /// <summary>
    /// The validator job statuses.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The job is scheduled to be executed.
        /// </summary>
        Enqueued,

        /// <summary>
        /// The job is processing.
        /// </summary>
        Processing,

        /// <summary>
        /// The job completed without errors.
        /// </summary>
        Completed,

        /// <summary>
        /// The job completed with errors.
        /// </summary>
        CompletedWithErrors,

        /// <summary>
        /// The job failed.
        /// </summary>
        Failed,
    }
}
