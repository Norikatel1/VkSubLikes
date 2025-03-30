namespace WinFormsApp1.Models
{
    public class GetLikeApiResponse
    {
        public GetLikeResponseData Response { get; set; }
    }

    public class GetLikeResponseData
    {
        public int Count { get; set; }
        public List<User> Items { get; set; }
        public List<int> Reactions { get; set; }
        public string ReactionSetId { get; set; }
        public List<ReactionSet> ReactionSets { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public bool Can__AccessClosed { get; set; }
        public bool Is_Closed { get; set; }
    }

    public class ReactionSet
    {
        public string Id { get; set; }
        public List<ReactionItem> Items { get; set; }
    }

    public class ReactionItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ReactionAsset Asset { get; set; }
        public TitleColor TitleColor { get; set; }
    }

    public class ReactionAsset
    {
        public string AnimationUrl { get; set; }
        public List<Image> Images { get; set; }
        public AssetTitle Title { get; set; }
    }

    public class Image
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class AssetTitle
    {
        public TitleColor Color { get; set; }
    }

    public class TitleColor
    {
        public ColorSettings Light { get; set; }
        public ColorSettings Dark { get; set; }
    }

    public class ColorSettings
    {
        public string Foreground { get; set; }
        public string Background { get; set; }
    }
}
