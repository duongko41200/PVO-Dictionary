using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pvo_dictionary_api.Models
{
    public class ExampleRelationship : BaseModel
    {
        [Key]
        public int example_relationship_id { get; set; }
        public int dictionary_id { get; set; }
        public int concept_id { get; set; }
        public int example_id { get; set; }
        public int example_link_id { get; set; }
        [ForeignKey("example_link_id")]
        public ExampleLink ExampleLink { get; set; }

      
    }
}
