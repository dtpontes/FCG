namespace FCG.Presentation.Models
{
    public class ResponseObject<T>
    {
        public T? Data { get; private set; } // Allow Data to be nullable
        public IEnumerable<string> Errors { get; private set; } = Array.Empty<string>(); // Initialize Errors to avoid null
        public bool Success { get; private set; }

        private ResponseObject() { }

        public ResponseObject(IEnumerable<string> errors)
        {
            Errors = errors ?? Array.Empty<string>();
            Success = false;
        }

        public ResponseObject(T data)
        {
            Success = true;
            Data = data;
        }

        public static ResponseObject<T> Succeed(T? data = default) // Allow nullable data
        {
            return new ResponseObject<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ResponseObject<T> Fail(IEnumerable<string>? errors = null)
        {
            return new ResponseObject<T>
            {
                Success = false,
                Errors = errors ?? Array.Empty<string>()
            };
        }
    }
}
