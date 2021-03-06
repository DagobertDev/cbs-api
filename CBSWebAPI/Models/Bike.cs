using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CBSWebAPI.Models
{
	public class Bike
	{
		public Bike(long id, long communityId, string name, GeoPosition? position)
		{
			Id = id;
			CommunityId = communityId;
			Name = name;
			Position = position;
		}

		public Bike(long communityId, string name)
		{
			CommunityId = communityId;
			Name = name;
		}

		public long Id { get; set; }

		[MaxLength(32)]
		public string Name { get; set; }
		
		[ForeignKey(nameof(Community))]
		public long CommunityId { get; set; }
		public Community Community { get; set; } = null!;
		
		[ForeignKey(nameof(User))]
		public string? UserId { get; set; }
		public string? User { get; set; }
		public GeoPosition? Position { get; set; }
	}

	public record BikeWrite(long CommunityId, [MaxLength(32)] string Name, GeoPosition? Position);

	public record BikeRead(long Id, long CommunityId, string Name, string? UserId, GeoPosition? Position)
	{
		public static BikeRead From(Bike bike) => new(bike.Id, bike.CommunityId, bike.Name, bike.UserId, bike.Position);
	}
}
