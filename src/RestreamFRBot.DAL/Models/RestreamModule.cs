namespace RestreamFRBot.DAL.Models
{
    public class RestreamModule
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<RestreamNotif>? RestreamNotifs { get; set; }
    }
}
