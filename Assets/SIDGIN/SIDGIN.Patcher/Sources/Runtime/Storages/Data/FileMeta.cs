namespace SIDGIN.Patcher.Storages
{
    public class FileMeta
    {
        public readonly string id;
        public readonly string link;
        public readonly string name;
        public readonly long size;
        public FileMeta(string id, string link, string name, long size)
        {
            this.id = id;
            this.link = link;
            this.name = name;
            this.size = size;
        }
    }
}
