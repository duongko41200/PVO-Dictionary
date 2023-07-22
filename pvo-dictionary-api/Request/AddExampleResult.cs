using pvo_dictionary_api.Models;

namespace pvo_dictionary_api.Request
{
    public class AddExampleResult
    {
        public Example AddedExample { get; set; }
        public ExampleRelationship AddedExampleRelationship { get; set; }
    }

}
