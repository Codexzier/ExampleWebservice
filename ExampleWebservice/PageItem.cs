namespace ExampleWebservice
{
    internal class PageItem
    {
        public PageItem(string filename, bool loadData = false)
        {
            this.Filename = filename;
            this.LoadData = loadData;
        }

        public string Filename { get; }
        public bool LoadData { get; }
    }
}