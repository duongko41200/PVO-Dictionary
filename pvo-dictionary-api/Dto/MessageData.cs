namespace pvo_dictionary_api.Dto
{
    public class MessageData
    {
        public int Status { get; set; } = 1;
        public object Data { get; set; }
        public string Code { get; set; } = "Success";
        public int ErrorCode { get; set; }
       
    
        public string Message { get; set;}
    }
}
