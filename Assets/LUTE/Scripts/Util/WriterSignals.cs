namespace LoGaCulture.LUTE
{
    public class WriterSignals
    {
        public static event WriterClickHandler OneWriterClick;
        public delegate void WriterClickHandler();

        public static void WriterClick() { OneWriterClick?.Invoke(); }
    }
}
