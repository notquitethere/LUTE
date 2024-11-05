namespace LoGaCulture.LUTE
{
    public class WriterSignals
    {
        public static event WriterClickHandler OneWriterClick;
        public delegate void WriterClickHandler();

        public static void WriterClick() { OneWriterClick?.Invoke(); }

        /// <summary>
        /// WriterState signal. Sent when the writer changes state.
        /// </summary>
        public static event WriterStateHandler OnWriterState;
        public delegate void WriterStateHandler(TextWriter writer, WriterState writerState);
        public static void DoWriterState(TextWriter writer, WriterState writerState)
        {
            if (OnWriterState != null) OnWriterState(writer, writerState);
        }
    }
}
