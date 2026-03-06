namespace Haven.Domain.DTO
{
    public class CreateMusterPointRequest
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMetres { get; set; }
        public int TotalMembers { get; set; }
    }
}
