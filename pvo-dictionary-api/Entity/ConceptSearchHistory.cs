using System.ComponentModel.DataAnnotations;

namespace pvo_dictionary_api.Models
{
    public class ConceptSearchHistory : BaseModel
    {
        [Key]
        public int Id { get; set; }
        public int user_id { get; set; }
        public int dictionary_id { get; set; }
        public string list_concept_search { get; set; }
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
    }
}
