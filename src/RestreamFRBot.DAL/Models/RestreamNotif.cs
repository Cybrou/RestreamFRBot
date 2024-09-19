namespace RestreamFRBot.DAL.Models
{
    public class RestreamNotif
    {
        public int RestreamModuleId { get; set; }

        public string Guid { get; set; } = "";

        public RestreamModule? RestreamModule { get; set; }

        public DateTime SentDate
        {
            get
            {
                return DateTime.UnixEpoch.AddSeconds(InternalSentDate);
            }
            set
            {
                InternalSentDate = (int)(value - DateTime.UnixEpoch).TotalSeconds;
            }
        }

        public int InternalSentDate { get; set; }
    }
}
