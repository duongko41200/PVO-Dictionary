using System;

namespace pvo_dictionary_api.Models
{
    public class BaseModel
    {
        public DateTime created_date { get; set; } = DateTime.Now;
        public DateTime updated_date { get; set; }
    }
}
